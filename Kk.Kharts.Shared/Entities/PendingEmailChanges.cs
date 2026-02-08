namespace Kk.Kharts.Shared.Entities
{
    public class PendingEmailChange
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string NewEmail { get; set; }
        public required string Token { get; set; }
        public DateTime RequestedAt { get; set; }
        public bool Confirmed { get; set; } = false;
    }
}
