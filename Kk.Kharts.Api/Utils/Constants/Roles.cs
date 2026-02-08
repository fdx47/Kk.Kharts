namespace Kk.Kharts.Api.Utility.Constants
{
    public static class Roles
    {
        // read write access   
        public const string Root = "Root";
        public const string SuperAdmin = nameof(SuperAdmin);
        public const string Admin = "Admin";
        public const string UserRW = "UserRW";

        // read only access
        public const string User = "User";
        public const string Technician = "Technician";
        public const string Demo = "Demo";
        public const string DemoRandom = "DemoRandom";
        
        // Lista de roles de leitura e escrita
        public static readonly string[] WriteAccessRoles = { Root, SuperAdmin, Admin, UserRW };

        // Lista de roles de leitura
        public static readonly string[] ReadOnlyAccessRoles = { User, Technician, DemoRandom, Demo };

        public static readonly string[] Others = { SuperAdmin, Admin, UserRW, User };

        public static readonly string[] NoRoot = { SuperAdmin, Admin, UserRW, User, Technician, DemoRandom, Demo };
    }
}
