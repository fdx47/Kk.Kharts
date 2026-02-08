using System.ComponentModel.DataAnnotations;
namespace Kk.Kharts.Shared.Entities
{
    public class FullDevice
    {
       
        [Key()]
        public int Id { get; set; }
        public string DevEui { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ApplicationID { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProfileID { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
        public int FCntUp { get; set; }
        public int FCntDown { get; set; }
        public bool SkipFCntCheck { get; set; }
        public string AppKey { get; set; } = string.Empty;
        public string DevAddr { get; set; } = string.Empty;
        public string AppSKey { get; set; } = string.Empty;
        public string NwkSKey { get; set; } = string.Empty;
        public bool SupportsJoin { get; set; }
        public bool Active { get; set; }
        public string LastSeenAt { get; set; } = string.Empty;
        public string MbMode { get; set; } = string.Empty;
        public string MbFramePort { get; set; } = string.Empty;
        public string MbTcpPort { get; set; } = string.Empty;
        public string LastSeenAtTime { get; set; } = string.Empty;
        public int FPort { get; set; }
        public string PayloadCodecID { get; set; } = string.Empty;
        public string PayloadName { get; set; } = string.Empty;       
    }
}
