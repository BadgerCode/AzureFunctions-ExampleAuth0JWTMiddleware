using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Net;

namespace ExampleAuthMiddleware.Auth
{
    public class JWTMiddleware : IFunctionsWorkerMiddleware
    {
        public const string AuthorisedUserFunctionContextKey = "AuthorisedUser";

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            // https://github.com/Azure/azure-functions-dotnet-worker/blob/main/samples/CustomMiddleware/MyCustomMiddleware.cs
            var requestData = await context.GetHttpRequestDataAsync();

            var authHeader = Security.GetAuthHeader(requestData);
            var authResponse = await Security.ValidateTokenAsync(authHeader);

            if (authResponse.IsAuthenticated == false)
            {
                context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                return;
            }

            context.Items.Add(AuthorisedUserFunctionContextKey, authResponse);
            await next(context);
        }
    }
}
