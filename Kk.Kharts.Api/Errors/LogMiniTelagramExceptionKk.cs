namespace Kk.Kharts.Api.Errors
{
    public class LogMiniTelagramExceptionKk : InvalidOperationException
    {
        public LogMiniTelagramExceptionKk() : base("Erreurs détectées.") 
        { 
       
        }

        public LogMiniTelagramExceptionKk(string message) : base(message) 
        {
       
        }

        public LogMiniTelagramExceptionKk(string message, Exception innerException) : base(message, innerException) 
        { 
        
        }
    }

}
