using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;

        public ClientsController(IClientsService clientsService)
        {
            _clientsService = clientsService;
        }
        
        /// Tworzy nowego klienta
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientDTO clientDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newClientId = await _clientsService.CreateClient(clientDto);
            return Created($"api/clients/{newClientId}", new { Id = newClientId });
        }
        
        /// Zwraca listę wycieczek dla konkretnego klienta
        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientTrips(int id)
        {
            var result = await _clientsService.GetClientTrips(id);
            if (result == null)
                return NotFound($"Client with ID {id} not found or has no trips.");

            return Ok(result);
        }
        
        /// Rejestruje klienta na wycieczkę
        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
        {
            var result = await _clientsService.RegisterClientToTrip(id, tripId);
            return result;
        }
        
        /// Usuwa klienta z wycieczki
        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> DeleteClientTrip(int id, int tripId)
        {
            var result = await _clientsService.RemoveClientFromTrip(id, tripId);
            return result;
        }
    }
}