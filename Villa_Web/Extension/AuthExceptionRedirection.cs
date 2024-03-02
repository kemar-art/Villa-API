using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Villa_Web.Services;

namespace Villa_Web.Extension
{
    public class AuthExceptionRedirection : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is AuthException) 
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
