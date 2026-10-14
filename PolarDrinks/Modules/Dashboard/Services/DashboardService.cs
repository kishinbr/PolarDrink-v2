using PolarDrinks.Modules.Dashboard.Models;
using PolarDrinks.Modules.Estoque.Models;
using PolarDrinks.Modules.Estoque.Repositories;
using PolarDrinks.Modules.Estoque.Services;
using PolarDrinks.Modules.Loja.Models;
using PolarDrinks.Modules.Loja.Repositories;
using PolarDrinks.Modules.Vendas.Models;
using PolarDrinks.Modules.Vendas.Repositories;

namespace PolarDrinks.Modules.Dashboard.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IVendaRepository _vendaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IEstoqueService _estoqueService;

        public DashboardService(
            IVendaRepository vendaRepository,
            IProdutoRepository produtoRepository,
            IPedidoRepository pedidoRepository,
            IEstoqueService estoqueService)
        {
            _vendaRepository = vendaRepository;
            _produtoRepository = produtoRepository;
            _pedidoRepository = pedidoRepository;
            _estoqueService = estoqueService;
        }
        private PagamentoResumoDto MontarResumoPagamento(List<VendaModel> vendas, List<PedidoModel> pedidos, string canal)
        {
            var resumo = new PagamentoResumoDto();

            if (canal == "presencial" || canal == "geral")
            {
                resumo.QtdPix += vendas.Count(v => v.VendaTipoPagamento == VendaModel.TipoPagamento.Pix);
                resumo.QtdCartao += vendas.Count(v => v.VendaTipoPagamento == VendaModel.TipoPagamento.Cartao);
                resumo.QtdDinheiro += vendas.Count(v => v.VendaTipoPagamento == VendaModel.TipoPagamento.Dinheiro);

                resumo.TotalPix += vendas.Where(v => v.VendaTipoPagamento == VendaModel.TipoPagamento.Pix).Sum(v => v.VendaValorTotal);
                resumo.TotalCartao += vendas.Where(v => v.VendaTipoPagamento == VendaModel.TipoPagamento.Cartao).Sum(v => v.VendaValorTotal);
                resumo.TotalDinheiro += vendas.Where(v => v.VendaTipoPagamento == VendaModel.TipoPagamento.Dinheiro).Sum(v => v.VendaValorTotal);
            }

            if (canal == "online" || canal == "geral")
            {
                resumo.QtdPix += pedidos.Count(p => p.PedidoTipoPagamento == PedidoModel.TipoPagamento.Pix);
                resumo.QtdCartao += pedidos.Count(p => p.PedidoTipoPagamento == PedidoModel.TipoPagamento.Cartao);

                resumo.TotalPix += pedidos.Where(p => p.PedidoTipoPagamento == PedidoModel.TipoPagamento.Pix).Sum(p => p.PedidoValorTotal);
                resumo.TotalCartao += pedidos.Where(p => p.PedidoTipoPagamento == PedidoModel.TipoPagamento.Cartao).Sum(p => p.PedidoValorTotal);
            }

            return resumo;
        }

        public DashboardViewModel GerarDashboard()
        {
            var dados = CarregarDados();
            var model = new DashboardViewModel();

            PreencherFinanceiroPresencial(model, dados);
            PreencherEstoque(model, dados);
            PreencherLojaOnline(model, dados);
            PreencherCombinado(model, dados);
            PreencherPagamentos(model, dados);
            PreencherGiroDeEstoque(model, dados);
            PreencherComparativoPresencialOnline(model, dados);
            PreencherOperacaoLojaOnline(model, dados);
            PreencherRankingsDeProdutos(model, dados);
            PreencherPrevisoes(model, dados);
            PreencherGraficos(model, dados);
            PreencherCancelamentos(model, dados);

            return model;
        }

        // Dados carregados uma única vez e compartilhados entre as seções do dashboard.
        private sealed class DadosDashboard
        {
            public DateTime Hoje { get; init; }
            public DateTime InicioMes { get; init; }
            public DateTime InicioAno { get; init; }
            public DateTime Inicio30Dias { get; init; }
            public List<VendaModel> TodasVendas { get; init; } = new();
            public List<VendaModel> VendasHoje { get; init; } = new();
            public List<VendaModel> VendasAno { get; init; } = new();
            public List<VendaModel> Vendas7Dias { get; init; } = new();
            public List<VendaModel> Vendas30Dias { get; init; } = new();
            public List<ProdutoModel> Produtos { get; init; } = new();
            public List<PedidoModel> TodosPedidos { get; init; } = new();
            public List<PedidoModel> PedidosValidos { get; init; } = new();
            public List<PedidoModel> PedidosHoje { get; init; } = new();

            // Resumos financeiros calculados no banco (COUNT e SUM), sem carregar as entidades
            public (int Quantidade, decimal Faturamento, decimal Lucro) ResumoVendasHoje { get; init; }
            public (int Quantidade, decimal Faturamento, decimal Lucro) ResumoVendasMes { get; init; }
            public (int Quantidade, decimal Faturamento, decimal Lucro) ResumoVendasTotal { get; init; }
            public (int Quantidade, decimal Faturamento, decimal Lucro) ResumoPedidosHoje { get; init; }
            public (int Quantidade, decimal Faturamento, decimal Lucro) ResumoPedidosMes { get; init; }
            public (int Quantidade, decimal Faturamento, decimal Lucro) ResumoPedidosTotal { get; init; }
        }

        private DadosDashboard CarregarDados()
        {
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            var inicioAno = new DateTime(hoje.Year, 1, 1);
            var inicio30Dias = hoje.AddDays(-30);

            var vendasAtivas = _vendaRepository.ObterVendasAtivasComDetalhes();

            var vendasHoje = vendasAtivas.Where(v => v.VendaData.Date == hoje).ToList();
            var vendasAno = vendasAtivas.Where(v => v.VendaData >= inicioAno).ToList();
            var vendas7Dias = vendasAtivas.Where(v => v.VendaData.Date >= hoje.AddDays(-6)).ToList();
            var vendas30Dias = vendasAtivas.Where(v => v.VendaData >= inicio30Dias).ToList();
            var todasVendas = vendasAtivas;

            var produtos = _produtoRepository.ObterTodos();

            // ===== LOJA ONLINE =====
            var todosPedidos = _pedidoRepository.ObterTodosComDetalhes();

            var statusQueContam = new[] { PedidoModel.Status.Concluido, PedidoModel.Status.CanceladoNaoRetirado };

            var pedidosValidos = todosPedidos.Where(p => statusQueContam.Contains(p.PedidoStatus)).ToList();

            var pedidosHoje = pedidosValidos.Where(p => p.PedidoData.Date == hoje).ToList();

            // ===== RESUMOS FINANCEIROS (agregados no banco: COUNT e SUM) =====
            var resumoVendasHoje = _vendaRepository.ObterResumoFinanceiro(hoje, hoje.AddDays(1));
            var resumoVendasMes = _vendaRepository.ObterResumoFinanceiro(inicioMes, null);
            var resumoVendasTotal = _vendaRepository.ObterResumoFinanceiro(null, null);

            var resumoPedidosHoje = _pedidoRepository.ObterResumoFinanceiro(statusQueContam, hoje, hoje.AddDays(1));
            var resumoPedidosMes = _pedidoRepository.ObterResumoFinanceiro(statusQueContam, inicioMes, null);
            var resumoPedidosTotal = _pedidoRepository.ObterResumoFinanceiro(statusQueContam, null, null);

            return new DadosDashboard
            {
                Hoje = hoje,
                InicioMes = inicioMes,
                InicioAno = inicioAno,
                Inicio30Dias = inicio30Dias,
                TodasVendas = todasVendas,
                VendasHoje = vendasHoje,
                VendasAno = vendasAno,
                Vendas7Dias = vendas7Dias,
                Vendas30Dias = vendas30Dias,
                Produtos = produtos,
                TodosPedidos = todosPedidos,
                PedidosValidos = pedidosValidos,
                PedidosHoje = pedidosHoje,
                ResumoVendasHoje = resumoVendasHoje,
                ResumoVendasMes = resumoVendasMes,
                ResumoVendasTotal = resumoVendasTotal,
                ResumoPedidosHoje = resumoPedidosHoje,
                ResumoPedidosMes = resumoPedidosMes,
                ResumoPedidosTotal = resumoPedidosTotal
            };
        }

        // Faturamento, lucro e ticket médio das vendas do balcão.
        private void PreencherFinanceiroPresencial(DashboardViewModel model, DadosDashboard d)
        {
            // FINANCEIRO (valores calculados no banco)
            model.TotalHoje = d.ResumoVendasHoje.Faturamento;
            model.TotalMes = d.ResumoVendasMes.Faturamento;

            model.LucroHoje = d.ResumoVendasHoje.Lucro;
            model.LucroMes = d.ResumoVendasMes.Lucro;

            // TICKET MÉDIO (todas as vendas ativas)
            model.TicketMedio = d.ResumoVendasTotal.Quantidade > 0
                ? d.ResumoVendasTotal.Faturamento / d.ResumoVendasTotal.Quantidade
                : 0;
        }

        // Indicadores de estoque e resumo de produtos.
        private void PreencherEstoque(DashboardViewModel model, DadosDashboard d)
        {
            var produtos = d.Produtos;

            // ESTOQUE (as contagens e a regra vêm do módulo de estoque; os grupos se sobrepõem)
            var alertas = _estoqueService.ContarAlertasDeEstoque();
            model.SemEstoque = alertas.SemEstoque;
            model.EstoqueBaixo = alertas.AbaixoDoMinimo;
            // PRODUTOS EM ESTOQUE (RESUMO)
            model.ProdutosCadastrados = produtos.Count(p => p.ProdutoAtivo);
            model.ProdutosIndisponiveis = produtos.Count(p => !p.ProdutoAtivo);
            model.ProdutosEmPromocao = produtos.Count(p => p.ProdutoAtivo && p.ProdutoPromocao > 0);

            model.ValorEstoqueAproximado = produtos
                .Where(p => p.ProdutoAtivo)
                .Sum(p => (p.ProdutoQtdEstoque ?? 0) * (p.ProdutoPrecoCusto ?? 0));
        }

        // Faturamento e lucro dos pedidos da loja online.
        private void PreencherLojaOnline(DashboardViewModel model, DadosDashboard d)
        {
            // Pedidos que contam como faturamento: ver statusQueContam em CarregarDados
            model.TotalOnlineHoje = d.ResumoPedidosHoje.Faturamento;
            model.TotalOnlineMes = d.ResumoPedidosMes.Faturamento;

            model.LucroOnlineHoje = d.ResumoPedidosHoje.Lucro;
            model.LucroOnlineMes = d.ResumoPedidosMes.Lucro;
        }

        // Totais combinados (balcão + online) e tickets médios.
        private void PreencherCombinado(DashboardViewModel model, DadosDashboard d)
        {
            // ===== COMBINADO (Presencial + Online) =====
            model.TotalCombinadoHoje = model.TotalHoje + model.TotalOnlineHoje;
            model.TotalCombinadoMes = model.TotalMes + model.TotalOnlineMes;
            model.LucroCombinadoHoje = model.LucroHoje + model.LucroOnlineHoje;
            model.LucroCombinadoMes = model.LucroMes + model.LucroOnlineMes;

            var quantidadeTransacoesTotal = d.ResumoVendasTotal.Quantidade + d.ResumoPedidosTotal.Quantidade;
            var somaTotalTransacoes = d.ResumoVendasTotal.Faturamento + d.ResumoPedidosTotal.Faturamento;

            model.TicketMedioCombinado = quantidadeTransacoesTotal > 0
                ? somaTotalTransacoes / quantidadeTransacoesTotal
                : 0;

            model.TicketMedioOnline = d.ResumoPedidosTotal.Quantidade > 0
                ? d.ResumoPedidosTotal.Faturamento / d.ResumoPedidosTotal.Quantidade
                : 0;
        }

        // Pagamentos por canal e período.
        private void PreencherPagamentos(DashboardViewModel model, DadosDashboard d)
        {
            var hoje = d.Hoje;
            var inicio30Dias = d.Inicio30Dias;
            var todasVendas = d.TodasVendas;
            var vendasHoje = d.VendasHoje;
            var vendas7Dias = d.Vendas7Dias;
            var vendas30Dias = d.Vendas30Dias;
            var todosPedidos = d.TodosPedidos;

            // ===== NOVA ESTRUTURA: PAGAMENTOS POR CANAL E PERÍODO =====
            var pedidosHojeParaPagamento = todosPedidos.Where(p => p.PedidoData.Date == hoje).ToList();
            var pedidos7DiasParaPagamento = todosPedidos.Where(p => p.PedidoData.Date >= hoje.AddDays(-6)).ToList();
            var pedidos30DiasParaPagamento = todosPedidos.Where(p => p.PedidoData >= inicio30Dias).ToList();
            var todosPedidosParaPagamento = todosPedidos;

            var canais = new[] { "presencial", "online", "geral" };

            var periodosVendas = new Dictionary<string, List<VendaModel>>
            {
                ["hoje"] = vendasHoje,
                ["semana"] = vendas7Dias,
                ["mes"] = vendas30Dias,
                ["total"] = todasVendas
            };

            var periodosPedidos = new Dictionary<string, List<PedidoModel>>
            {
                ["hoje"] = pedidosHojeParaPagamento,
                ["semana"] = pedidos7DiasParaPagamento,
                ["mes"] = pedidos30DiasParaPagamento,
                ["total"] = todosPedidosParaPagamento
            };

            foreach (var canal in canais)
            {
                model.PagamentosPorCanalEPeriodo[canal] = new Dictionary<string, PagamentoResumoDto>();

                foreach (var periodo in periodosVendas.Keys)
                {
                    model.PagamentosPorCanalEPeriodo[canal][periodo] =
                        MontarResumoPagamento(periodosVendas[periodo], periodosPedidos[periodo], canal);
                }
            }
        }

        // Giro de estoque (últimos 30 dias, balcão + online).
        private void PreencherGiroDeEstoque(DashboardViewModel model, DadosDashboard d)
        {
            var inicio30Dias = d.Inicio30Dias;
            var vendas30Dias = d.Vendas30Dias;
            var produtos = d.Produtos;
            var pedidosValidos = d.PedidosValidos;

            // GIRO DE ESTOQUE (baseado nos últimos 30 dias, Presencial + Online)
            var vendidoPorProdutoPresencial = vendas30Dias
                .SelectMany(v => v.Itens)
                .GroupBy(i => i.ProdutoID)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.ItemVendaQtd));

            var pedidos30Dias = pedidosValidos.Where(p => p.PedidoData >= inicio30Dias).ToList();

            var vendidoPorProdutoOnline = pedidos30Dias
                .SelectMany(p => p.Itens)
                .GroupBy(i => i.ProdutoID)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.ItemPedidoQtd));

            model.ProdutosGiroLento = produtos
                .Where(p => p.ProdutoAtivo)
                .Select(p =>
                {
                    var vendidoTotal =
                        (vendidoPorProdutoPresencial.ContainsKey(p.ProdutoID) ? vendidoPorProdutoPresencial[p.ProdutoID] : 0) +
                        (vendidoPorProdutoOnline.ContainsKey(p.ProdutoID) ? vendidoPorProdutoOnline[p.ProdutoID] : 0);

                    var mediaVendaPorDia = vendidoTotal / 30.0;

                    double? giro = mediaVendaPorDia > 0
                        ? (p.ProdutoQtdEstoque ?? 0) / mediaVendaPorDia
                        : null;

                    return new GiroProdutoDto
                    {
                        ProdutoNome = p.ProdutoNome,
                        EstoqueAtual = p.ProdutoQtdEstoque ?? 0,
                        GiroDias = giro
                    };
                })
                .Where(g => g.EstoqueAtual > 0)
                .OrderByDescending(g => g.GiroDias ?? double.MaxValue)
                .Take(5)
                .ToList();
        }

        // Participação do balcão e da loja online no mês.
        private void PreencherComparativoPresencialOnline(DashboardViewModel model, DadosDashboard d)
        {
            // ===== COMPARATIVO PRESENCIAL vs ONLINE (baseado no mês) =====
            var totalGeralMes = model.TotalMes + model.TotalOnlineMes;

            if (totalGeralMes > 0)
            {
                model.PercentualPresencialMes = Math.Round((model.TotalMes / totalGeralMes) * 100, 1);
                model.PercentualOnlineMes = Math.Round((model.TotalOnlineMes / totalGeralMes) * 100, 1);
            }
            else
            {
                model.PercentualPresencialMes = 0;
                model.PercentualOnlineMes = 0;
            }
        }

        // Indicadores operacionais da loja online.
        private void PreencherOperacaoLojaOnline(DashboardViewModel model, DadosDashboard d)
        {
            var inicioMes = d.InicioMes;
            var todosPedidos = d.TodosPedidos;

            // ===== OPERAÇÃO DA LOJA ONLINE =====
            var pedidosMesTodos = todosPedidos.Where(p => p.PedidoData >= inicioMes).ToList();

            var pedidosFinalizadosMes = pedidosMesTodos
                .Where(p => p.PedidoStatus == PedidoModel.Status.Concluido ||
                            p.PedidoStatus == PedidoModel.Status.CanceladoNaoRetirado)
                .ToList();

            model.TaxaNaoRetiradaMes = pedidosFinalizadosMes.Count > 0
                ? Math.Round((decimal)pedidosFinalizadosMes.Count(p => p.PedidoStatus == PedidoModel.Status.CanceladoNaoRetirado)
                    / pedidosFinalizadosMes.Count * 100, 1)
                : 0;

            var pedidosConcluidosComDatas = todosPedidos
                .Where(p => p.PedidoStatus == PedidoModel.Status.Concluido
                         && p.PedidoDataSeparado.HasValue
                         && p.PedidoDataConcluido.HasValue)
                .ToList();

            model.TempoMedioRetiradaHoras = pedidosConcluidosComDatas.Count > 0
                ? pedidosConcluidosComDatas.Average(p =>
                    (p.PedidoDataConcluido!.Value - p.PedidoDataSeparado!.Value).TotalHours)
                : null;

            model.HorarioPicoPedidos = todosPedidos.Count > 0
                ? todosPedidos
                    .GroupBy(p => p.PedidoData.Hour)
                    .OrderByDescending(g => g.Count())
                    .Select(g => (int?)g.Key)
                    .FirstOrDefault()
                : null;

            model.FaturamentoPorExpiracaoMes = pedidosMesTodos
                .Where(p => p.PedidoStatus == PedidoModel.Status.CanceladoNaoRetirado)
                .Sum(p => p.PedidoValorTotal);
        }

        // Produtos mais vendidos e mais lucrativos (balcão, online e total).
        private void PreencherRankingsDeProdutos(DashboardViewModel model, DadosDashboard d)
        {
            var todasVendas = d.TodasVendas;
            var pedidosValidos = d.PedidosValidos;

            // PRODUTOS
            model.ProdutoMaisVendido = todasVendas
                .SelectMany(v => v.Itens)
                .GroupBy(i => i.Produto.ProdutoNome)
                .OrderByDescending(g => g.Sum(x => x.ItemVendaQtd))
                .Select(g => g.Key)
                .FirstOrDefault();

            model.ProdutoMaisLucrativo = todasVendas
                .SelectMany(v => v.Itens)
                .GroupBy(i => i.Produto.ProdutoNome)
                .OrderByDescending(g => g.Sum(i => (i.ItemVendaPreco - i.ItemVendaCusto) * i.ItemVendaQtd))
                .Select(g => g.Key)
                .FirstOrDefault();
            // PRODUTOS - ONLINE
            model.ProdutoMaisVendidoOnline = pedidosValidos
                .SelectMany(p => p.Itens)
                .Where(i => i.Produto != null)
                .GroupBy(i => i.Produto!.ProdutoNome)
                .OrderByDescending(g => g.Sum(x => x.ItemPedidoQtd))
                .Select(g => g.Key)
                .FirstOrDefault();

            model.ProdutoMaisLucrativoOnline = pedidosValidos
                .SelectMany(p => p.Itens)
                .Where(i => i.Produto != null)
                .GroupBy(i => i.Produto!.ProdutoNome)
                .OrderByDescending(g => g.Sum(i => (i.ItemPedidoPreco - i.ItemPedidoCusto) * i.ItemPedidoQtd))
                .Select(g => g.Key)
                .FirstOrDefault();

            // PRODUTOS - TOTAL (Presencial + Online)
            var quantidadesPresencial = todasVendas
                .SelectMany(v => v.Itens)
                .GroupBy(i => i.Produto.ProdutoNome)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.ItemVendaQtd));

            var quantidadesOnline = pedidosValidos
                .SelectMany(p => p.Itens)
                .Where(i => i.Produto != null)
                .GroupBy(i => i.Produto!.ProdutoNome)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.ItemPedidoQtd));

            var todosNomesProdutos = quantidadesPresencial.Keys.Union(quantidadesOnline.Keys);

            model.ProdutoMaisVendidoTotal = todosNomesProdutos
                .Select(nome => new
                {
                    Nome = nome,
                    Total = (quantidadesPresencial.ContainsKey(nome) ? quantidadesPresencial[nome] : 0) +
                            (quantidadesOnline.ContainsKey(nome) ? quantidadesOnline[nome] : 0)
                })
                .OrderByDescending(x => x.Total)
                .Select(x => x.Nome)
                .FirstOrDefault();

            var lucrosPresencial = todasVendas
                .SelectMany(v => v.Itens)
                .GroupBy(i => i.Produto.ProdutoNome)
                .ToDictionary(g => g.Key, g => g.Sum(i => (i.ItemVendaPreco - i.ItemVendaCusto) * i.ItemVendaQtd));

            var lucrosOnline = pedidosValidos
                .SelectMany(p => p.Itens)
                .Where(i => i.Produto != null)
                .GroupBy(i => i.Produto!.ProdutoNome)
                .ToDictionary(g => g.Key, g => g.Sum(i => (i.ItemPedidoPreco - i.ItemPedidoCusto) * i.ItemPedidoQtd));

            var todosNomesLucro = lucrosPresencial.Keys.Union(lucrosOnline.Keys);

            model.ProdutoMaisLucrativoTotal = todosNomesLucro
                .Select(nome => new
                {
                    Nome = nome,
                    Total = (lucrosPresencial.ContainsKey(nome) ? lucrosPresencial[nome] : 0) +
                            (lucrosOnline.ContainsKey(nome) ? lucrosOnline[nome] : 0)
                })
                .OrderByDescending(x => x.Total)
                .Select(x => x.Nome)
                .FirstOrDefault();
        }

        // Previsão de vendas (regressão linear) e tendência por produto.
        private void PreencherPrevisoes(DashboardViewModel model, DadosDashboard d)
        {
            var hoje = d.Hoje;
            var todasVendas = d.TodasVendas;
            var vendas7Dias = d.Vendas7Dias;
            var pedidosValidos = d.PedidosValidos;

            // PREVISÃO
            var vendasPorDia = vendas7Dias
                .GroupBy(v => v.VendaData.Date)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(v => v.VendaValorTotal))
                .ToList();

            model.PrevisaoAmanha = PrevisaoLinear.PreverProximo(vendasPorDia);

            var vendasPorProduto = todasVendas
                .SelectMany(v => v.Itens)
                .Where(i => i.Produto != null)
                .GroupBy(i => i.ProdutoID)
                .Select(g => new
                {
                    ProdutoNome = g.First().Produto.ProdutoNome,
                    Dados = g.Select(x => new
                    {
                        Data = x.Venda.VendaData.Date,
                        Quantidade = x.ItemVendaQtd
                    })
                })
                .ToList();

            var tendencias = new List<(string Nome, double Slope)>();

            foreach (var p in vendasPorProduto)
            {
                var dadosPorDia = p.Dados
                    .GroupBy(x => x.Data)
                    .OrderBy(x => x.Key)
                    .Select(g => g.Sum(x => x.Quantidade))
                    .ToList();

                var inclinacao = PrevisaoLinear.Inclinacao(dadosPorDia);

                if (inclinacao == null)
                    continue;

                tendencias.Add((p.ProdutoNome, inclinacao.Value));
            }

            model.Top3ProdutosAlta = tendencias
                .OrderByDescending(t => t.Slope)
                .Take(3)
                .Select(t => t.Nome)
                .ToList();

            // PREVISÃO - ONLINE
            var pedidos7Dias = pedidosValidos.Where(p => p.PedidoData.Date >= hoje.AddDays(-6)).ToList();

            var pedidosPorDia = pedidos7Dias
                .GroupBy(p => p.PedidoData.Date)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(p => p.PedidoValorTotal))
                .ToList();

            model.PrevisaoAmanhaOnline = PrevisaoLinear.PreverProximo(pedidosPorDia);

            model.PrevisaoAmanhaCombinada = model.PrevisaoAmanha + model.PrevisaoAmanhaOnline;
        }

        // Séries dos gráficos de vendas por canal e período.
        private void PreencherGraficos(DashboardViewModel model, DadosDashboard d)
        {
            var hoje = d.Hoje;
            var inicioAno = d.InicioAno;
            var inicio30Dias = d.Inicio30Dias;
            var vendasHoje = d.VendasHoje;
            var vendasAno = d.VendasAno;
            var vendas7Dias = d.Vendas7Dias;
            var vendas30Dias = d.Vendas30Dias;
            var pedidosValidos = d.PedidosValidos;
            var pedidosHoje = d.PedidosHoje;

            // GRÁFICOS
            var vendasHojePorHora = vendasHoje
                .GroupBy(v => v.VendaData.Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            model.VendasHojeLista = Enumerable.Range(0, 24)
                .Select(hora => (decimal)(vendasHojePorHora.ContainsKey(hora) ? vendasHojePorHora[hora] : 0))
                .ToList();

            var ultimos7Dias = Enumerable.Range(0, 7)
                .Select(i => hoje.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var vendasAgrupadas = vendas7Dias
                .GroupBy(v => v.VendaData.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            model.VendasSemana = ultimos7Dias
                .Select(dia => (decimal)(vendasAgrupadas.ContainsKey(dia) ? vendasAgrupadas[dia] : 0))
                .ToList();

            var ultimos30Dias = Enumerable.Range(0, 30)
                .Select(i => hoje.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var vendasAgrupadasMes = vendas30Dias
                .GroupBy(v => v.VendaData.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            model.VendasMesGrafico = ultimos30Dias
                .Select(dia => (decimal)(vendasAgrupadasMes.ContainsKey(dia) ? vendasAgrupadasMes[dia] : 0))
                .ToList();

            var mesesAno = Enumerable.Range(1, 12).ToList();

            var vendasAgrupadasAno = vendasAno
                .GroupBy(v => v.VendaData.Month)
                .ToDictionary(g => g.Key, g => g.Count());

            model.VendasAno = mesesAno
                .Select(mes => (decimal)(vendasAgrupadasAno.ContainsKey(mes) ? vendasAgrupadasAno[mes] : 0))
                .ToList();
            // ===== VENDAS POR CANAL E PERÍODO (Presencial + Online) =====
            var vendasOnlineHojePorHora = pedidosHoje
                .GroupBy(p => p.PedidoData.Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            var vendasOnlineHojeLista = Enumerable.Range(0, 24)
                .Select(hora => (decimal)(vendasOnlineHojePorHora.ContainsKey(hora) ? vendasOnlineHojePorHora[hora] : 0))
                .ToList();

            var pedidos7DiasParaGrafico = pedidosValidos.Where(p => p.PedidoData.Date >= hoje.AddDays(-6)).ToList();
            var vendasOnlineAgrupadas = pedidos7DiasParaGrafico
                .GroupBy(p => p.PedidoData.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var vendasOnlineSemana = ultimos7Dias
                .Select(dia => (decimal)(vendasOnlineAgrupadas.ContainsKey(dia) ? vendasOnlineAgrupadas[dia] : 0))
                .ToList();

            var pedidos30DiasParaGrafico = pedidosValidos.Where(p => p.PedidoData >= inicio30Dias).ToList();
            var vendasOnlineAgrupadasMes = pedidos30DiasParaGrafico
                .GroupBy(p => p.PedidoData.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var vendasOnlineMesGrafico = ultimos30Dias
                .Select(dia => (decimal)(vendasOnlineAgrupadasMes.ContainsKey(dia) ? vendasOnlineAgrupadasMes[dia] : 0))
                .ToList();

            var vendasOnlineAgrupadasAno = pedidosValidos
                .Where(p => p.PedidoData >= inicioAno)
                .GroupBy(p => p.PedidoData.Month)
                .ToDictionary(g => g.Key, g => g.Count());

            var vendasOnlineAno = mesesAno
                .Select(mes => (decimal)(vendasOnlineAgrupadasAno.ContainsKey(mes) ? vendasOnlineAgrupadasAno[mes] : 0))
                .ToList();

            List<decimal> Somar(List<decimal> a, List<decimal> b) =>
                a.Zip(b, (x, y) => x + y).ToList();

            model.VendasPorCanalEPeriodo["presencial"] = new Dictionary<string, List<decimal>>
            {
                ["hoje"] = model.VendasHojeLista,
                ["semana"] = model.VendasSemana,
                ["mes"] = model.VendasMesGrafico,
                ["ano"] = model.VendasAno
            };

            model.VendasPorCanalEPeriodo["online"] = new Dictionary<string, List<decimal>>
            {
                ["hoje"] = vendasOnlineHojeLista,
                ["semana"] = vendasOnlineSemana,
                ["mes"] = vendasOnlineMesGrafico,
                ["ano"] = vendasOnlineAno
            };

            model.VendasPorCanalEPeriodo["geral"] = new Dictionary<string, List<decimal>>
            {
                ["hoje"] = Somar(model.VendasHojeLista, vendasOnlineHojeLista),
                ["semana"] = Somar(model.VendasSemana, vendasOnlineSemana),
                ["mes"] = Somar(model.VendasMesGrafico, vendasOnlineMesGrafico),
                ["ano"] = Somar(model.VendasAno, vendasOnlineAno)
            };
        }

        // Indicadores de cancelamento (balcão, online e total).
        private void PreencherCancelamentos(DashboardViewModel model, DadosDashboard d)
        {
            var hoje = d.Hoje;
            var inicioMes = d.InicioMes;
            var todosPedidos = d.TodosPedidos;

            // CANCELAMENTOS
            var vendasCanceladas = _vendaRepository.ObterVendasCanceladasComDetalhes();

            model.CanceladosHoje = vendasCanceladas.Count(v => v.VendaData.Date == hoje);
            model.CanceladosSemana = vendasCanceladas.Count(v => v.VendaData.Date >= hoje.AddDays(-6));
            model.CanceladosMes = vendasCanceladas.Count(v => v.VendaData >= inicioMes);

            model.ProdutoMaisCancelado = vendasCanceladas
                .SelectMany(v => v.Itens)
                .Where(i => i.Produto != null)
                .GroupBy(i => i.Produto.ProdutoNome)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            // CANCELAMENTOS - ONLINE
            var statusCancelados = new[]
            {
                PedidoModel.Status.CanceladoCliente,
                PedidoModel.Status.CanceladoAdmin,
                PedidoModel.Status.CanceladoNaoRetirado
            };

            var pedidosCancelados = todosPedidos.Where(p => statusCancelados.Contains(p.PedidoStatus)).ToList();

            model.CanceladosOnlineHoje = pedidosCancelados.Count(p => p.PedidoData.Date == hoje);
            model.CanceladosOnlineSemana = pedidosCancelados.Count(p => p.PedidoData.Date >= hoje.AddDays(-6));
            model.CanceladosOnlineMes = pedidosCancelados.Count(p => p.PedidoData >= inicioMes);

            model.ProdutoMaisCanceladoOnline = pedidosCancelados
                .SelectMany(p => p.Itens)
                .Where(i => i.Produto != null)
                .GroupBy(i => i.Produto!.ProdutoNome)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            // CANCELAMENTOS - TOTAL (Presencial + Online)
            model.CanceladosTotalHoje = model.CanceladosHoje + model.CanceladosOnlineHoje;
            model.CanceladosTotalSemana = model.CanceladosSemana + model.CanceladosOnlineSemana;
            model.CanceladosTotalMes = model.CanceladosMes + model.CanceladosOnlineMes;
            var canceladosPorProdutoPresencial = vendasCanceladas
                .SelectMany(v => v.Itens)
                .Where(i => i.Produto != null)
                .GroupBy(i => i.Produto!.ProdutoNome)
                .ToDictionary(g => g.Key, g => g.Count());

            var canceladosPorProdutoOnline = pedidosCancelados
                .SelectMany(p => p.Itens)
                .Where(i => i.Produto != null)
                .GroupBy(i => i.Produto!.ProdutoNome)
                .ToDictionary(g => g.Key, g => g.Count());

            var todosNomesCancelados = canceladosPorProdutoPresencial.Keys.Union(canceladosPorProdutoOnline.Keys);

            model.ProdutoMaisCanceladoTotal = todosNomesCancelados
                .Select(nome => new
                {
                    Nome = nome,
                    Total = (canceladosPorProdutoPresencial.ContainsKey(nome) ? canceladosPorProdutoPresencial[nome] : 0) +
                            (canceladosPorProdutoOnline.ContainsKey(nome) ? canceladosPorProdutoOnline[nome] : 0)
                })
                .OrderByDescending(x => x.Total)
                .Select(x => x.Nome)
                .FirstOrDefault();
        }
    }
}