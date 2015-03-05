namespace Microsoft.ConnectTheDots.Test
{
    interface ITest
    {
        void Run();
        void Completed( );
        int TotalMessagesSent { get; }
        int TotalMessagesToSend { get; }
    }
}
