using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Xml.Linq;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;

        //private readonly ILogger<VillaAPIController> _logger;

        public VillaAPIController(/*ILogger<VillaAPIController> logger*/IVillaRepository villaRepository, IMapper mapper)
        {
            _villaRepository = villaRepository;
            _mapper = mapper;
            //_logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
        {
            //_logger.LogInformation("Get All Villas");
            IEnumerable<Villa> villaList = await _villaRepository.GetAllAsync();
            var villas = _mapper.Map<List<VillaDTO>>(villaList);

            return Ok(villas);
        }

        [HttpGet("{id:int}",Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VillaDTO>> GetVilla(int id)
        {
            if (id == 0)
            {
                //_logger.LogError("Error getting Villa Id:" + id);
                return BadRequest();
            }

            var getVilla = await _villaRepository.GetAsync(v => v.Id == id);
            if (getVilla == null)
            {
                return NotFound();
            }

            var villa = _mapper.Map<VillaDTO>(getVilla);

            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO villaCreateDTO)
        {
            if (await _villaRepository.GetAsync(v => v.Name.ToLower() == villaCreateDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError(villaCreateDTO.Name, "Villa Already Exists!");
                return BadRequest(ModelState);
            }

            if (villaCreateDTO == null)
            {
                return BadRequest(villaCreateDTO);
            }

            //if (villaDTO.Id > 0)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError);
            //}

            Villa model = _mapper.Map<Villa>(villaCreateDTO);

            await _villaRepository.CreateAsync(model);
            return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            var villa = await _villaRepository.GetAsync(v => v.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

            await _villaRepository.DeleteAsync(villa);

            return NoContent();
        }


        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO villaUpdateDTO)
        {
            if (villaUpdateDTO == null || id != villaUpdateDTO.Id)
            {
                return BadRequest();
            }

            Villa model = _mapper.Map<Villa>(villaUpdateDTO);

            await _villaRepository.UpdateAsync(model);
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchVillaDTO)
        {
            if (patchVillaDTO == null || id == 0)
            {
                return BadRequest();
            }

            var villa = await _villaRepository.GetAsync(v => v.Id == id, tracked: false);

            VillaUpdateDTO villaDTO =_mapper.Map<VillaUpdateDTO>(villa);

            if (villa == null)
            {
                return BadRequest();
            }

            patchVillaDTO.ApplyTo(villaDTO, ModelState);

            Villa model = _mapper.Map<Villa>(villaDTO);

            await _villaRepository.UpdateAsync(model);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
