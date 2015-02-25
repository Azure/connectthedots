namespace Gateway
{
    public static class Constants
    {
        public const string WindowsServiceName = "GatewayService";

        public static class ResponseStatusCodes
        {
            public const int EnqueueSuccessStatusCode = 0;
            public const int EnqueueFailStatusCode = 1;
        }

        public static class LogMessageTexts
        {
            public const string EnqueueFailText = "Failed to enqueue data.";
            public const string BatchThreadErrorPrefix = "Error at batch sender thread occured: ";
            public const string AMQPSenderErrorPrefix = "Error at AMQP sender occured: ";
        }

        public const int ConcurrentConnections = 4;
        public const int MessagesLoggingThreshold = 1000;
    }
}
