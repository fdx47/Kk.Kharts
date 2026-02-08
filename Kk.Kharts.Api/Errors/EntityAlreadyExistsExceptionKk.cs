namespace Kk.Kharts.Api.Errors
{
    public class KkEntityAlreadyExistsException : Exception
    {
        public KkEntityAlreadyExistsException(string message) : base(message) { }
    }
}
