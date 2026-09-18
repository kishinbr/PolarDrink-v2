namespace PolarDrinks.Models
{
    public class DashboardViewModel
    {
        // FINANCEIRO
        public decimal TotalHoje { get; set; }
        public decimal TotalMes { get; set; }
        public decimal LucroHoje { get; set; }
        public decimal LucroMes { get; set; }

        // PAGAMENTOS (CARDS - HOJE)
        public int QtdPix { get; set; }
        public int QtdCartao { get; set; }
        public int QtdDinheiro { get; set; }
        public decimal TotalPix { get; set; }
        public decimal TotalCartao { get; set; }
        public decimal TotalDinheiro { get; set; }

        // PAGAMENTOS (GRÁFICO)
        public decimal PixHoje { get; set; }
        public decimal CartaoHoje { get; set; }
        public decimal DinheiroHoje { get; set; }

        public decimal PixSemana { get; set; }
        public decimal CartaoSemana { get; set; }
        public decimal DinheiroSemana { get; set; }

        public decimal PixMes { get; set; }
        public decimal CartaoMes { get; set; }
        public decimal DinheiroMes { get; set; }

        public decimal PixTotal { get; set; }
        public decimal CartaoTotal { get; set; }
        public decimal DinheiroTotal { get; set; }

        public Dictionary<string, Dictionary<string, List<decimal>>> VendasPorCanalEPeriodo { get; set; } = new();

        // ESTOQUE
        public int EstoqueBaixo { get; set; }
        public int SemEstoque { get; set; }

        // PRODUTOS
        public string? ProdutoMaisVendido { get; set; }
        public string? ProdutoMaisLucrativo { get; set; }

        // PREVISÃO
        public decimal PrevisaoAmanha { get; set; }
        public List<string> Top3ProdutosAlta { get; set; } = new();

        // GRÁFICOS
        public List<decimal> VendasHojeLista { get; set; } = new();
        public List<decimal> VendasSemana { get; set; } = new();
        public List<decimal> VendasMesGrafico { get; set; } = new();
        public List<decimal> VendasAno { get; set; } = new();

        public decimal TicketMedio { get; set; }
        public decimal TicketMedioOnline { get; set; }

        // CANCELAMENTOS
        public int CanceladosHoje { get; set; }
        public int CanceladosSemana { get; set; }
        public int CanceladosMes { get; set; }
        public string? ProdutoMaisCancelado { get; set; }
        public string? ProdutoMaisCanceladoTotal { get; set; }

        // LOJA ONLINE
        public decimal TotalOnlineHoje { get; set; }
        public decimal TotalOnlineMes { get; set; }
        public decimal LucroOnlineHoje { get; set; }
        public decimal LucroOnlineMes { get; set; }

        // COMBINADO (Presencial + Online)
        public decimal TotalCombinadoHoje { get; set; }
        public decimal TotalCombinadoMes { get; set; }
        public decimal LucroCombinadoHoje { get; set; }
        public decimal LucroCombinadoMes { get; set; }
        public decimal TicketMedioCombinado { get; set; }
        // COMPARATIVO PRESENCIAL vs ONLINE
        public decimal PercentualPresencialMes { get; set; }
        public decimal PercentualOnlineMes { get; set; }
        // PRODUTOS POR CANAL
        public string? ProdutoMaisVendidoOnline { get; set; }
        public string? ProdutoMaisLucrativoOnline { get; set; }
        public string? ProdutoMaisVendidoTotal { get; set; }
        public string? ProdutoMaisLucrativoTotal { get; set; }

        // CANCELAMENTOS - ONLINE
        public int CanceladosOnlineHoje { get; set; }
        public int CanceladosOnlineSemana { get; set; }
        public int CanceladosOnlineMes { get; set; }
        public string? ProdutoMaisCanceladoOnline { get; set; }

        // CANCELAMENTOS - TOTAL (Presencial + Online)
        public int CanceladosTotalHoje { get; set; }
        public int CanceladosTotalSemana { get; set; }
        public int CanceladosTotalMes { get; set; }
        // OPERAÇÃO DA LOJA ONLINE
        public decimal TaxaNaoRetiradaMes { get; set; }
        public double? TempoMedioRetiradaHoras { get; set; }
        public int? HorarioPicoPedidos { get; set; }
        public decimal FaturamentoPorExpiracaoMes { get; set; }

        // PREVISÃO - ONLINE E COMBINADA
        public decimal PrevisaoAmanhaOnline { get; set; }
        public decimal PrevisaoAmanhaCombinada { get; set; }
        // PAGAMENTOS - ONLINE (Hoje)
        public int QtdPixOnline { get; set; }
        public int QtdCartaoOnline { get; set; }
        public decimal TotalPixOnline { get; set; }
        public decimal TotalCartaoOnline { get; set; }

        // PAGAMENTOS - GERAL Hoje (Presencial + Online)
        public int QtdPixGeral { get; set; }
        public int QtdCartaoGeral { get; set; }
        public int QtdDinheiroGeral { get; set; }
        public decimal TotalPixGeral { get; set; }
        public decimal TotalCartaoGeral { get; set; }
        public decimal TotalDinheiroGeral { get; set; }
        public Dictionary<string, Dictionary<string, PagamentoResumoDto>> PagamentosPorCanalEPeriodo { get; set; } = new();

        // GIRO DE ESTOQUE
        public List<GiroProdutoDto> ProdutosGiroLento { get; set; } = new List<GiroProdutoDto>();

    }
    
    public class GiroProdutoDto
    {
        public string ProdutoNome { get; set; } = string.Empty;
        public int EstoqueAtual { get; set; }
        public double? GiroDias { get; set; }
    }
    public class PagamentoResumoDto
    {
        public int QtdPix { get; set; }
        public int QtdCartao { get; set; }
        public int QtdDinheiro { get; set; }
        public decimal TotalPix { get; set; }
        public decimal TotalCartao { get; set; }
        public decimal TotalDinheiro { get; set; }
    }
}