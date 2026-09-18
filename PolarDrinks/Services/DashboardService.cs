using PolarDrinks.Models;
using PolarDrinks.Models.Loja;
using PolarDrinks.Repositories;
using PolarDrinks.Repositories.Loja;

namespace PolarDrinks.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IVendaRepository _vendaRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IPedidoRepository _pedidoRepository;

        public DashboardService(
            IVendaRepository vendaRepository,
            IProdutoRepository produtoRepository,
            IPedidoRepository pedidoRepository)
        {
            _vendaRepository = vendaRepository;
            _produtoRepository = produtoRepository;
            _pedidoRepository = pedidoRepository;
        }
        private PagamentoResumoDto MontarResumoPagamento(List<VendaModel> vendas, List<PedidoModel> pedidos, string canal)
        {
            var resumo = new PagamentoResumoDto();

            if (canal == "presencial" || canal == "geral")
            {
                resumo.QtdPix += vendas.Count(v => v.VendaTipoPagamento == "Pix");
                resumo.QtdCartao += vendas.Count(v => v.VendaTipoPagamento == "Cartão");
                resumo.QtdDinheiro += vendas.Count(v => v.VendaTipoPagamento == "Dinheiro");

                resumo.TotalPix += vendas.Where(v => v.VendaTipoPagamento == "Pix").Sum(v => v.VendaValorTotal);
                resumo.TotalCartao += vendas.Where(v => v.VendaTipoPagamento == "Cartão").Sum(v => v.VendaValorTotal);
                resumo.TotalDinheiro += vendas.Where(v => v.VendaTipoPagamento == "Dinheiro").Sum(v => v.VendaValorTotal);
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
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            var inicioAno = new DateTime(hoje.Year, 1, 1);
            var inicio30Dias = hoje.AddDays(-30);

            var vendasAtivas = _vendaRepository.ObterVendasAtivasComDetalhes();

            var vendasHoje = vendasAtivas.Where(v => v.VendaData.Date == hoje).ToList();
            var vendasMes = vendasAtivas.Where(v => v.VendaData >= inicioMes).ToList();
            var vendasAno = vendasAtivas.Where(v => v.VendaData >= inicioAno).ToList();
            var vendas7Dias = vendasAtivas.Where(v => v.VendaData.Date >= hoje.AddDays(-6)).ToList();
            var vendas30Dias = vendasAtivas.Where(v => v.VendaData >= inicio30Dias).ToList();
            var todasVendas = vendasAtivas;

            var produtos = _produtoRepository.ObterTodos();

            var model = new DashboardViewModel();

            // FINANCEIRO
            model.TotalHoje = vendasHoje.Sum(v => v.VendaValorTotal);
            model.TotalMes = vendasMes.Sum(v => v.VendaValorTotal);

            model.LucroHoje = vendasHoje.Sum(v =>
                v.Itens.Sum(i => (i.ItemVendaPreco - i.ItemVendaCusto) * i.ItemVendaQtd));

            model.LucroMes = vendasMes.Sum(v =>
                v.Itens.Sum(i => (i.ItemVendaPreco - i.ItemVendaCusto) * i.ItemVendaQtd));

            // TICKET MÉDIO
            model.TicketMedio = todasVendas.Any() ? todasVendas.Average(v => v.VendaValorTotal) : 0;

            // PAGAMENTOS (CARDS - HOJE)
            model.QtdPix = vendasHoje.Count(v => v.VendaTipoPagamento == "Pix");
            model.QtdCartao = vendasHoje.Count(v => v.VendaTipoPagamento == "Cartão");
            model.QtdDinheiro = vendasHoje.Count(v => v.VendaTipoPagamento == "Dinheiro");

            model.TotalPix = vendasHoje.Where(v => v.VendaTipoPagamento == "Pix").Sum(v => v.VendaValorTotal);
            model.TotalCartao = vendasHoje.Where(v => v.VendaTipoPagamento == "Cartão").Sum(v => v.VendaValorTotal);
            model.TotalDinheiro = vendasHoje.Where(v => v.VendaTipoPagamento == "Dinheiro").Sum(v => v.VendaValorTotal);

            // PAGAMENTOS (GRÁFICOS)
            model.PixHoje = vendasHoje.Count(v => v.VendaTipoPagamento == "Pix");
            model.CartaoHoje = vendasHoje.Count(v => v.VendaTipoPagamento == "Cartão");
            model.DinheiroHoje = vendasHoje.Count(v => v.VendaTipoPagamento == "Dinheiro");

            model.PixSemana = vendas7Dias.Count(v => v.VendaTipoPagamento == "Pix");
            model.CartaoSemana = vendas7Dias.Count(v => v.VendaTipoPagamento == "Cartão");
            model.DinheiroSemana = vendas7Dias.Count(v => v.VendaTipoPagamento == "Dinheiro");

            model.PixMes = vendas30Dias.Count(v => v.VendaTipoPagamento == "Pix");
            model.CartaoMes = vendas30Dias.Count(v => v.VendaTipoPagamento == "Cartão");
            model.DinheiroMes = vendas30Dias.Count(v => v.VendaTipoPagamento == "Dinheiro");

            model.PixTotal = todasVendas.Count(v => v.VendaTipoPagamento == "Pix");
            model.CartaoTotal = todasVendas.Count(v => v.VendaTipoPagamento == "Cartão");
            model.DinheiroTotal = todasVendas.Count(v => v.VendaTipoPagamento == "Dinheiro");


            // ESTOQUE
            model.SemEstoque = produtos.Count(p => p.ProdutoAtivo && (p.ProdutoQtdEstoque ?? 0) == 0);
            model.EstoqueBaixo = produtos.Count(p => p.ProdutoAtivo && (p.ProdutoQtdEstoque ?? 0) <= p.ProdutoEstoqueMinimo);



            // ===== LOJA ONLINE =====
            var todosPedidos = _pedidoRepository.ObterTodosComDetalhes();

            var statusQueContam = new[] { PedidoModel.Status.Concluido, PedidoModel.Status.CanceladoNaoRetirado };

            var pedidosValidos = todosPedidos.Where(p => statusQueContam.Contains(p.PedidoStatus)).ToList();

            var pedidosHoje = pedidosValidos.Where(p => p.PedidoData.Date == hoje).ToList();
            var pedidosMes = pedidosValidos.Where(p => p.PedidoData >= inicioMes).ToList();

            model.TotalOnlineHoje = pedidosHoje.Sum(p => p.PedidoValorTotal);
            model.TotalOnlineMes = pedidosMes.Sum(p => p.PedidoValorTotal);

            model.LucroOnlineHoje = pedidosHoje.Sum(p =>
                p.Itens.Sum(i => (i.ItemPedidoPreco - i.ItemPedidoCusto) * i.ItemPedidoQtd));

            model.LucroOnlineMes = pedidosMes.Sum(p =>
                p.Itens.Sum(i => (i.ItemPedidoPreco - i.ItemPedidoCusto) * i.ItemPedidoQtd));
            // PAGAMENTOS - ONLINE (Hoje)
            model.QtdPixOnline = pedidosHoje.Count(p => p.PedidoTipoPagamento == PedidoModel.TipoPagamento.Pix);
            model.QtdCartaoOnline = pedidosHoje.Count(p => p.PedidoTipoPagamento == PedidoModel.TipoPagamento.Cartao);

            model.TotalPixOnline = pedidosHoje.Where(p => p.PedidoTipoPagamento == PedidoModel.TipoPagamento.Pix).Sum(p => p.PedidoValorTotal);
            model.TotalCartaoOnline = pedidosHoje.Where(p => p.PedidoTipoPagamento == PedidoModel.TipoPagamento.Cartao).Sum(p => p.PedidoValorTotal);

            // PAGAMENTOS - GERAL Hoje (Presencial + Online)
            model.QtdPixGeral = model.QtdPix + model.QtdPixOnline;
            model.QtdCartaoGeral = model.QtdCartao + model.QtdCartaoOnline;
            model.QtdDinheiroGeral = model.QtdDinheiro; // Dinheiro só existe no Presencial

            model.TotalPixGeral = model.TotalPix + model.TotalPixOnline;
            model.TotalCartaoGeral = model.TotalCartao + model.TotalCartaoOnline;
            model.TotalDinheiroGeral = model.TotalDinheiro; // Dinheiro só existe no Presencial

            // ===== COMBINADO (Presencial + Online) =====
            model.TotalCombinadoHoje = model.TotalHoje + model.TotalOnlineHoje;
            model.TotalCombinadoMes = model.TotalMes + model.TotalOnlineMes;
            model.LucroCombinadoHoje = model.LucroHoje + model.LucroOnlineHoje;
            model.LucroCombinadoMes = model.LucroMes + model.LucroOnlineMes;

            var quantidadeTransacoesTotal = todasVendas.Count + pedidosValidos.Count;
            var somaTotalTransacoes = todasVendas.Sum(v => v.VendaValorTotal) + pedidosValidos.Sum(p => p.PedidoValorTotal);

            model.TicketMedioCombinado = quantidadeTransacoesTotal > 0
                ? somaTotalTransacoes / quantidadeTransacoesTotal
                : 0;
            model.TicketMedioOnline = pedidosValidos.Any() ? pedidosValidos.Average(p => p.PedidoValorTotal) : 0;

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

            // PREVISÃO
            var vendasPorDia = vendas7Dias
                .GroupBy(v => v.VendaData.Date)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(v => v.VendaValorTotal))
                .ToList();

            int n = vendasPorDia.Count;

            if (n > 1)
            {
                double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

                for (int i = 0; i < n; i++)
                {
                    double x = i + 1;
                    double y = (double)vendasPorDia[i];

                    sumX += x;
                    sumY += y;
                    sumXY += x * y;
                    sumX2 += x * x;
                }

                double a = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
                double b = (sumY - a * sumX) / n;

                double previsao = a * (n + 1) + b;

                model.PrevisaoAmanha = (decimal)Math.Max(previsao, 0);
            }
            else
            {
                model.PrevisaoAmanha = 0;
            }

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

                int quantidadeDias = dadosPorDia.Count;

                if (quantidadeDias < 2)
                    continue;

                double sX = 0, sY = 0, sXY = 0, sX2 = 0;

                for (int i = 0; i < quantidadeDias; i++)
                {
                    double x = i + 1;
                    double y = (double)dadosPorDia[i];

                    sX += x;
                    sY += y;
                    sXY += x * y;
                    sX2 += x * x;
                }

                double slope = (quantidadeDias * sXY - sX * sY) / (quantidadeDias * sX2 - sX * sX);

                tendencias.Add((p.ProdutoNome, slope));
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

            int nOnline = pedidosPorDia.Count;

            if (nOnline > 1)
            {
                double sumXo = 0, sumYo = 0, sumXYo = 0, sumX2o = 0;

                for (int i = 0; i < nOnline; i++)
                {
                    double x = i + 1;
                    double y = (double)pedidosPorDia[i];

                    sumXo += x;
                    sumYo += y;
                    sumXYo += x * y;
                    sumX2o += x * x;
                }

                double aOnline = (nOnline * sumXYo - sumXo * sumYo) / (nOnline * sumX2o - sumXo * sumXo);
                double bOnline = (sumYo - aOnline * sumXo) / nOnline;

                double previsaoOnline = aOnline * (nOnline + 1) + bOnline;

                model.PrevisaoAmanhaOnline = (decimal)Math.Max(previsaoOnline, 0);
            }
            else
            {
                model.PrevisaoAmanhaOnline = 0;
            }

            model.PrevisaoAmanhaCombinada = model.PrevisaoAmanha + model.PrevisaoAmanhaOnline;

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


            return model;
        }
    }
}
