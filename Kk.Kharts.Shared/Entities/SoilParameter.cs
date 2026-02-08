namespace Kk.Kharts.Shared.Entities
{
    public class SoilParameter
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public float A0 { get; set; }
        public float A1 { get; set; }
        public float Epsilon0 { get; set; }
    }

}
