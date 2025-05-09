
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripsService _service;

    public TripsController(ITripsService service) => _service = service;
    
    /// Zwraca wszystkie wycieczki z przypisanymi krajami.
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        try
        {
            var trips = await _service.GetTrips();
            return Ok(trips);
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
    
    /// Zwraca wszystkie wycieczki danego klienta.
    [HttpGet("clients/{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        try
        {
            var trips = await _service.GetTripsForClient(id);
            if (!trips.Any()) return NotFound("Brak wycieczek lub klient nie istnieje");
            return Ok(trips);
        }
        catch
        {
            return StatusCode(500);
        }
    }
    
    /// Tworzy nowego klienta.
    [HttpPost("clients")]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDTO dto)
    {
        try
        {
            var id = await _service.CreateClient(dto);
            return Created($"api/clients/{id}", new { id });
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch
        {
            return StatusCode(500);
        }
    }
    
    /// Rejestruje klienta na wycieczkę.
    [HttpPut("clients/{clientId}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClient(int clientId, int tripId)
    {
        if (!await _service.RegisterClientToTrip(clientId, tripId))
            return BadRequest("Nie można zarejestrować klienta na tę wycieczkę.");
        return Ok("Zarejestrowano klienta na wycieczkę.");
    }
    
    /// Usuwa klienta z wycieczki.
    [HttpDelete("clients/{clientId}/trips/{tripId}")]
    public async Task<IActionResult> UnregisterClient(int clientId, int tripId)
    {
        if (!await _service.RemoveClientFromTrip(clientId, tripId))
            return NotFound("Nie znaleziono rejestracji.");
        return NoContent();
    }
}

