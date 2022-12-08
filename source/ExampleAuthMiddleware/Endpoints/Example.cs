using ExampleAuthMiddleware.Auth;
using ExampleAuthMiddleware.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace ExampleAuthMiddleware.Endpoints
{
    public class Example
    {
        private readonly UserService _userService;

        public Example(UserService userService)
        {
            _userService = userService;
        }

        [Function("Example")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestData req)
        {
            var authResponse = req.GetAuthenticatedUser();
            if (authResponse.IsAuthenticated == false)
                return req.CreateResponse(HttpStatusCode.Unauthorized);

            var user = await _userService.FetchUserAsync(authResponse.AuthToken);

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(new
            {
                user = user
            }, HttpStatusCode.OK);
            return response;
        }
    }
}
