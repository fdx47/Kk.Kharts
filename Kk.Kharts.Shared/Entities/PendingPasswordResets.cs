namespace Kk.Kharts.Shared.Entities
{
    public class PendingPasswordReset
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public bool Used { get; set; } = false;
    }

}
