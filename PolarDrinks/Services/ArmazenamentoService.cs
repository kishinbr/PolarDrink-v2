

namespace PolarDrinks.Services
{
    public class ArmazenamentoService : IArmazenamentoService
    {
        private readonly IWebHostEnvironment _ambiente;
        private const string PastaProdutos = "uploads/produtos";

        public ArmazenamentoService(IWebHostEnvironment ambiente)
        {
            _ambiente = ambiente;
        }

                public string SalvarImagemProduto(Stream conteudo, string nomeArquivo, int produtoId)
        {
            var pastaFisica = Path.Combine(_ambiente.WebRootPath, PastaProdutos);

            if (!Directory.Exists(pastaFisica))
            {
                Directory.CreateDirectory(pastaFisica);
            }

            var extensao = Path.GetExtension(nomeArquivo);
            
            var caminhoFisico = Path.Combine(pastaFisica, nomeArquivo);

            using (var stream = new FileStream(caminhoFisico, FileMode.Create))
            {
                conteudo.CopyTo(stream);
            }

            return $"{PastaProdutos}/{nomeArquivo}";
        }

        public void RemoverImagemProduto(string? caminhoRelativo)
        {
            if (string.IsNullOrWhiteSpace(caminhoRelativo))
                return;

            var caminhoFisico = Path.Combine(_ambiente.WebRootPath, caminhoRelativo);

            if (File.Exists(caminhoFisico))
            {
                File.Delete(caminhoFisico);
            }
        }
    }
}