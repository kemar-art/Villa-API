using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Villa_Utility;
using Villa_Web.Models;
using Villa_Web.Models.Dto;
using Villa_Web.Services.IServices;

namespace Villa_Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDTO loginRequestDTO = new();
            return View(loginRequestDTO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> login(LoginRequestDTO loginRequestDTO)
        {
            APIResponse aPIResponse = await _authService.LoginAsync<APIResponse>(loginRequestDTO);
            if (aPIResponse != null && aPIResponse.IsSuccess)
            {
                TokenDTO model = JsonConvert.DeserializeObject<TokenDTO>(Convert.ToString(aPIResponse.Result));


                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(model.AccessToken);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(u => u.Type == "unique_name").Value));
                identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);





                _tokenProvider.SetToken(model);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("CostomError", aPIResponse.ErrorsMessages.FirstOrDefault());
                return View(loginRequestDTO);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new (){Text = StaticDetails.Admin, Value = StaticDetails.Admin},
                new (){Text = StaticDetails.Customer, Value = StaticDetails.Customer}
            };
            ViewBag.RoleList = roleList;
            //RegisterationRequestDTO requestDTO = new();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterationRequestDTO requestDTO)
        {
            if (string.IsNullOrEmpty(requestDTO.Role))
            {
                requestDTO.Role = StaticDetails.Customer;
            }
            APIResponse response = await _authService.RegisterAsync<APIResponse>(requestDTO);
            if (response != null && response.IsSuccess)
            {
                return RedirectToAction("Login");
            }

            var roleList = new List<SelectListItem>()
            {
                new (){Text = StaticDetails.Admin, Value = StaticDetails.Admin},
                new (){Text = StaticDetails.Customer, Value = StaticDetails.Customer}
            };
            ViewBag.RoleList = roleList;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}