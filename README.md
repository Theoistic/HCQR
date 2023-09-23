# HCQR 

The `HCQR` package provides a lightweight HCQR (Holistic Command Query Responsibility) middleware for ASP.NET Core applications, 
making it easy to map routes directly to handlers that implement the `IHandler` interface. 
This ensures a more clean and direct flow from request to response without any excess boilerplate.

Heavily inspired by CQRS, but more holistic in its approch and condences the handle, response and request objects.

## Features
- Directly maps HTTP routes to command or query handlers.
- Handles both query parameters and request bodies.
- Automatically registers and resolves handler types.
- Allows for custom route patterns with placeholders.
  
## Getting Started

### 1. Installation

Install the `HCQR` package from NuGet:

```bash
dotnet add package HCQR
```

### 2. Configuration

In your Startup.cs or wherever you configure your services and middleware, do the following:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHCQR();
    // ... other services
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseHCQR();
    // ... other middlewares
}
```

### 3. Creating Handlers

To create a handler, implement the IHandler interface:

```csharp
[Map("/api/user/{id}")]
public class GetUserHandler : IHandler
{
    public IResponse Handle(IRequest request)
    {
        // Handle the request and return the response
    }
}
```

Make sure to mark the handler with the [Map] attribute to specify the route.

### 4. Creating Requests & Responses

Each handler should have a nested class that implements IRequest. For example:

```csharp
public class GetUserHandler : IHandler
{
    public class Request : IRequest
    {
        public int Id { get; set; }
    }

    public IResponse Handle(IRequest request)
    {
        var req = (Request)request;
        // Handle the request using req.Id and return the response
    }
}
```

Responses can be any object implementing the IResponse interface.

### 5. Handling Routes

Once everything is set up, any incoming request that matches the mapped routes will be automatically directed to the corresponding handler. The HCQRMiddleware will deserialize any parameters in the route, query string, or body and pass them to your handler.

### Contribute

Your contributions to improve HCQR are welcome! Feel free to open issues or submit pull requests.
License

This project is licensed under the MIT License.
