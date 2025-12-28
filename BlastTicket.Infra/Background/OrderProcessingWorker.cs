using BlastTicket.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection; // Scope用
using Microsoft.Extensions.Hosting; // BackgroundService用
using Microsoft.Extensions.Logging;

namespace BlastTicket.Infra.Background
{
    public class OrderProcessingWorker : BackgroundService
    {
        private readonly ILogger<OrderProcessingWorker> _logger;
        private readonly IOrderQueue _orderQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public OrderProcessingWorker(
            ILogger<OrderProcessingWorker> logger,
            IOrderQueue orderQueue,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _orderQueue = orderQueue;
            _serviceScopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderProcessingWorker started.");

            // キューからのデータを非同期で待つ
            await foreach (var order in _orderQueue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    // スコープを作成してIOrderServiceを取得
                    using var scope = _serviceScopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                    await repository.CreateAsync(order);

                    _logger.LogInformation("Processed order {OrderId}", order.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
                }
            }
            _logger.LogInformation("OrderProcessingWorker stopped.");
        }
    }
} 
