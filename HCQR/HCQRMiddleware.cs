﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace HCQR;

/// <summary>
/// Middleware to handle requests based on HCQR (Holistic Command Query Responsibility) pattern.
/// This middleware uses the RouteMatcher to determine if a given HTTP request should be handled 
/// by a specific handler. If a match is found, the handler is invoked and its response is sent back.
/// </summary>
public class HCQRMiddleware
{
	// Delegate to the next middleware in the pipeline.
	private readonly RequestDelegate _next;

	// Dependency injection container to resolve services.
	private readonly IServiceProvider _serviceProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="HCQRMiddleware"/> class.
	/// </summary>
	/// <param name="next">The next middleware in the execution pipeline.</param>
	/// <param name="serviceProvider">Service provider for dependency resolution.</param>
	public HCQRMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
	{
		_next = next;
		_serviceProvider = serviceProvider;
	}

	/// <summary>
	/// Processes an individual request, determining if it matches any registered route and if so, handles it.
	/// Otherwise, it passes the request to the next middleware in the pipeline.
	/// </summary>
	/// <param name="context">The context for the current HTTP request.</param>
	/// <returns>The task representing the asynchronous operation.</returns>
	public async Task InvokeAsync(HttpContext context)
	{
		// Check if the request matches any registered route.
		var path = context.Request.Path.Value;
		var routeMatch = RouteMatcher.Match(path);

		if (routeMatch != null)
		{
			// Create the specific handler instance using the resolved service type.
			var handlerInstance = (IHandler)_serviceProvider.GetRequiredService(routeMatch.HandlerType);

			// Determine if the handler has a nested type implementing the IRequest interface.
			var requestType = routeMatch.HandlerType.GetNestedTypes().FirstOrDefault(x => typeof(IRequest).IsAssignableFrom(x));
			if (requestType == null)
			{
				await _next(context);
				return;
			}

			// Create an instance of the request type.
			var requestInstance = (IRequest)Activator.CreateInstance(requestType);

			// Extract the query parameters from the HTTP request.
			var queryParameters = context.Request.Query.ToDictionary(
				item => item.Key,
				item => item.Value.ToString(),
				StringComparer.OrdinalIgnoreCase
			);

			// If the request is POST or PUT, extract the body parameters assuming they are in JSON format.
			Dictionary<string, string> bodyParameters = new Dictionary<string, string>();
			if (context.Request.Method == "POST" || context.Request.Method == "PUT")
			{
				using var reader = new StreamReader(context.Request.Body);
				var body = await reader.ReadToEndAsync();
				bodyParameters = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
			}

			// Populate the request instance's properties with values from route variables, query parameters, or the request body.
			foreach (var prop in requestType.GetProperties())
			{
				var propName = prop.Name.ToLowerInvariant();
				if (routeMatch.Variables.TryGetValue(propName, out var variableValue))
				{
					prop.SetValue(requestInstance, Convert.ChangeType(variableValue, prop.PropertyType));
				}
				else if (queryParameters.TryGetValue(propName, out var queryValue))
				{
					prop.SetValue(requestInstance, Convert.ChangeType(queryValue, prop.PropertyType));
				}
				else if (bodyParameters.TryGetValue(propName, out var bodyValue))
				{
					prop.SetValue(requestInstance, Convert.ChangeType(bodyValue, prop.PropertyType));
				}
			}

			// Handle the request using the created handler and send back the response in JSON format.
			var response = (object)handlerInstance.Handle(requestInstance);

			context.Response.ContentType = "application/json";
			string json = JsonSerializer.Serialize(response);
			await context.Response.WriteAsync(json);
			return;
		}

		// If no route matches the request, continue to the next middleware.
		await _next(context);
	}
}