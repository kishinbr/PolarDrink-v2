using PolarDrinks.Modules.Loja.Models;

namespace PolarDrinks.Modules.Loja.Services
{
    public interface ITokenService
    {
        string GerarToken(ClienteModel cliente);
    }
}