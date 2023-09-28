using HCQR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHCQR(Lifetime.Transient);


var app = builder.Build();

app.UseHCQR();

app.Run();

namespace HCQR.Sample.News
{
	/// <summary>
	/// returns news for a given topic.
	/// </summary>
	[Get("/api/news")]
	public class GetNews : IHandler
	{
		public class Request : IRequest
		{

		}

		public class Response : IResponse
		{
			/// <summary>
			/// A list of news for the given topic
			/// </summary>
			public List<string>? News { get; set; }
		}

		public IResponse Handle(IRequest? request)
		{
			return new Response
			{
				News = new List<string>
			    {
				    $"News 1",
				    $"News 2",
				    $"News 3"
			    }
			};
		}
	}

    /// <summary>
    /// returns news for a given topic.
    /// </summary>
    [Post("/api/news/upload")]
    public class UploadNews : IHandler
    {
        public class Request : IRequest
        {
            /// <summary>
            /// The title of the news
            /// </summary>
            public string? Title { get; set; }
            /// <summary>
            /// The main article body
            /// </summary>
            public string? Body { get; set; } 
        }

        public class Response : IResponse
        {
            /// <summary>
            /// Success or failure of the upload
            /// </summary>
            public bool Suceess { get; set; }
            /// <summary>
            /// Message if any
            /// </summary>
            public string? Message { get; set; }
        }

        public IResponse Handle(IRequest? request)
        {
            var req = (Request)request;
            return new Response
            {
                Suceess = true,
                Message = $"The article: '{req.Title}' uploaded successfully"
            };
        }
    }
}
