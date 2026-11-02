using NSubstitute;
using PolarDrinks.Modules.Dashboard.Services;
using PolarDrinks.Modules.Estoque.Models;
using PolarDrinks.Modules.Estoque.Repositories;
using PolarDrinks.Modules.Estoque.Services;
using PolarDrinks.Modules.Loja.Models;
using PolarDrinks.Modules.Loja.Repositories;
using PolarDrinks.Modules.Vendas.Models;
using PolarDrinks.Modules.Vendas.Repositories;

namespace PolarDrinks.Tests.Dashboard;

public class DashboardServiceFinanceiroTests
{
    private readonly IVendaRepository _vendaRepository = Substitute.For<IVendaRepository>();
    private readonly IProdutoRepository _produtoRepository = Substitute.For<IProdutoRepository>();
    private readonly IPedidoRepository _pedidoRepository = Substitute.For<IPedidoRepository>();
    private readonly IEstoqueService _estoqueService = Substitute.For<IEstoqueService>();
    private readonly DashboardService _service;

    private static readonly (int Quantidade, decimal Faturamento, decimal Lucro) Vazio = (0, 0m, 0m);

    public DashboardServiceFinanceiroTests()
    {
        // Listas vazias: só os resumos calculados no banco alimentam o financeiro
        _vendaRepository.ObterVendasAtivasComDetalhes().Returns(new List<VendaModel>());
        _vendaRepository.ObterVendasCanceladasComDetalhes().Returns(new List<VendaModel>());
        _produtoRepository.ObterTodos().Returns(new List<ProdutoModel>());
        _pedidoRepository.ObterTodosComDetalhes().Returns(new List<PedidoModel>());

        _service = new DashboardService(_vendaRepository, _produtoRepository, _pedidoRepository, _estoqueService);
    }

    [Fact]
    public void Financeiro_DeveUsarOsResumosCalculadosNoBanco()
    {
        ConfigurarResumos(
            vendasHoje: (2, 300m, 120m), vendasMes: (10, 1500m, 600m), vendasTotal: (40, 6000m, 2400m),
            pedidosHoje: (1, 100m, 40m), pedidosMes: (4, 480m, 200m), pedidosTotal: (8, 960m, 400m));

        var model = _service.GerarDashboard();

        // Balcão
        Assert.Equal(300m, model.TotalHoje);
        Assert.Equal(1500m, model.TotalMes);
        Assert.Equal(120m, model.LucroHoje);
        Assert.Equal(600m, model.LucroMes);
        Assert.Equal(150m, model.TicketMedio); // 6000 / 40

        // Loja online
        Assert.Equal(100m, model.TotalOnlineHoje);
        Assert.Equal(480m, model.TotalOnlineMes);
        Assert.Equal(40m, model.LucroOnlineHoje);
        Assert.Equal(200m, model.LucroOnlineMes);
        Assert.Equal(120m, model.TicketMedioOnline); // 960 / 8

        // Combinado
        Assert.Equal(400m, model.TotalCombinadoHoje);
        Assert.Equal(1980m, model.TotalCombinadoMes);
        Assert.Equal(160m, model.LucroCombinadoHoje);
        Assert.Equal(800m, model.LucroCombinadoMes);
        Assert.Equal(145m, model.TicketMedioCombinado); // (6000 + 960) / (40 + 8)
    }

    [Fact]
    public void SemVendasNemPedidos_TicketsDevemSerZero()
    {
        ConfigurarResumos(Vazio, Vazio, Vazio, Vazio, Vazio, Vazio);

        var model = _service.GerarDashboard();

        Assert.Equal(0m, model.TicketMedio);
        Assert.Equal(0m, model.TicketMedioOnline);
        Assert.Equal(0m, model.TicketMedioCombinado);
        Assert.Equal(0m, model.TotalCombinadoMes);
    }

    [Fact]
    public void Resumos_DevemSerPedidosParaHojeMesETotal()
    {
        ConfigurarResumos(Vazio, Vazio, Vazio, Vazio, Vazio, Vazio);

        _service.GerarDashboard();

        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

        // Vendas: (inicio, fim)
        var vendas = ChamadasDeResumo(_vendaRepository);
        Assert.Contains(vendas, a => (DateTime?)a[0] == hoje && (DateTime?)a[1] == hoje.AddDays(1));
        Assert.Contains(vendas, a => (DateTime?)a[0] == inicioMes && a[1] == null);
        Assert.Contains(vendas, a => a[0] == null && a[1] == null);

        // Pedidos: (statusValidos, inicio, fim)
        var pedidos = ChamadasDeResumo(_pedidoRepository);
        Assert.Contains(pedidos, a => (DateTime?)a[1] == hoje && (DateTime?)a[2] == hoje.AddDays(1));
        Assert.Contains(pedidos, a => (DateTime?)a[1] == inicioMes && a[2] == null);
        Assert.Contains(pedidos, a => a[1] == null && a[2] == null);
    }

    [Fact]
    public void Pedidos_SoContamNoFaturamento_ConcluidosENaoRetirados()
    {
        ConfigurarResumos(Vazio, Vazio, Vazio, Vazio, Vazio, Vazio);

        _service.GerarDashboard();

        var esperados = new[] { PedidoModel.Status.Concluido, PedidoModel.Status.CanceladoNaoRetirado };

        foreach (var chamada in ChamadasDeResumo(_pedidoRepository))
        {
            var recebidos = ((IEnumerable<string>)chamada[0]!).OrderBy(s => s).ToArray();

            Assert.Equal(esperados.OrderBy(s => s).ToArray(), recebidos);
        }
    }

    // ---------- Métodos auxiliares ----------

    // Responde ao resumo conforme o período pedido: com fim = hoje, só início = mês, sem limites = total
    private void ConfigurarResumos(
        (int Quantidade, decimal Faturamento, decimal Lucro) vendasHoje,
        (int Quantidade, decimal Faturamento, decimal Lucro) vendasMes,
        (int Quantidade, decimal Faturamento, decimal Lucro) vendasTotal,
        (int Quantidade, decimal Faturamento, decimal Lucro) pedidosHoje,
        (int Quantidade, decimal Faturamento, decimal Lucro) pedidosMes,
        (int Quantidade, decimal Faturamento, decimal Lucro) pedidosTotal)
    {
        _vendaRepository.ObterResumoFinanceiro(null, null).ReturnsForAnyArgs(chamada =>
            Escolher(chamada.ArgAt<DateTime?>(0), chamada.ArgAt<DateTime?>(1), vendasHoje, vendasMes, vendasTotal));

        _pedidoRepository.ObterResumoFinanceiro(Array.Empty<string>(), null, null).ReturnsForAnyArgs(chamada =>
            Escolher(chamada.ArgAt<DateTime?>(1), chamada.ArgAt<DateTime?>(2), pedidosHoje, pedidosMes, pedidosTotal));
    }

    private static (int Quantidade, decimal Faturamento, decimal Lucro) Escolher(
        DateTime? inicio, DateTime? fim,
        (int Quantidade, decimal Faturamento, decimal Lucro) hoje,
        (int Quantidade, decimal Faturamento, decimal Lucro) mes,
        (int Quantidade, decimal Faturamento, decimal Lucro) total)
    {
        if (inicio == null && fim == null)
        {
            return total;
        }

        return fim != null ? hoje : mes;
    }

    private static List<object?[]> ChamadasDeResumo(object repositorio)
    {
        return repositorio.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == "ObterResumoFinanceiro")
            .Select(c => c.GetArguments())
            .ToList();
    }
}