using PolarDrinks.Models;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public interface IUsuarioService
    {
        List<UsuarioModel> ListarUsuarios();
        UsuarioModel? ObterUsuario(int id);

        ResultadoOperacao CadastrarUsuario(
            string usuarioNome, string usuarioLogin, string senha, string confirmacaoSenha,
            string usuarioPerfil, string senhaAtual, string? loginUsuarioLogado);

        ResultadoOperacao AlterarSenha(int id, string senhaAtual, string novaSenha, string confirmacaoSenha);

        ResultadoOperacao Desativar(int id, string senhaAtual, string? loginUsuarioLogado);
    }
}