using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace HCQR;

/// <summary>
/// Helper class to generate the OpenAPI JSON document.
/// </summary>
public class OpenApiGen
{
    /// <summary>
    /// Generate the OpenAPI JSON document.
    /// </summary>
    /// <param name="handlerTypes"></param>
    /// <returns></returns>
    public static string GenerateOpenApiJson(IEnumerable<Type> handlerTypes)
    {
        string name = "API";
        string version = "1.0.0.0";

        Assembly? entryAsm = Assembly.GetEntryAssembly();
        if (entryAsm != null)
        {
            name = entryAsm.GetName().Name ?? "API";
            version = entryAsm.GetName().Version?.ToString() ?? "1.0.0.0";
        }

        var xmlDoc = LoadXmlDocumentation();

        var openApiDocument = new OpenApiDocument
        {
            Info = new OpenApiInfo
            {
                Title = name,
                Version = version
            },
            Paths = new OpenApiPaths()
        };

        foreach (var handlerType in handlerTypes)
        {
            var handlerDescription = ExtractXmlSummary(handlerType, xmlDoc);

            var httpMapAttribute = handlerType.GetCustomAttributes().OfType<IHttpMap>();
            foreach (var attribute in httpMapAttribute)
            {
                var httpMethod = attribute.GetHttpMethod();

                var route = attribute;
                var path = route.Path;

                if (!openApiDocument.Paths.TryGetValue(path, out OpenApiPathItem? pathItem))
                {
                    pathItem = new OpenApiPathItem();
                    openApiDocument.Paths.Add(path, pathItem);
                }

                // Extract request and response models
                var requestType = handlerType.GetNestedType("Request");
                var responseType = handlerType.GetNestedType("Response");

                var namespaceParts = handlerType.Namespace?.Split('.');
                var tagName = namespaceParts?[namespaceParts.Length - 1] ?? "default";

                var operation = new OpenApiOperation
                {
                    Summary = handlerDescription,
                    Tags = new List<OpenApiTag> { new OpenApiTag { Name = tagName } },
                    RequestBody = (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Head && requestType != null) ? new OpenApiRequestBody
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new OpenApiMediaType
                            {
                                Schema = ExtractSchemaFromType(requestType, xmlDoc)
                            }
                        }
                    } : null,
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Success",
                            Content = responseType == null ? null : new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType
                                {
                                    Schema = ExtractSchemaFromType(responseType, xmlDoc)
                                }
                            }
                        }
                    }
                };

                switch (httpMethod)
                {
                    case HttpMethod.Get:
                        pathItem.Operations.Add(OperationType.Get, operation);
                        break;
                    case HttpMethod.Post:
                        pathItem.Operations.Add(OperationType.Post, operation);
                        break;
                    case HttpMethod.Put:
                        pathItem.Operations.Add(OperationType.Put, operation);
                        break;
                    case HttpMethod.Delete:
                        pathItem.Operations.Add(OperationType.Delete, operation);
                        break;
                    case HttpMethod.Patch:
                        pathItem.Operations.Add(OperationType.Patch, operation);
                        break;
                    case HttpMethod.Options:
                        pathItem.Operations.Add(OperationType.Options, operation);
                        break;
                    case HttpMethod.Head:
                        pathItem.Operations.Add(OperationType.Head, operation);
                        break;
                }

            }
        }

        var writerSettings = new OpenApiJsonWriterSettings();
        var output = new StringWriter();
        openApiDocument.SerializeAsV3(new OpenApiJsonWriter(output, writerSettings));

        var json = output.ToString();

        return json;
    }

    /// <summary>
    /// Get the path to the XML documentation file.
    /// </summary>
    private static string XMLDocumentation => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml");

    /// <summary>
    /// Load the XML documentation file.
    /// </summary>
    /// <returns></returns>
    private static XDocument? LoadXmlDocumentation() => File.Exists(XMLDocumentation) ? XDocument.Load(XMLDocumentation) : null;

    /// <summary>
    /// Extract the summary from the XML documentation file.
    /// </summary>
    /// <param name="member"></param>
    /// <param name="xmlDoc"></param>
    /// <returns></returns>
    private static string? ExtractXmlSummary(MemberInfo member, XDocument xmlDoc)
    {
        string memberName;

        switch (member.MemberType)
        {
            case MemberTypes.Method:
                memberName = $"M:{member.DeclaringType?.FullName}.{member.Name}".Replace('+', '.');
                break;
            case MemberTypes.Property:
                memberName = $"P:{member.DeclaringType?.FullName}.{member.Name}".Replace('+', '.');
                break;
            case MemberTypes.Field:
                memberName = $"F:{member.DeclaringType?.FullName}.{member.Name}".Replace('+', '.');
                break;
            case MemberTypes.Event:
                memberName = $"E:{member.DeclaringType?.FullName}.{member.Name}".Replace('+', '.');
                break;
            case MemberTypes.TypeInfo:
                memberName = $"T:{((TypeInfo)member).FullName}";
                break;
            case MemberTypes.NestedType:
                memberName = $"T:{member.DeclaringType?.FullName}";
                break;
            default:
                return null;
        }

        var memberNode = xmlDoc.XPathSelectElement($"/doc/members/member[@name='{memberName}']/summary");
        return memberNode?.Value.Trim();
    }

    /// <summary>
    /// Extract the schema from the XML documentation file.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="xmlDoc"></param>
    /// <returns></returns>
    private static OpenApiSchema ExtractSchemaFromType(Type type, XDocument xmlDoc)
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = type.GetProperties().ToDictionary(
                prop => prop.Name,
                prop => new OpenApiSchema
                {
                    Type = GetOpenApiType(prop.PropertyType),
                    Description = ExtractXmlSummary(prop, xmlDoc)
                }
            )
        };

        return schema;
    }

    /// <summary>
    /// Extract the schema from the type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static OpenApiSchema ExtractSchemaFromType(Type type)
	{
		var schema = new OpenApiSchema
		{
			Type = "object",
			Properties = type.GetProperties().ToDictionary(
				prop => prop.Name,
				prop => new OpenApiSchema { Type = GetOpenApiType(prop.PropertyType) }
			)
		};

		return schema;
	}

    /// <summary>
    /// Get the OpenAPI type from the .NET type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
	private static string GetOpenApiType(Type type)
	{
		switch (Type.GetTypeCode(type))
		{
			case TypeCode.Boolean:
				return "boolean";
			case TypeCode.String:
				return "string";
			case TypeCode.Int32:
				return "integer";
			case TypeCode.Double:
				return "number";
			default:
				return "object";
		}
	}
}