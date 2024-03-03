using Azure;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository;
using Villa_API.Repository.IRepository;

namespace Villa_API.Controllers
{
    [Route("api/v{version:apiVersion}/UserAuth")]
    [ApiVersionNeutral]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        protected APIResponse _response;
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _response = new();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var tokenDto = await _userRepository.Login(model);
            if (tokenDto == null || string.IsNullOrEmpty(tokenDto.AccessToken))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorsMessages.Add("Username or password is incorrect");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = tokenDto;
            return Ok(_response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            bool ifUserNameIsUnique = _userRepository.IsUniqueUser(model.UserName);
            if (!ifUserNameIsUnique)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorsMessages.Add("Username already exists");
                return BadRequest(_response);
            }

            var user = await _userRepository.Register(model);
            if (user == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorsMessages.Add("Error while registering");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return Ok(_response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDTO tokenDTO)
        {
            if (ModelState.IsValid)
            {
                var tokenDTOResponse = await _userRepository.RefreshAccessToken(tokenDTO);
                if (tokenDTOResponse == null || string.IsNullOrEmpty(tokenDTOResponse.AccessToken))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorsMessages.Add("Invalid Token");
                    return BadRequest(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = tokenDTOResponse;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.ErrorsMessages.Add("Invalid Input");
                return BadRequest(_response);
            }
            
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] TokenDTO tokenDTO)
        {

            if (ModelState.IsValid)
            {
                await _userRepository.RevokeRefreshToken(tokenDTO);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            _response.IsSuccess = false;
            _response.Result = "Invalid Input";
            return BadRequest(_response);
        }

    }
}
//bool ifUserNameIsUnique = _userRepository.IsUniqueUser(model.UserName);


//var user = await _userRepository.Register(model);
//if (user == null)
//{
//    _response.StatusCode = HttpStatusCode.BadRequest;
//    _response.IsSuccess = false;
//    _response.ErrorsMessages.Add("Error while registering");
//    return BadRequest(_response);
//}

//_response.StatusCode = HttpStatusCode.OK;
//_response.IsSuccess = true;
//return Ok(_response);