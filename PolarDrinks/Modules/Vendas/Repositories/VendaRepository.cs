using Microsoft.EntityFrameworkCore;
using PolarDrinks.Data;
using PolarDrinks.Modules.Estoque.Models;
using PolarDrinks.Modules.Vendas.Models;

namespace PolarDrinks.Modules.Vendas.Repositories
{
    public class VendaRepository : IVendaRepository
    {
        private readonly ApplicationDbContext _db;

        public VendaRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<VendaModel> ObterPorPeriodo(DateTime? dataInicio, DateTime? dataFim)
        {
            var vendasQuery = _db.Vendas
                .Include(v => v.Itens)
                .AsQueryable();

            if (dataInicio.HasValue)
                vendasQuery = vendasQuery.Where(v => v.VendaData.Date >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                vendasQuery = vendasQuery.Where(v => v.VendaData.Date <= dataFim.Value.Date);

            return vendasQuery.OrderByDescending(v => v.VendaData).ToList();
        }

        public VendaModel? ObterPorId(int id)
        {
            return _db.Vendas
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .Include(v => v.Usuario)
                .FirstOrDefault(v => v.VendaID == id);
        }

        public (string? Descricao, string? UsuarioNome)? ObterMotivoCancelamento(int vendaId)
        {
            var motivo = _db.MovimentacoesEstoque
                .Include(m => m.Usuario)
                .Where(m => m.ItemVenda.VendaID == vendaId
                         && m.MovimentacaoTipo == MovimentacaoEstoqueModel.Tipos.Cancelamento
                         && m.MovimentacaoDescricao != null)
                .Select(m => new
                {
                    m.MovimentacaoDescricao,
                    UsuarioNome = m.Usuario.UsuarioNome
                })
                .FirstOrDefault();

            if (motivo == null)
                return null;

            return (motivo.MovimentacaoDescricao, motivo.UsuarioNome);
        }

        public void Adicionar(VendaModel venda)
        {
            _db.Vendas.Add(venda);
        }
        public List<VendaModel> ObterVendasAtivasComDetalhes()
        {
            return _db.Vendas
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .Where(v => !v.VendaCancelada)
                .ToList();
        }

        public List<VendaModel> ObterVendasCanceladasComDetalhes()
        {
            return _db.Vendas
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .Where(v => v.VendaCancelada)
                .ToList();
        }

        public (int Quantidade, decimal Faturamento, decimal Lucro) ObterResumoFinanceiro(DateTime? inicio, DateTime? fim)
        {
            var vendas = _db.Vendas.Where(v => !v.VendaCancelada);

            if (inicio != null)
            {
                vendas = vendas.Where(v => v.VendaData >= inicio.Value);
            }

            if (fim != null)
            {
                vendas = vendas.Where(v => v.VendaData < fim.Value);
            }

            var quantidade = vendas.Count();
            var faturamento = vendas.Sum(v => v.VendaValorTotal);
            var lucro = vendas
                .SelectMany(v => v.Itens)
                .Sum(i => (i.ItemVendaPreco - i.ItemVendaCusto) * i.ItemVendaQtd);

            return (quantidade, faturamento, lucro);
        }
    }
}