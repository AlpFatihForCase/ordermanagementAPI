using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OrderListFetcher.Settings;

namespace OrderListFetcher.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenService> _logger;
        private string _accessToken;
        private DateTime _expiresAt;
        private int _requestCount = 0;
        private DateTime? _lastRequestTime;

        public TokenService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> GetTokenAsync()
        {
            var tokenSettings = _configuration.GetSection("TokenSettings").Get<TokenSettings>();
            if (tokenSettings == null || string.IsNullOrEmpty(tokenSettings.TokenUrl))
            {
                _logger.LogError("Token URL yapılandırması bulunamadı.");
                return null;
            }

            if (_accessToken != null && _expiresAt > DateTime.UtcNow.AddMinutes(1))
            {
                return _accessToken;
            }

            var now = DateTime.UtcNow;
            if (_lastRequestTime.HasValue && (now - _lastRequestTime.Value).TotalHours < 1 && _requestCount >= 5)
            {
                _logger.LogWarning("Saatlik token istek sınırına ulaşıldı.");
                return null;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, tokenSettings.TokenUrl);
                var content = new StringContent($"grant_type=client_credentials&client_id={tokenSettings.ClientId}&client_secret={tokenSettings.ClientSecret}", Encoding.UTF8, "application/x-www-form-urlencoded");
                request.Content = content;

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenData = JsonDocument.Parse(responseContent).RootElement;

                _accessToken = tokenData.GetProperty("access_token").GetString();
                var expiresInSeconds = tokenData.GetProperty("expires_in").GetInt32();
                _expiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);

                _requestCount++;
                _lastRequestTime = now;
                _logger.LogInformation($"Yeni token alındı. Geçerlilik süresi: {expiresInSeconds} saniye.");
                return _accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token isteği sırasında hata oluştu: {ex.Message}");
                return null;
            }
        }
}