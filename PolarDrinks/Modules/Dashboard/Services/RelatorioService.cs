using PolarDrinks.Compartilhado.Common;
using PolarDrinks.Modules.Dashboard.Models;
using PolarDrinks.Modules.Dashboard.Repositories;

namespace PolarDrinks.Modules.Dashboard.Services
{
    public class RelatorioService : IRelatorioService
    {
        private static readonly string[] CanaisValidos = { "presencial", "online", "geral" };

        private readonly IRelatorioRepository _relatorioRepository;

        public RelatorioService(IRelatorioRepository relatorioRepository)
        {
            _relatorioRepository = relatorioRepository;
        }

        public ResultadoOperacao<ResumoVendasDto> GerarResumoPorPeriodo(string? canal, DateTime? inicio, DateTime? fim)
        {
            if (canal == null || !CanaisValidos.Contains(canal))
            {
                return ResultadoOperacao<ResumoVendasDto>.Erro("Canal inválido.");
            }

            if (inicio == null || fim == null)
            {
                return ResultadoOperacao<ResumoVendasDto>.Erro("Informe a data de início e a data de fim.");
            }

            if (inicio.Value.Date > fim.Value.Date)
            {
                return ResultadoOperacao<ResumoVendasDto>.Erro("A data de início não pode ser maior que a data de fim.");
            }

            var resumo = _relatorioRepository.ObterResumoPorPeriodo(canal, inicio.Value.Date, fim.Value.Date);

            return ResultadoOperacao<ResumoVendasDto>.Ok(resumo);
        }
    }
}