namespace Kk.Kharts.Shared.Entities
{
    public class Technician
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User User { get; set; } = default!;

        // Relation plusieurs-à-plusieurs avec Device
        public List<TechnicianDevice> TechnicianDevices { get; set; } = new();
    }
}
