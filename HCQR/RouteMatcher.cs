using System.Net.Http;

namespace HCQR;

/// <summary>
/// Provides functionality to match incoming URLs with registered routes and determine the appropriate handler.
/// </summary>
public static class RouteMatcher
{
	/// <summary>
	/// List of registered routes along with their associated handler types.
	/// </summary>
	private static readonly List<RouteHandlerMapping> _routes = new();

	/// <summary>
	/// Registers a handler type to a specific route.
	/// </summary>
	/// <param name="route">The route or path to associate with the handler.</param>
	/// <param name="handlerType">The type of the handler to be invoked when the route is matched.</param>
	public static void Register(HttpMethod httpMethod, string route, Type handlerType)
	{
		var variableNames = new List<string>();
		var tokens = route.Split('/');
		foreach (var token in tokens)
		{
			if (token.StartsWith("{") && token.EndsWith("}"))
			{
				variableNames.Add(token.Trim('{', '}'));
			}
		}

		_routes.Add(new RouteHandlerMapping
		{
			OriginalRoute = route,
			HandlerType = handlerType,
			VariableNames = variableNames,
			HttpMethod = httpMethod // Store the HTTP method
		});
	}

	/// <summary>
	/// Matches an incoming URL against the registered routes and returns the associated handler type.
	/// </summary>
	/// <param name="incomingUrl">The URL to be matched.</param>
	/// <returns>The match result containing the handler type and route variables, or null if no match is found.</returns>
	public static RouteMatchResult? Match(HttpMethod httpMethod, string incomingUrl)
	{
		var queryStringStart = incomingUrl.IndexOf("?");
		var path = queryStringStart >= 0 ? incomingUrl[..queryStringStart] : incomingUrl;
		var queryString = queryStringStart >= 0 ? incomingUrl[(queryStringStart + 1)..] : null;

		var incomingTokens = path.Split('/');
		foreach (var route in _routes)
		{
			if (route.HttpMethod != httpMethod) continue;
			var routeTokens = route.OriginalRoute.Split('/');
			if (incomingTokens.Length != routeTokens.Length) continue;

			var isMatch = true;
			var variables = new Dictionary<string, string>();
			for (int i = 0; i < routeTokens.Length; i++)
			{
				if (routeTokens[i].StartsWith("{") && routeTokens[i].EndsWith("}"))
				{
					// Capture the variable
					var variableName = routeTokens[i].Trim('{', '}');
					variables[variableName] = incomingTokens[i];
				}
				else if (routeTokens[i] != incomingTokens[i])
				{
					isMatch = false;
					break;
				}
			}

			if (isMatch && queryString != null)
			{
				var parameters = queryString.Split('&');
				foreach (var parameter in parameters)
				{
					var parts = parameter.Split('=');
					if (parts.Length == 2)
					{
						variables[parts[0]] = Uri.UnescapeDataString(parts[1]);
					}
				}
			}

			if (isMatch)
			{
				return new RouteMatchResult
				{
					HandlerType = route.HandlerType,
					Variables = variables
				};
			}
		}
		return null;
	}
}

/// <summary>
/// Represents a mapping between a route and its associated handler type.
/// </summary>
public class RouteHandlerMapping
{
	/// <summary>
	/// Gets or sets the original route pattern.
	/// </summary>
	public required string OriginalRoute { get; set; }

	/// <summary>
	/// Gets or sets the type of the handler associated with the route.
	/// </summary>
	public required Type HandlerType { get; set; }

	/// <summary>
	/// Gets or sets the list of variable names in the route.
	/// </summary>
	/// <remarks>
	/// For example, in the route "/items/{itemId}", "itemId" would be a variable name.
	/// </remarks>
	public required List<string> VariableNames { get; set; } = new List<string>();

	/// <summary>
	/// The HTTP method associated with the route.
	/// </summary>
	public required HttpMethod HttpMethod { get; set; } 
}

/// <summary>
/// Represents the result of a route match, containing the matched handler type and any route variables.
/// </summary>
public class RouteMatchResult
{
	/// <summary>
	/// Gets or sets the type of the handler to be invoked for the matched route.
	/// </summary>
	public required Type HandlerType { get; set; }

	/// <summary>
	/// Gets or sets the dictionary of route variables captured from the matched route.
	/// </summary>
	public required Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}