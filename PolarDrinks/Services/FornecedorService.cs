using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public class FornecedorService : IFornecedorService
    {
        private readonly IFornecedorRepository _fornecedorRepository;

        public FornecedorService(IFornecedorRepository fornecedorRepository)
        {
            _fornecedorRepository = fornecedorRepository;
        }

        public List<FornecedorModel> ListarFornecedores()
        {
            return _fornecedorRepository.ObterTodos();
        }

        public FornecedorModel? ObterFornecedor(int id)
        {
            return _fornecedorRepository.ObterPorId(id);
        }

        public ResultadoOperacao CadastrarFornecedor(FornecedorModel fornecedor)
        {
            if (_fornecedorRepository.ExisteCNPJ(fornecedor.FornecedorCNPJ!))
            {
                return ResultadoOperacao.Erro(
                    "Este CNPJ já está cadastrado.",
                    campoErro: nameof(FornecedorModel.FornecedorCNPJ));
            }

            _fornecedorRepository.Adicionar(fornecedor);
            _fornecedorRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Fornecedor cadastrado com sucesso!");
        }

        public ResultadoOperacao EditarFornecedor(FornecedorModel fornecedor)
        {
            if (_fornecedorRepository.ExisteCNPJ(fornecedor.FornecedorCNPJ!, fornecedor.FornecedorID))
            {
                return ResultadoOperacao.Erro(
                    "Este CNPJ já está cadastrado.",
                    campoErro: nameof(FornecedorModel.FornecedorCNPJ));
            }

            var fornecedorDb = _fornecedorRepository.ObterPorId(fornecedor.FornecedorID);
            if (fornecedorDb == null)
            {
                return ResultadoOperacao.Erro("Fornecedor não encontrado.");
            }

            fornecedorDb.FornecedorNome = fornecedor.FornecedorNome;
            fornecedorDb.FornecedorCNPJ = fornecedor.FornecedorCNPJ;
            fornecedorDb.FornecedorTelefone = fornecedor.FornecedorTelefone;
            fornecedorDb.FornecedorEmail = fornecedor.FornecedorEmail;
            fornecedorDb.FornecedorCEP = fornecedor.FornecedorCEP;
            fornecedorDb.FornecedorCidade = fornecedor.FornecedorCidade;
            fornecedorDb.FornecedorEstado = fornecedor.FornecedorEstado;
            fornecedorDb.FornecedorBairro = fornecedor.FornecedorBairro;
            fornecedorDb.FornecedorLogradouro = fornecedor.FornecedorLogradouro;
            fornecedorDb.FornecedorNum = fornecedor.FornecedorNum;
            fornecedorDb.FornecedorAtivo = fornecedor.FornecedorAtivo;

            _fornecedorRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok("Fornecedor editado com sucesso!");
        }
    }
}