using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zeebe.Client.Accelerator.ConnectorSecrets.Providers.Conjur;

public class ConjurSecretProvider : ISecretProvider
    {
        private readonly ConjurSecretProviderOptions _options;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ConjurSecretProvider> _logger;
        private string _authToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public ConjurSecretProvider(
            ILogger<ConjurSecretProvider> logger, 
            IOptions<ConjurSecretProviderOptions> options, 
            HttpClient httpClient = null)
        {
            _logger = logger;
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<string> GetSecretAsync(string key)
        {
            try
            {
                EnsureAuthenticated();
                string secretPath = $"{_options.SecretPath}/{key}";
                
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.ApiUrl}/secrets/{secretPath}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Token", $"token=\"{_authToken}\"");

                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                
                _logger.LogWarning("Failed to retrieve secret {Key} from Conjur: {StatusCode}", key, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secret {Key} from Conjur", key);
                return null;
            }
        }

        private void EnsureAuthenticated()
        {
            if (_authToken != null && DateTime.UtcNow < _tokenExpiry)
                return;

            try
            {
                // Authenticate to Conjur and get token
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.ApiUrl}/authn/login");
                var content = new StringContent(_options.Username);
                request.Content = content;
                
                var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                
                var apiKey = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                
                // Get actual auth token
                using var authRequest = new HttpRequestMessage(HttpMethod.Post, $"{_options.ApiUrl}/authn");
                authRequest.Content = new StringContent(_options.Password);
                
                var authResponse = _httpClient.SendAsync(authRequest).GetAwaiter().GetResult();
                authResponse.EnsureSuccessStatusCode();
                
                _authToken = authResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                _tokenExpiry = DateTime.UtcNow.AddMinutes(5); // Typical token lifetime
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate with Conjur");
                throw;
            }
        }
    }