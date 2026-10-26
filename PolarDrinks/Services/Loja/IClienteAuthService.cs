using PolarDrinks.Services.Common;

namespace PolarDrinks.Services.Loja
{
    public interface IClienteAuthService
    {
        ResultadoOperacao<string> Cadastrar(
            string nome, string email, string senha, string confirmacaoSenha,
            string telefone, string cpf);

        ResultadoOperacao<string> Login(string email, string senha);
    }
}