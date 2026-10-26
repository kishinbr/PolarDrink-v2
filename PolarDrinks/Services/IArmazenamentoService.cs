using Microsoft.AspNetCore.Http;

namespace PolarDrinks.Services
{
    public interface IArmazenamentoService
    {
        string SalvarImagemProduto(IFormFile arquivo, int produtoId);
        void RemoverImagemProduto(string? caminhoRelativo);
    }
}