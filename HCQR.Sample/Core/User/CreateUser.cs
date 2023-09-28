namespace HCQR.Sample.Core.User;


/// <summary>
/// returns news for a given topic.
/// </summary>
[Post("/api/user/create")]
public class CreateUser : IHandler
{
    public class Request : IRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Response : IResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public IResponse Handle(IRequest? request)
    {
        var req = (Request)request;
        return new Response
        {
            Success = true,
            Message = $"User {req.UserName} created"
        };
    }
}
