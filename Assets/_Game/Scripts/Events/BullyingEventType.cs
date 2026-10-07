namespace BullyingGame.Events
{
    public enum BullyingType
    {
        Verbal,
        Physical,
        Social,
        Cyber
    }

    public enum BullyingEventState
    {
        Inactive,
        Triggered,
        InProgress,
        WaitingResponse,
        Resolved,
        Failed,
        Approaching,
        Confrontation
    }
}
