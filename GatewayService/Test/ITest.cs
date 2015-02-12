namespace Test
{
    interface ITest
    {
        void Run();
        void Completed();
        int TotalMessagesSent { get; }
    }
}
