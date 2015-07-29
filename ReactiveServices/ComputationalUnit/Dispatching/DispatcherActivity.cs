namespace ReactiveServices.ComputationalUnit.Dispatching
{
    public enum DispatcherActivity
    {
        DispatcherInitialized = 0,
        JobWasReceived = 1,
        JobWasDispatched = 2,
        JobWasRejected = 3,
        JobWasRepublished = 4,
        JobInProgressWasIdentified = 5,
        WorkStillInProgress = 6,
        JobWasCompleted = 7,
        RequestWorkerWasFinalized = 8,
        CheckingForUnfinishedJobs = 9,
        PoisonPillWasReceived = 10,
        PoisonPillWasTaken = 11,
        BackgroundThreadException = 12,
        AbortedAllWorkerThreads = 13,
        WorkerNotSendingLifeSignal = 14
    }
}
