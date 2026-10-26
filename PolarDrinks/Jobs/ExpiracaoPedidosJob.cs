using PolarDrinks.Services.Loja;

namespace PolarDrinks.Jobs
{
    public class ExpiracaoPedidosJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(15);

        public ExpiracaoPedidosJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var escopo = _serviceProvider.CreateScope())
                    {
                        var pedidoService = escopo.ServiceProvider.GetRequiredService<IPedidoService>();
                        var quantidade = pedidoService.ExpirarPedidosNaoRetirados();

                        if (quantidade > 0)
                        {
                            Console.WriteLine($"[ExpiracaoPedidosJob] {quantidade} pedido(s) expirado(s) automaticamente.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ExpiracaoPedidosJob] Erro ao executar: {ex.Message}");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }
        }
    }
}