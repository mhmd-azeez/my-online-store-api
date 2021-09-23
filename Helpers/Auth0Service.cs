using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace MyOnlineStoreAPI.Helpers
{
    public class Auth0User
    {
        [JsonPropertyName("user_id")]
        public string Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Auth0Service
    {
        private HttpClient _idpClient;
        private HttpClient _auth0Client;
        private AuthOptions _authOptions;

        public Auth0Service(
            IHttpClientFactory httpClientFactory,
            IOptions<AuthOptions> authOptions)
        {
            _idpClient = httpClientFactory.CreateClient("IdPClient");
            _auth0Client = httpClientFactory.CreateClient("Auth0Client");
            _authOptions = authOptions.Value;
        }

        // https://auth0.com/docs/api/management/v2#!/Users_By_Email/get_users_by_email
        public async Task<List<Auth0User>> SearchAsync(string email)
        {
            var query = new QueryBuilder();
            query.Add("q", $"email:{email}*");
            query.Add("fields", "user_id,email,name");
            query.Add("include_fields", "true");

            await SetToken();

            return await _auth0Client.GetFromJsonAsync<List<Auth0User>>($"/api/v2/users{query}");
        }

        public async Task<Auth0User> GetUserAsync(string id)
        {
            var query = new QueryBuilder();
            query.Add("id", id);
            query.Add("fields", "user_id,email,name");
            query.Add("include_fields", "true");

            await SetToken();
            
            var users = await _auth0Client.GetFromJsonAsync<List<Auth0User>>($"/api/v2/users{query}");
            return users.FirstOrDefault();
        }

        private async Task SetToken()
        {
            var token = await GetTokenAsync();
            _auth0Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<string> GetTokenAsync()
        {
            var document = await _idpClient.GetDiscoveryDocumentAsync();
            var response = await _idpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = document.TokenEndpoint,
                ClientId = _authOptions.M2MClientId,
                ClientSecret = _authOptions.M2MClientSecret,
                Scope = "read:users",
                Parameters = new Parameters(new Dictionary<string, string>
                {
                    { "audience", _authOptions.ManagementApiUrl }
                })
            });

            return response.AccessToken;
        }
    }
}