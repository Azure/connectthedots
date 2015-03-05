namespace Microsoft.ConnectTheDots.Gateway
{
    public static class Constants
    {
        public const string WindowsServiceName = "GatewayService";

        public static class ResponseStatusCodes
        {
            public const int EnqueueSuccessStatusCode = 0;
            public const int EnqueueFailStatusCode = 1;
        }

        public const int ConcurrentConnections = 4;
        public const int MessagesLoggingThreshold = 1000;
    }
}
