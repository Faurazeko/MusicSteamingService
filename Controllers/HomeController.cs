using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingService.Data;
using MusicStreamingService.Models;
using System.Diagnostics;
using System.Security.Claims;

#pragma warning disable CS8602

namespace MusicSteamingService.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository _repository;

        private const string _loginErrorString = "Error. Username or password is incorrect :(";

        public HomeController(IRepository repository, IMapper mapper)
        {
            _repository = repository;
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult Index() => View();

        public IActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return Redirect("/");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        public IActionResult Denied() => View();

        [NonAction]
        private ClaimsPrincipal GenerateClaimsPrincipal(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim("username", user.Username),
                new Claim("loginUtcDateTime", DateTime.UtcNow.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Name, user.Username),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimsPrincipal;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return Redirect("/");

            var user = _repository.GetUser(username);

            if (user == null)
            {
                TempData["Error"] = _loginErrorString;
                return Redirect("login");
            }

            var isCredentialsCorrect = user.Username == username && user.Password == password;

            if (isCredentialsCorrect)
            {
                if (string.IsNullOrEmpty(returnUrl))
                    returnUrl = "/";

                var claims = GenerateClaimsPrincipal(user);
                await HttpContext.SignInAsync(claims);

                return Redirect(returnUrl);
            }

            TempData["Error"] = _loginErrorString;
            return Redirect("login");
        }

        [HttpGet]
        [Route("Logout")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpGet("UserSettings")]
        public IActionResult UserSettings() => View();
    }
}