using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace ExampleAuthMiddleware.Auth
{
    public static class Security
    {
        private static readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        private static readonly string ISSUER = Environment.GetEnvironmentVariable("Auth0BaseURL");
        private static readonly string AUDIENCE = Environment.GetEnvironmentVariable("Auth0Audience");

        static Security()
        {
            var documentRetriever = new HttpDocumentRetriever { RequireHttps = ISSUER.StartsWith("https://") };

            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{ISSUER}.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever
            );
        }

        public static AuthenticationHeaderValue? GetAuthHeader(Microsoft.Azure.Functions.Worker.Http.HttpRequestData request)
        {
            if (!request.Headers.TryGetValues("Authorization", out var rawAuthHeader))
                return null;

            if (!AuthenticationHeaderValue.TryParse(rawAuthHeader?.FirstOrDefault(), out var authHeader))
                return null;

            return authHeader;
        }

        public static async Task<AuthorisedUser> ValidateTokenAsync(AuthenticationHeaderValue? authenticationHeader)
        {
            var result = new AuthorisedUser { IsAuthenticated = false };

            if (authenticationHeader == null)
                return result;

            if (authenticationHeader?.Scheme != "Bearer" || string.IsNullOrEmpty(authenticationHeader?.Parameter))
                return result;

            result.AuthToken = authenticationHeader.Parameter;

            var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);

            var validationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidAudience = AUDIENCE,
                ValidateAudience = true,
                ValidIssuer = ISSUER,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys
            };

            try
            {
                result = PerformValidation(authenticationHeader.Parameter, validationParameters);
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                // This exception is thrown if the signature key of the JWT could not be found.
                // This could be the case when the issuer changed its signing keys, so we trigger a 
                // refresh and retry validation.
                _configurationManager.RequestRefresh();
                result = PerformValidation(authenticationHeader.Parameter, validationParameters);
            }
            catch (SecurityTokenException) { }
            catch (Exception) { }

            return result;
        }

        private static AuthorisedUser PerformValidation(string bearerToken, TokenValidationParameters validationParameters)
        {
            var handler = new JwtSecurityTokenHandler();
            var claimsPrincipal = handler.ValidateToken(bearerToken, validationParameters, out var _);

            if ((claimsPrincipal.Identity?.IsAuthenticated ?? false) == false)
                return new AuthorisedUser { AuthToken = bearerToken, IsAuthenticated = false };

            return new AuthorisedUser
            {
                AuthToken = bearerToken,
                IsAuthenticated = true,
                UserID = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
            };
        }
    }
}
