using UnityEngine;

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
    public enum DialogueTrigger
    {
        BacklogUrgency, AssignmentSuccess, AssignmentFailure, ExecutionSuccess, ExecutionFailure,
        RefusalSuccess, RefusalFailure, DayQuotaMet, DayQuotaMissed,
        TaskSuccess, TaskFailure, TaskRejected
    }
    public sealed class TaskInstance
    {
        public readonly int Id;
        public int Day { get; internal set; }
        public readonly TaskData Data;
        public CharacterRole Owner { get; }
        public TaskStatus Status { get; internal set; }
        public TaskStrategy Strategy { get; internal set; }
        public float Remaining { get; internal set; }
        public int Age { get; internal set; }
        public int Wave { get; }
        public int Difficulty { get; }
        public bool ResolvedAutomatically { get; internal set; }
        public bool Refusing { get; internal set; }
        public bool IsFinal => Status == TaskStatus.Completed || Status == TaskStatus.Failed || Status == TaskStatus.Rejected || Status == TaskStatus.Expired;
        public TaskInstance(int id, int day, TaskData data, int age = 0, int wave = 1)
        {
            Id = id;
            Day = day;
            Data = data;
            Owner = data.owner;
            Age = age;
            Wave = wave;
            Difficulty = Mathf.Clamp(wave, 1, 3);
            Status = TaskStatus.Available;
        }
    }
}
