using System.Threading.Tasks;
using Tutorial8.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Tutorial8.Services
{
    public interface IClientsService
    {
        /// <summary>
        /// Tworzy nowego klienta i zwraca jego ID
        /// </summary>
        /// <param name="clientDto">Dane klienta</param>
        /// <returns>ID nowo utworzonego klienta</returns>
        Task<int> CreateClient(CreateClientDTO clientDto);

        /// <summary>
        /// Zwraca listę wycieczek przypisanych do klienta
        /// </summary>
        /// <param name="clientId">ID klienta</param>
        /// <returns>Lista wycieczek lub null jeśli brak danych</returns>
        Task<IEnumerable<TripDTO>?> GetClientTrips(int clientId);

        /// <summary>
        /// Rejestruje klienta na wycieczkę
        /// </summary>
        /// <param name="clientId">ID klienta</param>
        /// <param name="tripId">ID wycieczki</param>
        /// <returns>Wynik operacji jako IActionResult</returns>
        Task<IActionResult> RegisterClientToTrip(int clientId, int tripId);

        /// <summary>
        /// Usuwa klienta z wycieczki
        /// </summary>
        /// <param name="clientId">ID klienta</param>
        /// <param name="tripId">ID wycieczki</param>
        /// <returns>Wynik operacji jako IActionResult</returns>
        Task<IActionResult> RemoveClientFromTrip(int clientId, int tripId);
    }
}