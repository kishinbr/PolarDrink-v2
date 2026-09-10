using PolarDrinks.Models.Loja;
using PolarDrinks.Repositories.Loja;
using PolarDrinks.Services.Common;

namespace PolarDrinks.Services.Loja
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
            if (string.IsNullOrWhiteSpace(nome))
            {
                return ResultadoOperacao<string>.Erro("Informe seu nome.", campoErro: nameof(nome));
            }

            if (string.IsNullOrWhiteSpace(email))
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

            if (string.IsNullOrWhiteSpace(cpf))
            {
                return ResultadoOperacao<string>.Erro("Informe seu CPF.", campoErro: nameof(cpf));
            }

            if (_clienteRepository.ExisteEmail(email))
            {
                return ResultadoOperacao<string>.Erro("Já existe uma conta com esse e-mail.", campoErro: nameof(email));
            }

            if (_clienteRepository.ExisteCPF(cpf))
            {
                return ResultadoOperacao<string>.Erro("Já existe uma conta com esse CPF.", campoErro: nameof(cpf));
            }

            var cliente = new ClienteModel
            {
                ClienteNome = nome,
                ClienteEmail = email.Trim().ToLower(),
                ClienteSenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                ClienteTelefone = telefone,
                ClienteCPF = cpf,
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