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

	internal static bool UsingSwagger { get; set; } = false;
	internal static string SwaggerPath { get; set; } = "/doc/";
	internal static List<Type> Handlers { get; set; } = new List<Type>();

	/// <summary>
	/// Registers the HCQR middleware in the application pipeline.
	/// This method also discovers all types in the entry assembly with the <see cref="MapAttribute"/> and
	/// registers them with the <see cref="RouteMatcher"/>.
	/// </summary>
	/// <param name="app">The application builder instance.</param>
	/// <param name="useSwagger">Whether to use the built-in Swagger UI.</param>
	public static void UseHCQR(this IApplicationBuilder app, bool useSwagger = true)
	{
		UsingSwagger = useSwagger;
		// Register the HCQR middleware in the application pipeline.
		app.UseMiddleware<HCQRMiddleware>();
	}

	/// <summary>
	/// Registers handler types (implementing the <see cref="IHandler"/> interface) from the executing assembly 
	/// as transient services in the dependency injection container.
	/// </summary>
	/// <param name="services">The service collection to add the services to.</param>
	/// <param name="lifetime">The lifetime of the handler.</param>
	/// <param name="assemblies">The assemblies to scan for handlers. If null, the executing assembly is used.</param>
	public static void AddHCQR(this IServiceCollection services, Lifetime lifetime = Lifetime.Transient, Assembly[]? assemblies = null)
	{
		Handlers = new List<Type>();
		if(assemblies != null)
		{
			foreach(var asm in assemblies)
			{
				Handlers.AddRange(asm.GetTypes().Where(t => typeof(IHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract));
			}
		} else
		{
			Assembly? asm = Assembly.GetEntryAssembly();
			if (asm != null)
			{
				Handlers.AddRange(asm.GetTypes().Where(t => typeof(IHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract));
			}
		}

		// Discover types with IHttpMap attributes (like GetAttribute, PostAttribute, etc.) and register them with RouteMatcher.
		foreach (var type in Handlers)
		{
			var httpMapAttribute = type.GetCustomAttributes().OfType<IHttpMap>().FirstOrDefault();
			if (httpMapAttribute != null)
			{
				var httpMethod = httpMapAttribute.GetHttpMethod();
				RouteMatcher.Register(httpMethod, httpMapAttribute.Path, type);
			}
		}

		// Discover handler types in the executing assembly and register them as transient services.
		foreach (var type in Handlers)
		{
			if (lifetime == Lifetime.Transient)
				services.AddTransient(type);
			else if (lifetime == Lifetime.Scoped)
				services.AddScoped(type);
		}
	}
}