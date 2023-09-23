namespace HCQR;

/// <summary>
/// Defines a contract for handling requests and producing responses.
/// </summary>
public interface IHandler
{
	/// <summary>
	/// Handles the given request and produces a response.
	/// </summary>
	/// <param name="request">The request to be handled.</param>
	/// <returns>The response generated after handling the request.</returns>
	IResponse Handle(IRequest request);
}

/// <summary>
/// Defines a contract for the response produced by the handler.
/// </summary>
public interface IResponse { }

/// <summary>
/// Defines a contract for a request that can be handled by a handler.
/// </summary>
public interface IRequest { }