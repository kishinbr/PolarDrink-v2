namespace PolarDrinks.Modules.Estoque.Services
{
    public class ArmazenamentoService : IArmazenamentoService
    {
        private readonly IWebHostEnvironment _ambiente;
        private const string PastaProdutos = "uploads/produtos";

        private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public ArmazenamentoService(IWebHostEnvironment ambiente)
        {
            _ambiente = ambiente;
        }

        public static bool ExtensaoPermitida(string? nomeArquivo)
        {
            var extensao = Path.GetExtension(nomeArquivo ?? string.Empty).ToLowerInvariant();
            return ExtensoesPermitidas.Contains(extensao);
        }

        public string SalvarImagemProduto(Stream conteudo, string nomeArquivo, int produtoId)
        {
            if (!ExtensaoPermitida(nomeArquivo))
            {
                throw new InvalidOperationException("Formato de imagem não permitido.");
            }

            var pastaFisica = Path.Combine(_ambiente.WebRootPath, PastaProdutos);
            Directory.CreateDirectory(pastaFisica);

            var extensao = Path.GetExtension(nomeArquivo).ToLowerInvariant();
            var nomeSeguro = $"{produtoId}_{Guid.NewGuid():N}{extensao}";
            var caminhoFisico = Path.Combine(pastaFisica, nomeSeguro);

            using (var stream = new FileStream(caminhoFisico, FileMode.CreateNew))
            {
                conteudo.CopyTo(stream);
            }

            return $"{PastaProdutos}/{nomeSeguro}";
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