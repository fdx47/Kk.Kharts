namespace Kk.Kharts.Shared.Entities
{
    public class UserAlarmRule
    {          
        public int UserId { get; set; }
        public int AlarmRuleId { get; set; }

        public DateTime AssignedDate { get; set; }
        // Propriedades de Navegação para as entidades relacionadas
        public User User { get; set; } = default!;
        public AlarmRule AlarmRule { get; set; } = default!;
    }
}
