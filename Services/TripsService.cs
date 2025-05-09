
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;
using System.Text.RegularExpressions;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();
        var countriesDict = new Dictionary<int, List<CountryDTO>>();

        const string tripsQuery = @"SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip";
        const string countriesQuery = @"
            SELECT ct.IdTrip, c.Name
            FROM Country_Trip ct
            JOIN Country c ON ct.IdCountry = c.IdCountry";

        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            //Ładowanie krajów na wycieczke
            using (var cmd = new SqlCommand(countriesQuery, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tripId = reader.GetInt32(0);
                    string countryName = reader.GetString(1);
                    if (!countriesDict.ContainsKey(tripId))
                        countriesDict[tripId] = new List<CountryDTO>();
                    countriesDict[tripId].Add(new CountryDTO { Name = countryName });
                }
            }

            //Ładowanie wycieczek
            using (var cmd = new SqlCommand(tripsQuery, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt32(0);
                    trips.Add(new TripDTO
                    {
                        Id = id,
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                        Countries = countriesDict.ContainsKey(id) ? countriesDict[id] : new()
                    });
                }
            }
        }

        return trips;
    }

    public async Task<List<ClientTripDTO>> GetTripsForClient(int clientId)
    {
        var result = new List<ClientTripDTO>();

        const string query = @"
            SELECT t.IdTrip, t.Name, ct.RegisteredAt, ct.PaymentDate
            FROM Client_Trip ct
            JOIN Trip t ON ct.IdTrip = t.IdTrip
            WHERE ct.IdClient = @id";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", clientId);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ClientTripDTO
            {
                TripId = reader.GetInt32(0),
                TripName = reader.GetString(1),
                RegisteredAt = reader.GetDateTime(2),
                PaymentDate = reader.GetDateTime(3)
            });
        }

        return result;
    }

    public async Task<int> CreateClient(CreateClientDTO client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName) ||
            string.IsNullOrWhiteSpace(client.LastName) ||
            !Regex.IsMatch(client.Email, @"^.+@.+\\..+$") ||
            client.Pesel.Length != 11)
            throw new ArgumentException("Invalid client data.");

        const string insert = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                                OUTPUT INSERTED.IdClient
                                VALUES (@fn, @ln, @em, @tel, @pesel)";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(insert, conn);
        cmd.Parameters.AddWithValue("@fn", client.FirstName);
        cmd.Parameters.AddWithValue("@ln", client.LastName);
        cmd.Parameters.AddWithValue("@em", client.Email);
        cmd.Parameters.AddWithValue("@tel", client.Telephone);
        cmd.Parameters.AddWithValue("@pesel", client.Pesel);

        await conn.OpenAsync();
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> RegisterClientToTrip(int clientId, int tripId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var checkClient = new SqlCommand("SELECT 1 FROM Client WHERE IdClient=@id", conn);
        checkClient.Parameters.AddWithValue("@id", clientId);
        if (await checkClient.ExecuteScalarAsync() == null) return false;

        var checkTrip = new SqlCommand("SELECT 1 FROM Trip WHERE IdTrip=@id", conn);
        checkTrip.Parameters.AddWithValue("@id", tripId);
        if (await checkTrip.ExecuteScalarAsync() == null) return false;

        var countCmd = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip=@id", conn);
        countCmd.Parameters.AddWithValue("@id", tripId);
        int count = (int)await countCmd.ExecuteScalarAsync();

        var maxCmd = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip=@id", conn);
        maxCmd.Parameters.AddWithValue("@id", tripId);
        int max = (int)await maxCmd.ExecuteScalarAsync();

        if (count >= max) return false;

        var insert = new SqlCommand(@"INSERT INTO Client_Trip VALUES (@cid, @tid, GETDATE(), NULL)", conn);
        insert.Parameters.AddWithValue("@cid", clientId);
        insert.Parameters.AddWithValue("@tid", tripId);

        return await insert.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> RemoveClientFromTrip(int clientId, int tripId)
    {
        const string check = "SELECT 1 FROM Client_Trip WHERE IdClient=@cid AND IdTrip=@tid";
        const string delete = "DELETE FROM Client_Trip WHERE IdClient=@cid AND IdTrip=@tid";

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var checkCmd = new SqlCommand(check, conn);
        checkCmd.Parameters.AddWithValue("@cid", clientId);
        checkCmd.Parameters.AddWithValue("@tid", tripId);

        if (await checkCmd.ExecuteScalarAsync() == null) return false;

        using var deleteCmd = new SqlCommand(delete, conn);
        deleteCmd.Parameters.AddWithValue("@cid", clientId);
        deleteCmd.Parameters.AddWithValue("@tid", tripId);
        return await deleteCmd.ExecuteNonQueryAsync() > 0;
    }
}
