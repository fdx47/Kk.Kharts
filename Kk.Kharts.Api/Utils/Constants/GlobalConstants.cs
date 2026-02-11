namespace Kk.Kharts.Api.Utility.Constants
{

    public static class GlobalConstants
    {
        public const string SharedSchema = "kk_shared";
        public const string DevSchemaOLD = "kk_dev";
        public const string Issuer = "kk.kharts.server";
        public const string Audience = "kk.kharts.client";

        /// <summary>
        /// Nom du répertoire de logs applicatifs (relatif à AppContext.BaseDirectory).
        /// </summary>
        public const string LogsDirectoryName = "kklogs";

        /// <summary>
        /// ID de la société KropKontrol (utilisé pour les comptes démo et les devices de démonstration).
        /// </summary>
        public const int KropKontrolCompanyId = 20;
    }
}