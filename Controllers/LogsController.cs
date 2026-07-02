using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TourPlanner.API.DTO;
using TourPlanner.Services;

namespace TourPlanner.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly TourLogService _tourLogService;

        public LogsController(TourLogService service)
        {
            _tourLogService = service;
        }

        [HttpPost("")]
        public async Task<ActionResult> CreateTourLog([FromBody] TourLogDTO tourLogDto)
        {
            try
            {
                var tl = await _tourLogService.AddTourLog(tourLogDto);
                return Ok(tl);
            }
            catch (Exception ex)
            {
                // Sauber: Nur die Nachricht als JSON-Objekt zurückgeben
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTourLog([FromBody] TourLogDTO tourLog, int id)
        {
            try
            {
                var t = await _tourLogService.UpdateTourLog(tourLog, id);
                return Ok(t);
            }
            catch (Exception ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTourLog(int id, [FromBody] TourLogDeleteDTO tourId)
        {
            try
            {
                await _tourLogService.DeleteTourLog(id, tourId);
                return Ok();
            }
            catch (Exception ex)
            {
                // Sauber: Nur die Nachricht als JSON-Objekt zurückgeben
                return Conflict(new { error = ex.Message });
            }
        }
    }
}
