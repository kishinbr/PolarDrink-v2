using PolarDrinks.Models.Loja;

namespace PolarDrinks.Services.Loja
{
    public interface ITokenService
    {
        string GerarToken(ClienteModel cliente);
    }
}