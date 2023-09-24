using HCQR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHCQR(Lifetime.Transient);


var app = builder.Build();

app.UseHCQR();

app.Run();

namespace HCQR.Sample.News
{

	[Get("/api/{topic}")]
	public class GetNews : IHandler
	{
		public class Request : IRequest
		{
			public string? Topic { get; set; }
		}

		public class Response : IResponse
		{
			public List<string>? News { get; set; }
		}

		public IResponse Handle(IRequest request)
		{
			var req = (Request)request;
			return new Response
			{
				News = new List<string>
			{
				$"{req.Topic} News 1",
				$"{req.Topic} News 2",
				$"{req.Topic} News 3"
			}
			};
		}
	}
}
