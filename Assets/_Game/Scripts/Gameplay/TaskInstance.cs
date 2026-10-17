namespace Caudal
{
    public enum TaskStatus
    {
        Available, Assigning, PendingResponse, Accepted, InProgress, Completed, Failed, Rejected, Expired
    }
    public enum TaskStrategy
    {
        Normal, Pressure
    }
    public enum InteractionKind
    {
        Assignment, Execution, Refusal
    }
    public enum InteractionResult
    {
        Success, Failure, Cancelled
    }
    public sealed class TaskInstance
    {
        public readonly int Id, Day;
        public readonly TaskData Data;
        public TaskStatus Status { get; internal set; }
        public TaskStrategy Strategy { get; internal set; }
        public float Remaining { get; internal set; }
        public bool Refusing { get; internal set; }
        public bool IsFinal => Status == TaskStatus.Completed || Status == TaskStatus.Failed || Status == TaskStatus.Rejected || Status == TaskStatus.Expired;
        public TaskInstance(int id, int day, TaskData data)
        {
            Id = id;
            Day = day;
            Data = data;
            Status = TaskStatus.Available;
        }
    }
}
