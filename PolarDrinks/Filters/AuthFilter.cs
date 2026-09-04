using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PolarDrinks.Filters
{
    public class AuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var logado = context.HttpContext.Session.GetString("Logado");

            if (logado != "true")
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}