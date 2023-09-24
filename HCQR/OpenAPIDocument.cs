using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using System.Reflection;

namespace HCQR;

public class OpenApiGen
{
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

		var openApiDocument = new OpenApiDocument
		{
			Info = new OpenApiInfo { 
				Title = name, 
				Version = version 
			},
			Paths = new OpenApiPaths()
		};

		foreach (var handlerType in handlerTypes)
		{
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
					Tags = new List<OpenApiTag> { new OpenApiTag { Name = tagName } },
					RequestBody = (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Head && requestType != null) ? new OpenApiRequestBody
					{
						Content = new Dictionary<string, OpenApiMediaType>
						{
							["application/json"] = new OpenApiMediaType
							{
								Schema = ExtractSchemaFromType(requestType)
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
									Schema = ExtractSchemaFromType(responseType)
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
			// Handle other types and custom mappings (like List<T>) here...
			default:
				return "object";
		}
	}
}