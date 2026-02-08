using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.Entities
{
    public class DeviceModel
    {
        //public int Id { get; set; }
        [Key()]
        public int ModelId { get; set; }
        public string? Model { get; set; }
        public string? Description { get; set; }

        //public List<Device> Devices { get; set; } = new();


        public DeviceModel()
        {

        }
    }
}
