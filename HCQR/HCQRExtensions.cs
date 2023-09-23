using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HCQR;

/// <summary>
/// When Resolving a IHandler from the DI Container, this enum determines the lifetime of the handler.
/// </summary>
public enum Lifetime
{
	Transient,
	Scoped
}

/// <summary>
/// Contains extension methods for integrating HCQR (Hybrid Command Query Responsibility) 
/// into the ASP.NET Core application pipeline and service registration.
/// </summary>
public static class HCQRExtensions
{
	/// <summary>
	/// Registers the HCQR middleware in the application pipeline.
	/// This method also discovers all types in the entry assembly with the <see cref="MapAttribute"/> and
	/// registers them with the <see cref="RouteMatcher"/>.
	/// </summary>
	/// <param name="app">The application builder instance.</param>
	public static void UseHCQR(this IApplicationBuilder app)
	{
		// Discover types with the MapAttribute and register them with RouteMatcher.
		foreach (var type in Assembly.GetEntryAssembly().GetTypes())
		{
			var mapAttribute = type.GetCustomAttribute<MapAttribute>();
			if (mapAttribute != null)
			{
				RouteMatcher.Register(mapAttribute.Path, type);
			}
		}

		// Register the HCQR middleware in the application pipeline.
		app.UseMiddleware<HCQRMiddleware>();
	}

	/// <summary>
	/// Registers handler types (implementing the <see cref="IHandler"/> interface) from the executing assembly 
	/// as transient services in the dependency injection container.
	/// </summary>
	/// <param name="services">The service collection to add the services to.</param>
	/// <param name="lifetime">The lifetime of the handler.</param>
	public static void AddHCQR(this IServiceCollection services, Lifetime lifetime = Lifetime.Transient)
	{
		// Discover handler types in the executing assembly and register them as transient services.
		foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
		{
			if (lifetime == Lifetime.Transient)
				services.AddTransient(type);
			else if (lifetime == Lifetime.Scoped)
				services.AddScoped(type);
		}
	}
}