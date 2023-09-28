using Microsoft.AspNetCore.Http;

namespace HCQR;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute
{
    public string Policy { get; set; } = string.Empty;

    public AuthorizeAttribute() { }

    public AuthorizeAttribute(string policy)
    {
        Policy = policy;
    }

    /// <summary>
    /// Verify that the policy is valid for the current user.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="policy"></param>
    /// <returns></returns>
    public static bool IsAuthorized(HttpContext context, string policy)
    {
        // If the user is authenticated and the policy is empty, then the user is authorized.
        if ((context.User.Identity?.IsAuthenticated ?? false) && policy == string.Empty)
            return true;

        // if the user is authenticated and the policy is not empty, then check if the user has the policy.
        if (context.User.HasClaim(claim => claim.Type == policy)) 
            return true;

        // If the user is not authenticated and the policy is empty, then the user is not authorized.
        return false;
    }
}