namespace Kk.Kharts.Shared.Entities
{
    public class Dashboard
    {
        public int Id { get; set; }
        public string? StateJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Chave estrangeira para o User
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
