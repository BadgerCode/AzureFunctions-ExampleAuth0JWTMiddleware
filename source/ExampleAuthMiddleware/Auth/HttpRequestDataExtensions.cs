using Microsoft.Azure.Functions.Worker.Http;

namespace ExampleAuthMiddleware.Auth
{
    public static class HttpRequestDataExtensions
    {
        public static AuthorisedUser GetAuthenticatedUser(this HttpRequestData req)
        {
            if (req.FunctionContext.Items.TryGetValue(JWTMiddleware.AuthorisedUserFunctionContextKey, out var user))
                return (AuthorisedUser)user;

            return new AuthorisedUser { IsAuthenticated = false };
        }
    }
}
