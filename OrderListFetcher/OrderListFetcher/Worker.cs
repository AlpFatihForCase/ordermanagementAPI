using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderListFetcher.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using OrderListFetcher.Settings;

namespace OrderListFetcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly OrderFetchingService _orderFetchingService;
        private Timer _timer;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, OrderFetchingService orderFetchingService)
        {
            _logger = logger;
            _configuration = configuration;
            _orderFetchingService = orderFetchingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workerSettings = _configuration.GetSection("WorkerSettings").Get<WorkerSettings>();
            int intervalMilliseconds = workerSettings?.FetchIntervalMinutes * 60 * 1000 ?? 5 * 60 * 1000; // Default 5 minutes

            _timer = new Timer(async _ =>
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Sipariş listesi getirme işlemi başlatılıyor.");
                    await _orderFetchingService.FetchOrdersAsync();
                    _logger.LogInformation($"Sipariş listesi getirme işlemi tamamlandı. Bir sonraki çalıştırma: {DateTime.UtcNow.AddMilliseconds(intervalMilliseconds)} UTC");
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(intervalMilliseconds));

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override void Dispose()
        {
            _timer?.Dispose();
        }
    }
}