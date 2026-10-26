using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PolarDrinks.Models.Loja;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PolarDrinks.Services.Loja
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }
        public string GerarToken(ClienteModel cliente)
        {
            var chaveSecreta = _config["Jwt:ChaveSecreta"]!;
            var emissor = _config["Jwt:Emissor"]!;
            var duracaoHoras = double.Parse(_config["Jwt:DuracaoEmHoras"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, cliente.ClienteID.ToString()),
                new Claim(ClaimTypes.Email, cliente.ClienteEmail),
                new Claim(ClaimTypes.Name, cliente.ClienteNome)
            };

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: emissor,
                audience: emissor,
                claims: claims,
                expires: DateTime.Now.AddHours(duracaoHoras),
                signingCredentials: credenciais
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}