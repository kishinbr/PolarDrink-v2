using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PolarDrinks.Compartilhado.Filters
{
    public class AdminFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var perfil = context.HttpContext.Session.GetString("Perfil");

            if (perfil != "Admin")
            {
                context.Result = new RedirectToActionResult("AcessoNegado", "Auth", null);
            }
        }
    }
}