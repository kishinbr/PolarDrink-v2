using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;

namespace PolarDrinks.Compartilhado.Mvc
{
    /// <summary>
    /// Faz o MVC procurar as views dentro da pasta do módulo do controller:
    ///   /Modules/&lt;Modulo&gt;/Views/&lt;Controller&gt;/&lt;View&gt;.cshtml
    /// O módulo é descoberto pelo namespace do controller (PolarDrinks.Modules.&lt;Modulo&gt;.Controllers).
    /// Controllers fora de um módulo (Home) continuam usando a pasta Views/ padrão.
    /// </summary>
    public class ModuloViewLocationExpander : IViewLocationExpander
    {
        private const string Chave = "modulo";
        private const string Marcador = ".Modules.";

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var namespaceDoController = (context.ActionContext.ActionDescriptor as ControllerActionDescriptor)
                ?.ControllerTypeInfo.Namespace;

            if (namespaceDoController == null)
            {
                return;
            }

            var posicao = namespaceDoController.IndexOf(Marcador, StringComparison.Ordinal);

            if (posicao < 0)
            {
                return;
            }

            var aPartirDoModulo = namespaceDoController[(posicao + Marcador.Length)..];

            context.Values[Chave] = aPartirDoModulo.Split('.')[0];
        }

        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(Chave, out var modulo) && !string.IsNullOrEmpty(modulo))
            {
                yield return $"/Modules/{modulo}/Views/{{1}}/{{0}}.cshtml";
            }

            foreach (var local in viewLocations)
            {
                yield return local;
            }
        }
    }
}
