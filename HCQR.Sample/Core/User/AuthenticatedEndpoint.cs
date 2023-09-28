namespace HCQR.Sample.Core.User;

[Authorize]
[Get("/api/user/autenticated")]
public class AuthenticatedEndpoint : IHandler
{
    public class Request : IRequest { }

    public class Response : IResponse
    {
        public bool Success { get; set; }
    }

    public IResponse Handle(IRequest? request)
    {
        return new Response
        {
            Success = true,
        };
    }
}
