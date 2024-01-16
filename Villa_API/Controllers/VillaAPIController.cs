using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Villa_API.Models;
using Villa_API.Models.Dto;

namespace Villa_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<VillaDTO> GetVillas()
        {
            return new List<VillaDTO> {
            new() { Id=1,Name= "Test 1"},
            new() { Id=2,Name= "Test 2"}
            };
        }
    }
}
