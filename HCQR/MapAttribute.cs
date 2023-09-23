namespace HCQR;

/// <summary>
/// Attribute used to map a handler to a specific path or route.
/// This allows the middleware to determine which handler to invoke based on the request path.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MapAttribute : Attribute
{
	/// <summary>
	/// Gets the path or route associated with the handler.
	/// </summary>
	public string Path { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MapAttribute"/> class with the specified path.
	/// </summary>
	/// <param name="path">The path or route to associate with the handler.</param>
	public MapAttribute(string path)
	{
		Path = path;
	}
}