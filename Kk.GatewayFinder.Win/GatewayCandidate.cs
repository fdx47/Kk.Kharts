namespace Kk.GatewayFinder.Win
{
    internal sealed class GatewayCandidate
    {
        public GatewayCandidate(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
        }

        public string IpAddress { get; }
        public int Port { get; }
        public bool IsPortOpen { get; set; }
        public bool LooksValid => VerificationDetails?.Length > 0;
        public string? VerificationDetails { get; set; }
    }
}
