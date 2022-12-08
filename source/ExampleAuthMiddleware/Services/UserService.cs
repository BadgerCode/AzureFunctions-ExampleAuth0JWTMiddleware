using System.Text.Json.Serialization;

namespace ExampleAuthMiddleware.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Auth0");
        }

        public async Task<Auth0User> FetchUserAsync(string accessToken)
        {
            // TODO: Retry on transient
            var request = new HttpRequestMessage(HttpMethod.Get, "userinfo");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error from users API: ({response.StatusCode}) {responseBody}");

            return System.Text.Json.JsonSerializer.Deserialize<Auth0User>(responseBody);
        }
    }

    public class Auth0User
    {
        [JsonPropertyName("sub")]
        public string ID { get; set; }

        [JsonPropertyName("name")]
        public string FullName { get; set; }

        [JsonPropertyName("given_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("family_name")]
        public string LastName { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("picture")]
        public string PictureURL { get; set; }

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonPropertyName("email")]
        public string EmailAddress { get; set; }

        [JsonPropertyName("email_verified")]
        public bool IsEmailVerified { get; set; }
    }
}
