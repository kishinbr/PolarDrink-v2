namespace PolarDrinks.Modules.Estoque.Services
{
    public interface IArmazenamentoService
    {
        string SalvarImagemProduto(Stream conteudo, string nomeArquivo, int produtoId);
        void RemoverImagemProduto(string? caminhoRelativo);
    }
}