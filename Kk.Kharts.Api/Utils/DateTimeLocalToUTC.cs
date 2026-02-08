namespace Kk.Kharts.Api.Utility
{
    public static class DateTimeLocal
    {
        public static DateTime ConvertToUTCOld(this string time)
        {
            try
            {
                DateTimeOffset localTime = DateTimeOffset.Parse(time);  // Parse string para um objeto DateTimeOffset
                return localTime.DateTime;
            }
            catch (FormatException ex)
            {
                throw new ArgumentException($"A string fornecida '{time}' não tem o formato válido de data e hora.", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao converter a string para DateTime.", ex);
            }

        }

        public static DateTime ConvertToUTC(this string time)
        {
            try
            {               
                DateTimeOffset localTime = DateTimeOffset.Parse(time);               
                DateTimeOffset utcTime = localTime.ToUniversalTime();
                return utcTime.DateTime;
            }
            catch (FormatException ex)
            {
                throw new ArgumentException($"A string fornecida '{time}' não tem o formato válido de data e hora.", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao converter a string para DateTime.", ex);
            }
        }
    }

}
