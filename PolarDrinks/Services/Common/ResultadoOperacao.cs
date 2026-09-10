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
    public class ResultadoOperacao<T> : ResultadoOperacao
    {
        public T? Dado { get; }

        private ResultadoOperacao(bool sucesso, string? mensagem, T? dado, string? campoErro = null)
            : base(sucesso, mensagem, campoErro)
        {
            Dado = dado;
        }

        public static ResultadoOperacao<T> Ok(T dado, string? mensagem = null) => new(true, mensagem, dado);
        public static new ResultadoOperacao<T> Erro(string mensagem, string? campoErro = null) => new(false, mensagem, default, campoErro);
    }
}