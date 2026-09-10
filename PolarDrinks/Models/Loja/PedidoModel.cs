using System.ComponentModel.DataAnnotations;
using PolarDrinks.Models;

namespace PolarDrinks.Models.Loja
{
    public class PedidoModel
    {
        [Key]
        public int PedidoID { get; set; }

        [Required]
        [StringLength(4)]
        public string PedidoCodigo { get; set; } = string.Empty;

        public int ClienteID { get; set; }
        public ClienteModel? Cliente { get; set; }

        public DateTime PedidoData { get; set; } = DateTime.Now;

        [Required]
        [StringLength(30)]
        public string PedidoStatus { get; set; } = Status.AguardandoSeparacao;

        public decimal PedidoValorTotal { get; set; }

        public DateTime? PedidoDataSeparado { get; set; }
        public int? UsuarioSeparouID { get; set; }
        public UsuarioModel? UsuarioSeparou { get; set; }

        public DateTime? PedidoDataConcluido { get; set; }
        public int? UsuarioEntregouID { get; set; }
        public UsuarioModel? UsuarioEntregou { get; set; }

        public List<ItemPedidoModel> Itens { get; set; } = new List<ItemPedidoModel>();

        public static class Status
        {
            public const string AguardandoSeparacao = "AguardandoSeparacao";
            public const string Separado = "Separado";
            public const string Concluido = "Concluido";
            public const string CanceladoCliente = "CanceladoCliente";
            public const string CanceladoNaoRetirado = "CanceladoNaoRetirado";
        }
    }
}