using PolarDrinks.Models.Loja;
using PolarDrinks.Repositories;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriaService(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        public List<CategoriaModel> ListarCategorias()
        {
            return _categoriaRepository.ObterTodas();
        }

        public List<CategoriaModel> ListarAtivas()
        {
            return _categoriaRepository.ObterAtivas();
        }

        public CategoriaModel? ObterCategoria(int id)
        {
            return _categoriaRepository.ObterPorId(id);
        }

        public ResultadoOperacao CadastrarCategoria(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return ResultadoOperacao.Erro("Informe o nome da categoria.", campoErro: nameof(CategoriaModel.CategoriaNome));
            }

            if (_categoriaRepository.ExisteNome(nome))
            {
                return ResultadoOperacao.Erro("Já existe uma categoria com esse nome.", campoErro: nameof(CategoriaModel.CategoriaNome));
            }

            var categoria = new CategoriaModel
            {
                CategoriaNome = nome,
                CategoriaAtiva = true
            };

            _categoriaRepository.Adicionar(categoria);
            _categoriaRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Categoria cadastrada com sucesso!");
        }

        public ResultadoOperacao EditarCategoria(int id, string nome, bool ativa)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return ResultadoOperacao.Erro("Informe o nome da categoria.", campoErro: nameof(CategoriaModel.CategoriaNome));
            }

            if (_categoriaRepository.ExisteNome(nome, id))
            {
                return ResultadoOperacao.Erro("Já existe uma categoria com esse nome.", campoErro: nameof(CategoriaModel.CategoriaNome));
            }

            var categoria = _categoriaRepository.ObterPorId(id);
            if (categoria == null)
            {
                return ResultadoOperacao.Erro("Categoria não encontrada.");
            }

            categoria.CategoriaNome = nome;
            categoria.CategoriaAtiva = ativa;

            _categoriaRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Categoria atualizada com sucesso!");
        }
    }
}