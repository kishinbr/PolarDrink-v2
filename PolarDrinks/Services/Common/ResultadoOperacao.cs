namespace PolarDrinks.Services.Common
{
    public class ResultadoOperacao
    {
        public bool Sucesso { get; }
        public string? Mensagem { get; }

        public string? CampoErro { get; }

        protected ResultadoOperacao(bool sucesso, string? mensagem, string? campoErro = null)
        {
            Sucesso = sucesso;
            Mensagem = mensagem;
            CampoErro = campoErro;
        }

        public static ResultadoOperacao Ok(string? mensagem = null) => new(true, mensagem);
        public static ResultadoOperacao Erro(string mensagem, string? campoErro = null) => new(false, mensagem, campoErro);
    }
}