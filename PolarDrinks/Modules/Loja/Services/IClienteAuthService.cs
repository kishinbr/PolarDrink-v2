using PolarDrinks.Compartilhado.Common;

namespace PolarDrinks.Modules.Loja.Services
{
    public interface IClienteAuthService
    {
        ResultadoOperacao<string> Cadastrar(
            string nome, string email, string senha, string confirmacaoSenha,
            string telefone, string cpf);

        ResultadoOperacao<string> Login(string email, string senha);
    }
}