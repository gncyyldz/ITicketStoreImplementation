using System.Security.Claims;
using ITicketStoreImplementation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ITicketStoreImplementation.Controllers
{
    public class AccountController : Controller
    {
        // GET /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginViewModel { ReturnUrl = returnUrl });

        // POST /Account/Login
        // Demo amaçlı: boş olmayan her kullanıcı adı/parola kabul edilir.
        // SignInAsync çağrısı, SessionStore (ITicketStore) üzerinden StoreAsync'i tetikler.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, model.UserName!),
                new(ClaimTypes.NameIdentifier, model.UserName!)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // POST /Account/Logout
        // SignOutAsync, SessionStore (ITicketStore) üzerinden RemoveAsync'i tetikler.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
