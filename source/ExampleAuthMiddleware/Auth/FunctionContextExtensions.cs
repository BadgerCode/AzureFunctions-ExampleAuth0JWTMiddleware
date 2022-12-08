using Microsoft.Azure.Functions.Worker;
using System.Net;
using System.Reflection;

namespace ExampleAuthMiddleware.Auth
{
    public static class FunctionContextExtensions
    {
        public static void SetHttpResponseStatusCode(this FunctionContext context, HttpStatusCode statusCode)
        {
            // https://joonasw.net/view/azure-ad-jwt-authentication-in-net-isolated-process-azure-functions
            var coreAssembly = Assembly.Load("Microsoft.Azure.Functions.Worker.Core");
            var featureInterfaceName = "Microsoft.Azure.Functions.Worker.Context.Features.IFunctionBindingsFeature";
            var featureInterfaceType = coreAssembly.GetType(featureInterfaceName);
            var bindingsFeature = context.Features.Single(
                f => f.Key.FullName == featureInterfaceType.FullName).Value;
            var invocationResultProp = featureInterfaceType.GetProperty("InvocationResult");

            var grpcAssembly = Assembly.Load("Microsoft.Azure.Functions.Worker.Grpc");
            var responseDataType = grpcAssembly.GetType("Microsoft.Azure.Functions.Worker.GrpcHttpResponseData");
            var responseData = Activator.CreateInstance(responseDataType, context, statusCode);

            invocationResultProp.SetMethod.Invoke(bindingsFeature, new object[] { responseData });
        }
    }
}
