using PolarDrinks.Models;
using PolarDrinks.Repositories;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public List<UsuarioModel> ListarUsuarios()
        {
            return _usuarioRepository.ObterTodosOrdenados();
        }

        public UsuarioModel? ObterUsuario(int id)
        {
            return _usuarioRepository.ObterPorId(id);
        }
        public ResultadoOperacao CadastrarUsuario(
                string usuarioNome, string usuarioLogin, string senha, string confirmacaoSenha,
                string usuarioPerfil, string senhaAtual, string? loginUsuarioLogado)
        {
            if (string.IsNullOrWhiteSpace(usuarioNome))
            {
                return ResultadoOperacao.Erro("Informe o nome do usuário.");
            }

            if (string.IsNullOrWhiteSpace(usuarioLogin))
            {
                return ResultadoOperacao.Erro("Informe o login do usuário.");
            }

            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
            {
                return ResultadoOperacao.Erro("A senha deve ter pelo menos 6 caracteres.");
            }

            if (senha != confirmacaoSenha)
            {
                return ResultadoOperacao.Erro("As senhas não conferem.");
            }

            if (_usuarioRepository.ExisteLogin(usuarioLogin))
            {
                return ResultadoOperacao.Erro("Já existe um usuário com esse login.");
            }

            // valida a senha do usuário logado
            var usuarioLogado = loginUsuarioLogado != null
                ? _usuarioRepository.ObterPorLogin(loginUsuarioLogado)
                : null;

            if (usuarioLogado == null || !BCrypt.Net.BCrypt.Verify(senhaAtual, usuarioLogado.UsuarioSenhaHash))
            {
                return ResultadoOperacao.Erro("Senha atual incorreta.");
            }

            var perfil = usuarioPerfil == "Admin" ? "Admin" : "Funcionario";

            var novo = new UsuarioModel
            {
                UsuarioNome = usuarioNome,
                UsuarioLogin = usuarioLogin.Trim(),
                UsuarioSenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                UsuarioPerfil = perfil,
                UsuarioAtivo = true,
                UsuarioCriadoEm = DateTime.Now
            };

            _usuarioRepository.Adicionar(novo);
            _usuarioRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok($"Usuário '{novo.UsuarioLogin}' criado com sucesso!");
        }
        public ResultadoOperacao AlterarSenha(int id, string senhaAtual, string novaSenha, string confirmacaoSenha)
        {
            var usuario = _usuarioRepository.ObterPorId(id);
            if (usuario == null)
            {
                return ResultadoOperacao.Erro("Usuário não encontrado.");
            }

            if (!BCrypt.Net.BCrypt.Verify(senhaAtual, usuario.UsuarioSenhaHash))
            {
                return ResultadoOperacao.Erro("Senha atual incorreta.");
            }

            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6)
            {
                return ResultadoOperacao.Erro("A nova senha deve ter pelo menos 6 caracteres.");
            }

            if (novaSenha != confirmacaoSenha)
            {
                return ResultadoOperacao.Erro("As senhas não conferem.");
            }

            usuario.UsuarioSenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            _usuarioRepository.SalvarAlteracoes();

            return ResultadoOperacao.Ok($"Senha de '{usuario.UsuarioNome}' alterada com sucesso!");
        }
        public ResultadoOperacao Desativar(int id, string senhaAtual, string? loginUsuarioLogado)
        {
            var usuario = _usuarioRepository.ObterPorId(id);
            if (usuario == null)
            {
                return ResultadoOperacao.Erro("Usuário não encontrado.");
            }

            var usuarioLogado = loginUsuarioLogado != null
                ? _usuarioRepository.ObterPorLogin(loginUsuarioLogado)
                : null;

            if (usuarioLogado == null)
            {
                return ResultadoOperacao.Erro("Usuário não encontrado.");
            }

            if (!BCrypt.Net.BCrypt.Verify(senhaAtual, usuarioLogado.UsuarioSenhaHash))
            {
                return ResultadoOperacao.Erro("Senha incorreta.");
            }

            if (usuario.UsuarioLogin == loginUsuarioLogado && usuario.UsuarioAtivo)
            {
                return ResultadoOperacao.Erro("Você não pode desativar sua própria conta.", campoErro: "REDIRECT");
            }

            if (usuario.UsuarioAtivo && usuario.UsuarioPerfil == "Admin"
                && _usuarioRepository.ContarAdminsAtivos() == 1)
            {
                return ResultadoOperacao.Erro("Não é possível desativar o único administrador ativo.", campoErro: "REDIRECT");
            }

            usuario.UsuarioAtivo = !usuario.UsuarioAtivo;
            _usuarioRepository.SalvarAlteracoes();

            var mensagem = $"Usuário '{usuario.UsuarioNome}' " + (usuario.UsuarioAtivo ? "ativado" : "desativado") + ".";
            return ResultadoOperacao.Ok(mensagem);
        }
    }
}