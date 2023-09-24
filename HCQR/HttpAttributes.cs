namespace HCQR;

/// <summary>
/// The base inferace for all HTTP requests.
/// </summary>
public interface IHttpMap {
	/// <summary>
	/// The path to match.
	/// </summary>
	public string Path { get; }
}

/// <summary>
/// Defines a contract for handling requests and producing responses.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class GetAttribute : Attribute, IHttpMap
{
	/// <summary>
	/// The path to match.
	/// </summary>
	public string Path { get; }

	public GetAttribute(string path)
	{
		Path = path;
	}
}

/// <summary>
/// Defines a contract for handling requests and producing responses.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PostAttribute : Attribute, IHttpMap
{
	/// <summary>
	/// The path to match.
	/// </summary>
	public string Path { get; }

	public PostAttribute(string path)
	{
		Path = path;
	}
}

/// <summary>
/// Defines a contract for handling requests and producing responses.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PutAttribute : Attribute, IHttpMap
{
	/// <summary>
	/// The path to match.
	/// </summary>
	public string Path { get; }

	public PutAttribute(string path)
	{
		Path = path;
	}
}

/// <summary>
/// Defines a contract for handling requests and producing responses.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DeleteAttribute : Attribute, IHttpMap
{
	/// <summary>
	/// The path to match.
	/// </summary>
	public string Path { get; }

	public DeleteAttribute(string path)
	{
		Path = path;
	}
}

/// <summary>
/// Defines a contract for handling requests and producing responses.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PatchAttribute : Attribute, IHttpMap
{
	/// <summary>
	/// The path to match.
	/// </summary>
	public string Path { get; }

	public PatchAttribute(string path)
	{
		Path = path;
	}
}

/// <summary>
/// Defines a contract for handling requests and producing responses.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class OptionsAttribute : Attribute, IHttpMap
{
	/// <summary>
	/// The path to match.
	/// </summary>
	public string Path { get; }

	public OptionsAttribute(string path)
	{
		Path = path;
	}
}

/// <summary>
/// Defines a contract for handling requests and producing responses.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HeadAttribute : Attribute, IHttpMap
{
	/// <summary>
	/// The path to match.
	/// </summary>
	public string Path { get; }

	public HeadAttribute(string path)
	{
		Path = path;
	}
}

/// <summary>
/// HTTP method.
/// </summary>
public enum HttpMethod
{
	Get,
	Post,
	Put,
	Delete,
	Patch,
	Options,
	Head
}

/// <summary>
/// HTTP method extentions.
/// </summary>
internal static class HttpAttributeExtentions
{
	/// <summary>
	/// Gets the HTTP method associated with the HTTP map.
	/// </summary>
	/// <param name="httpMap"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static HttpMethod GetHttpMethod(this IHttpMap httpMap)
	{
		return httpMap switch
		{
			GetAttribute => HttpMethod.Get,
			PostAttribute => HttpMethod.Post,
			PutAttribute => HttpMethod.Put,
			DeleteAttribute => HttpMethod.Delete,
			PatchAttribute => HttpMethod.Patch,
			OptionsAttribute => HttpMethod.Options,
			HeadAttribute => HttpMethod.Head,
			_ => throw new Exception("Unknown HTTP method")
		};
	}

	/// <summary>
	/// Gets the HTTP method name associated with the HTTP map.
	/// </summary>
	/// <param name="httpMap"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static string GetHttpMethodName(this IHttpMap httpMap)
	{
		return httpMap switch
		{
			GetAttribute => "GET",
			PostAttribute => "POST",
			PutAttribute => "PUT",
			DeleteAttribute => "DELETE",
			PatchAttribute => "PATCH",
			OptionsAttribute => "OPTIONS",
			HeadAttribute => "HEAD",
			_ => throw new Exception("Unknown HTTP method")
		};
	}

	/// <summary>
	/// Converts the HTTP method name to the HTTP method.
	/// </summary>
	/// <param name="httpMethod"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static HttpMethod GetHttpMethod(this string httpMethod)
	{
		return httpMethod.ToLower() switch
		{
			"get" => HttpMethod.Get,
			"post" => HttpMethod.Post,
			"put" => HttpMethod.Put,
			"delete" => HttpMethod.Delete,
			"patch" => HttpMethod.Patch,
			"options" => HttpMethod.Options,
			"head" => HttpMethod.Head,
			_ => throw new Exception("Unknown HTTP method")
		};
	}
}
