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

        // MÉTRICAS NOVAS
        public decimal TicketMedio { get; set; }

        // CANCELAMENTOS
        public int CanceladosHoje { get; set; }
        public int CanceladosSemana { get; set; }
        public int CanceladosMes { get; set; }
        public string? ProdutoMaisCancelado { get; set; }
    }
}