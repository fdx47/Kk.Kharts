namespace Kk.Kharts.Shared.Entities
{
    public class TechnicianDevice
    {
        public int TechnicianId { get; set; }
        public Technician Technician { get; set; } = default!;

        public int DeviceId { get; set; }
        public Device Device { get; set; } = default!;
    }
}
