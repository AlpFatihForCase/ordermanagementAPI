using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using OrderListFetcher.Settings;


namespace OrderListFetcher.Services;

public class OrderFetchingService
{
    private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderFetchingService> _logger;
        private readonly TokenService _tokenService;

        public OrderFetchingService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<OrderFetchingService> logger, TokenService tokenService)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _tokenService = tokenService;
        }

        public async Task FetchOrdersAsync()
        {
            var orderListSettings = _configuration.GetSection("OrderListSettings").Get<OrderListSettings>();
            if (orderListSettings == null || string.IsNullOrEmpty(orderListSettings.OrderListUrl))
            {
                _logger.LogError("Sipariş listesi URL yapılandırması bulunamadı.");
                return;
            }

            var token = await _tokenService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Geçerli token alınamadı, sipariş listesi getirilemiyor.");
                return;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(orderListSettings.OrderListUrl);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var orders = JsonDocument.Parse(responseContent).RootElement;
                _logger.LogInformation($"Sipariş listesi alındı: {orders.ToString()}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sipariş listesi isteği sırasında hata oluştu: {ex.Message}");
            }
        }
}