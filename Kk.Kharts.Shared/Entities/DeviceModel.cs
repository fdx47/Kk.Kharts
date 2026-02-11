using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.Entities
{
    public class DeviceModel
    {
        [Key()]
        public int ModelId { get; set; }
        public string? Model { get; set; }
        public string? Description { get; set; }

        public DeviceModel()
        {

        }
    }
}
