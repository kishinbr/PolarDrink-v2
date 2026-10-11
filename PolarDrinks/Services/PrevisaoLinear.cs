namespace PolarDrinks.Services
{
    /// <summary>
    /// Regressão linear simples (mínimos quadrados) usada nas previsões do dashboard.
    ///
    /// Método: ajusta a reta y = a·x + b sobre uma série de valores em ordem cronológica,
    /// onde x é a POSIÇÃO do valor na série (1, 2, 3, ...) e y é o valor.
    /// A previsão do "próximo ponto" é a reta avaliada em x = n + 1.
    ///
    /// Limites conhecidos:
    ///  - Com menos de 2 pontos não há tendência: a previsão é 0 e a inclinação é nula.
    ///  - Previsões negativas são truncadas em 0.
    ///  - A série contém apenas os dias que TIVERAM movimento. Dias sem venda não entram
    ///    como zero, então um intervalo parado no meio do período "some" da tendência.
    /// </summary>
    public static class PrevisaoLinear
    {
        // Retorna (inclinação a, intercepto b), ou null quando a série tem menos de 2 pontos.
        public static (double Inclinacao, double Intercepto)? Ajustar(IReadOnlyList<double> valores)
        {
            int n = valores.Count;

            if (n < 2)
            {
                return null;
            }

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                double x = i + 1;
                double y = valores[i];

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            double a = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double b = (sumY - a * sumX) / n;

            return (a, b);
        }

        // Valor previsto para o próximo ponto da série (0 se a série tiver menos de 2 pontos).
        public static decimal PreverProximo(IEnumerable<decimal> valores)
        {
            var serie = valores.Select(v => (double)v).ToList();
            var ajuste = Ajustar(serie);

            if (ajuste == null)
            {
                return 0;
            }

            double previsao = ajuste.Value.Inclinacao * (serie.Count + 1) + ajuste.Value.Intercepto;

            return (decimal)Math.Max(previsao, 0);
        }

        // Inclinação da tendência (positiva = alta), ou null se a série tiver menos de 2 pontos.
        public static double? Inclinacao(IEnumerable<int> valores)
        {
            var serie = valores.Select(v => (double)v).ToList();

            return Ajustar(serie)?.Inclinacao;
        }
    }
}