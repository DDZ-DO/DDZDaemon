namespace DDZManager.CronService
{
    public interface ICronJob
    {
        public Guid Id { get; }
        public DateTimeOffset NextExecution { get; }
        public bool IsEnabled { get; }
        public void CalculateNextExecution();
        
        public Task Start();

    }
}
