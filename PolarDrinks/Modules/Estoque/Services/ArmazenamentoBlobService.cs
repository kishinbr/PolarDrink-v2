using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PolarDrinks.Modules.Estoque.Services
{
    public class ArmazenamentoBlobService : IArmazenamentoService
    {
        private readonly BlobContainerClient _container;

        public ArmazenamentoBlobService(BlobContainerClient container)
        {
            _container = container;
        }

        public string SalvarImagemProduto(Stream conteudo, string nomeArquivo, int produtoId)
        {
            if (!ArmazenamentoService.ExtensaoPermitida(nomeArquivo))
            {
                throw new InvalidOperationException("Formato de imagem não permitido.");
            }

            var extensao = Path.GetExtension(nomeArquivo).ToLowerInvariant();
            var nomeSeguro = $"{produtoId}_{Guid.NewGuid():N}{extensao}";
            var blob = _container.GetBlobClient(nomeSeguro);

            blob.Upload(conteudo, new BlobUploadOptions
            {
                // Faz o navegador exibir a imagem em vez de baixá-la
                HttpHeaders = new BlobHttpHeaders { ContentType = TipoDaImagem(extensao) }
            });

            // Link completo da imagem (https://...), gravado no banco
            return blob.Uri.ToString();
        }

        public void RemoverImagemProduto(string? caminhoRelativo)
        {
            if (string.IsNullOrWhiteSpace(caminhoRelativo))
                return;

            // Só apaga imagens que estão neste contêiner do Blob.
            // Caminhos antigos de pasta local (uploads/produtos/...) são ignorados.
            var prefixo = _container.Uri.ToString().TrimEnd('/') + "/";

            if (!caminhoRelativo.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase))
                return;

            var nomeBlob = Uri.UnescapeDataString(caminhoRelativo.Substring(prefixo.Length));
            _container.GetBlobClient(nomeBlob).DeleteIfExists();
        }

        private static string TipoDaImagem(string extensao) => extensao switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}