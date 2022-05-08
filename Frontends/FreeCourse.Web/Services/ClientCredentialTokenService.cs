using FreeCourse.Web.Models;
using FreeCourse.Web.Services.Interfaces;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class ClientCredentialTokenService : IClientCredentialTokenService
    {
        private readonly ServiceApiSettings serviceApiSettings;
        private readonly ClientSettings clientSettings;
        private readonly IClientAccessTokenCache clientAccessTokenCache;
        private readonly HttpClient httpClient;

        public ClientCredentialTokenService(
            IOptions<ServiceApiSettings> serviceApiSettings,
            IOptions<ClientSettings> clientSettings,
            IClientAccessTokenCache clientAccessTokenCache,
            HttpClient httpClient
            )
        {
            this.serviceApiSettings = serviceApiSettings.Value;
            this.clientSettings = clientSettings.Value;
            this.clientAccessTokenCache = clientAccessTokenCache;
            this.httpClient = httpClient;
        }

        public async Task<string> GetTokenAsync()
        {
            var currentToken = await clientAccessTokenCache.GetAsync("WebClientToken");

            if (currentToken != null)
            {
                return currentToken.AccessToken;
            }

            // Discovery Endpoint
            var disco = await this.httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = serviceApiSettings.IdentityBaseUrl,
                Policy = new DiscoveryPolicy { RequireHttps = false }
            });

            if (disco.IsError)
            {
                throw disco.Exception;
            }

            var clientCredentialTokenRequest = new ClientCredentialsTokenRequest
            {
                ClientId = clientSettings.WebClient.ClientId,
                ClientSecret = clientSettings.WebClient.ClientSecret,
                Address = disco.TokenEndpoint
            };

            var newToken = await httpClient.RequestClientCredentialsTokenAsync(clientCredentialTokenRequest);

            if (newToken.IsError)
            {
                throw newToken.Exception;
            }

            await clientAccessTokenCache.SetAsync("WebClientToken", newToken.AccessToken, newToken.ExpiresIn);

            return newToken.AccessToken;
        }
    }
}
