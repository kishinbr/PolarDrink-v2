using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Modules.Loja.Models;
using PolarDrinks.Modules.Loja.Repositories;

namespace PolarDrinks.Modules.Loja.Services
{
    public class ClienteAuthService : IClienteAuthService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ITokenService _tokenService;

        public ClienteAuthService(IClienteRepository clienteRepository, ITokenService tokenService)
        {
            _clienteRepository = clienteRepository;
            _tokenService = tokenService;
        }

        public ResultadoOperacao<string> Cadastrar(
            string nome, string email, string senha, string confirmacaoSenha,
            string telefone, string cpf)
        {

            var emailNormalizado = (email ?? string.Empty).Trim().ToLower();
            var cpfNormalizado = new string((cpf ?? string.Empty).Where(char.IsDigit).ToArray());

            if (string.IsNullOrWhiteSpace(nome))
            {
                return ResultadoOperacao<string>.Erro("Informe seu nome.", campoErro: nameof(nome));
            }

            if (emailNormalizado.Length == 0)
            {
                return ResultadoOperacao<string>.Erro("Informe seu e-mail.", campoErro: nameof(email));
            }

            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
            {
                return ResultadoOperacao<string>.Erro("A senha deve ter pelo menos 6 caracteres.", campoErro: nameof(senha));
            }

            if (senha != confirmacaoSenha)
            {
                return ResultadoOperacao<string>.Erro("As senhas não conferem.", campoErro: nameof(confirmacaoSenha));
            }

            if (cpfNormalizado.Length == 0)
            {
                return ResultadoOperacao<string>.Erro("Informe seu CPF.", campoErro: nameof(cpf));
            }

            if (cpfNormalizado.Length != 11)
            {
                return ResultadoOperacao<string>.Erro("O CPF deve ter 11 dígitos.", campoErro: nameof(cpf));
            }

            if (_clienteRepository.ExisteEmail(emailNormalizado))
            {
                return ResultadoOperacao<string>.Erro("Já existe uma conta com esse e-mail.", campoErro: nameof(email));
            }

            if (_clienteRepository.ExisteCPF(cpfNormalizado))
            {
                return ResultadoOperacao<string>.Erro("Já existe uma conta com esse CPF.", campoErro: nameof(cpf));
            }

            var cliente = new ClienteModel
            {
                ClienteNome = nome.Trim(),
                ClienteEmail = emailNormalizado,
                ClienteSenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                ClienteTelefone = telefone,
                ClienteCPF = cpfNormalizado,
                ClienteCriadoEm = DateTime.Now,
                ClienteAtivo = true
            };

            _clienteRepository.Adicionar(cliente);
            _clienteRepository.SalvarAlteracoes();

            var token = _tokenService.GerarToken(cliente);

            return ResultadoOperacao<string>.Ok(token, "Conta criada com sucesso!");
        }
        public ResultadoOperacao<string> Login(string email, string senha)
        {
            var cliente = _clienteRepository.ObterPorEmail(email.Trim().ToLower());

            if (cliente == null || !BCrypt.Net.BCrypt.Verify(senha, cliente.ClienteSenhaHash))
            {
                return ResultadoOperacao<string>.Erro("E-mail ou senha incorretos.");
            }

            if (!cliente.ClienteAtivo)
            {
                return ResultadoOperacao<string>.Erro("Esta conta está inativa.");
            }

            var token = _tokenService.GerarToken(cliente);

            return ResultadoOperacao<string>.Ok(token, "Login realizado com sucesso!");
        }
    }
}