using PolarDrinks.Models;

namespace PolarDrinks.Repositories
{
    public interface ICompraEstoqueRepository
    {
        List<CompraEstoqueModel> ObterPorStatus(string status);
        CompraEstoqueModel? ObterDetalhes(int id);
        CompraEstoqueModel? ObterParaConfirmarEntrega(int id);
        string? ObterUsuarioConfirmacaoEntrada(int compraId);

        void Adicionar(CompraEstoqueModel compra);
        void Remover(CompraEstoqueModel compra);
        void SalvarAlteracoes();
    }
}