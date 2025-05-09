namespace Tutorial8.Models.DTOs;

public class ClientTripDTO
{
    public int TripId { get; set; }
    public string TripName { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
}