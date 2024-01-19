using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;

namespace Villa_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public VillaNumberAPIController(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<VillaNumberDTO>> GetAllVillaNumbersAsync()
        {
            IEnumerable<VillaNumber> VillaNumbers = await _dbContext.VillaNumbers.ToListAsync();
            var villaNumber = _mapper.Map<List<VillaNumberDTO>>(VillaNumbers);
            return Ok(villaNumber);
        }


        [HttpGet("{id:int}", Name = "GetByIdVillaNumbersAsync")]
        public async Task<ActionResult<VillaNumberDTO>> GetByIdVillaNumbersAsync(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var villaNumber = await _dbContext.VillaNumbers.FirstOrDefaultAsync(x => x.VillaNo == id);
            if (villaNumber == null)
            {
                return NoContent();
            }

            var mapVillaNumber = _mapper.Map<VillaNumberDTO>(villaNumber);

            return Ok(mapVillaNumber);
        }
        
        [HttpPost]
        public async Task<ActionResult<VillaNumberCreateDTO>> CreateVillaNumbersAsync([FromBody] VillaNumberCreateDTO villaNumberCreateDTO)
        {
            if (await _dbContext.VillaNumbers.FirstOrDefaultAsync(x => x.VillaNo == villaNumberCreateDTO.VillaNo) != null)
            {
                ModelState.AddModelError(villaNumberCreateDTO.VillaNo.ToString(), "The Room Number Already Exists!");
                return BadRequest(ModelState);
            }

            if (villaNumberCreateDTO == null)
            {
                return BadRequest(villaNumberCreateDTO);
            }

            VillaNumber villaNumber = _mapper.Map<VillaNumber>(villaNumberCreateDTO);

            await _dbContext.VillaNumbers.AddAsync(villaNumber);
            await _dbContext.SaveChangesAsync();

            return Ok(villaNumber);
        }

        [HttpDelete("{id:int}", Name = "DeleteVillaNumbersAsync")]
        public async Task<ActionResult<VillaNumber>> DeleteVillaNumbersAsync(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var villaNumber = await _dbContext.VillaNumbers.FirstOrDefaultAsync(x => x.VillaNo == id); 
            if (villaNumber == null)
            {
                return NotFound(villaNumber);
            }

            _dbContext.VillaNumbers.Remove(villaNumber);
            await _dbContext.SaveChangesAsync();

            return Ok(villaNumber);
        }

        [HttpPut("{id:int}", Name = "")]
        public async Task<ActionResult<VillaNumberUpdateDTO>> UpdateVillaNumbersAsync(int id, [FromBody] VillaNumberUpdateDTO villaNumberUpdateDTO)
        {
            if (villaNumberUpdateDTO == null || id != villaNumberUpdateDTO.VillaNo)
            {
                return BadRequest(villaNumberUpdateDTO);
            }

            VillaNumber villaNumber = _mapper.Map<VillaNumber>(villaNumberUpdateDTO);

            _dbContext.VillaNumbers.Update(villaNumber);
            await _dbContext.SaveChangesAsync();
            return Ok(villaNumber);
        }
    }
}
