using apbd_test1.Exceptions;
using apbd_test1.Models.DTOs;
using apbd_test1.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_test1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public AppointmentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentDetails(int id)
        {
            try
            {
                var res = await _dbService.GetAppointmentDetailsByIdAsync(id);
                return Ok(res);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> AddAppointment([FromBody] AddAppointmentDTO appointmentDTO)
        {
            try
            {
                await _dbService.AddAppointmentAsync(appointmentDTO);
                return CreatedAtAction(nameof(GetAppointmentDetails), new { appointmentDTO.AppointmentId }, null);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

