using System.Collections.Generic;
using PolarDrinks.Models;

namespace PolarDrinks.ViewModels
{
    //essa classe/model é usada para organizar as compras em duas listas, uma para as compras pendentes e outra para as compras concluídas, facilitando a exibição na view
    //ela tem duas propriedades, Pendentes e Concluidas, que são listas de CompraEstoqueModel, e ambas são inicializadas como vazias para evitar null reference
    public class CompraIndexViewModel
    {
        public List<CompraEstoqueModel> Pendentes { get; set; } = new();
        public List<CompraEstoqueModel> Concluidas { get; set; } = new();
    }
}