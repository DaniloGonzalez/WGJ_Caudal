using System.Collections.Generic;
using UnityEngine;
using System;

namespace Caudal
{
    public sealed class TaskManager : MonoBehaviour
    {
        [SerializeField] List<TaskData> catalog = new List<TaskData>();
        [SerializeField, Min(1)] int tasksPerDay = 6;
        [SerializeField] float pressureRewardMultiplier = 1.5f;
        [SerializeField] float pressureStressMultiplier = 2;
        [SerializeField] float failureStress = 2;

        readonly List<TaskInstance> tasks = new List<TaskInstance>();
        int nextId;
        StatsManager stats;
        DayManager days;
        float quotaMultiplier = 1;
        float productivityMultiplier = 1;
        float happinessCostMultiplier = 1;

        public void SetCharacters(CharacterData worker, CharacterData boss)
        {
            // Se recalcula desde cero: reiniciar nunca acumula bonificaciones.
            quotaMultiplier = 1;
            productivityMultiplier = 1;
            happinessCostMultiplier = 1;
            ApplyCharacter(worker);
            ApplyCharacter(boss);
        }

        void ApplyCharacter(CharacterData character)
        {
            if (character == null) return;
            quotaMultiplier += Mathf.Max(0, character.quotaBonusPercent) / 100;
            productivityMultiplier += Mathf.Max(0, character.productivityBonusPercent) / 100;
            happinessCostMultiplier *= 1 - Mathf.Clamp01(character.happinessCostReductionPercent / 100);
        }

        public IReadOnlyList<TaskInstance> Tasks => tasks;
        public TaskInstance Assigning { get; private set; }
        public TaskInstance Pending { get; private set; }
        public TaskInstance Active { get; private set; }
        public string Message { get; private set; } = "";

        public event Action<TaskInstance, InteractionKind> OnInteractionStarted;
        public event Action<TaskInstance, InteractionResult> OnInteractionResolved;

        public void Initialize(StatsManager stats, DayManager days)
        {
            this.stats = stats;
            this.days = days;
        }

        public void StartDay()
        {
            Assigning = null;
            Pending = null;
            Active = null;
            tasks.Clear();
            Message = "Selecciona una tarea para comenzar.";

            var available = new List<TaskData>();
            var ids = new HashSet<string>();
            foreach (var data in catalog)
            {
                if (data == null)
                    continue;
                string id = string.IsNullOrEmpty(data.taskId) ? data.name : data.taskId;
                if (ids.Add(id))
                    available.Add(data);
            }

            // Sacamos cada elegida de la bolsa para no repetirla ese día.
            while (tasks.Count < tasksPerDay && available.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, available.Count);
                tasks.Add(new TaskInstance(++nextId, days.CurrentDay, available[index]));
                available.RemoveAt(index);
            }
        }

        public bool Assign(int index, TaskStrategy strategy)
        {
            if (Assigning != null || Pending != null)
                return false;
            if (index < 0 || index >= tasks.Count)
                return false;
            if (tasks[index].Status != TaskStatus.Available)
                return false;

            Assigning = tasks[index];
            Assigning.Strategy = strategy;
            Assigning.Status = TaskStatus.Assigning;
            Assigning.Remaining = Assigning.Data.assignmentTimeLimit;
            Message = "Asignación en curso.";
            OnInteractionStarted?.Invoke(Assigning, InteractionKind.Assignment);
            return true;
        }

        public void Accept()
        {
            if (Pending == null || Active != null || Pending.Refusing)
                return;

            Active = Pending;
            Pending = null;
            Active.Status = TaskStatus.InProgress;
            Active.Remaining = Active.Data.executionTimeLimit;
            Message = "Tarea aceptada explícitamente.";

            // Gatillar minijuego
            OnInteractionStarted?.Invoke(Active, InteractionKind.Execution);
        }

        public void Reject()
        {
            if (Pending == null || Pending.Refusing || Active != null)
                return;

            bool needsConversation = Pending.Strategy == TaskStrategy.Pressure || Pending.Data.requiresRefusalMinigame;
            if (needsConversation)
            {
                Pending.Refusing = true;
                Pending.Remaining = 10;
                Message = "Rechazo insistente: falta resolver la conversación.";
                OnInteractionStarted?.Invoke(Pending, InteractionKind.Refusal);
                return;
            }

            Pending.Status = TaskStatus.Rejected;
            Pending = null;
            Message = "Solicitud rechazada. Sin producción ni coste de felicidad.";
        }

        // Entrada para el simulador y los futuros minijuegos.
        public bool Resolve(int id, int day, InteractionKind kind, InteractionResult result)
        {
            TaskInstance task;
            switch (kind)
            {
                case InteractionKind.Assignment:
                    task = Assigning;
                    break;
                case InteractionKind.Execution:
                    task = Active;
                    break;
                case InteractionKind.Refusal:
                    task = Pending;
                    break;
                default:
                    return false;
            }

            // No aplicar dos veces un resultado ni aceptar uno de una jornada anterior.
            if (task == null || task.Id != id || task.Day != day || task.IsFinal)
                return false;
            if (kind == InteractionKind.Refusal && !task.Refusing)
                return false;

            switch (kind)
            {
                case InteractionKind.Assignment:
                    FinishAssignment(task, result);
                    break;
                case InteractionKind.Execution:
                    FinishExecution(task, result);
                    break;
                case InteractionKind.Refusal:
                    FinishRefusal(task, result);
                    break;
            }

            // Notificar fin del minijuego
            OnInteractionResolved?.Invoke(task, result);

            return true;
        }

        void FinishAssignment(TaskInstance task, InteractionResult result)
        {
            Assigning = null;
            if (result == InteractionResult.Success)
            {
                task.Status = TaskStatus.PendingResponse;
                Pending = task;
                Message = "Nueva solicitud enviada.";
            }
            else
            {
                task.Status = TaskStatus.Available;
                Message = "Asignación cancelada o fallida; puede reintentarse.";
            }
        }

        void FinishExecution(TaskInstance task, InteractionResult result)
        {
            Active = null;
            if (result != InteractionResult.Success)
            {
                task.Status = TaskStatus.Failed;
                stats.Apply(-failureStress * happinessCostMultiplier, 0);
                Message = "Tarea fallida. Sin producción.";
                return;
            }

            float reward = 1;
            float stress = 1;
            if (task.Strategy == TaskStrategy.Pressure)
            {
                reward = pressureRewardMultiplier;
                stress = pressureStressMultiplier;
            }

            task.Status = TaskStatus.Completed;
            stats.Apply(-task.Data.happinessCost * stress * happinessCostMultiplier,
                task.Data.productivityReward * reward * productivityMultiplier);
            days.AddProduction(task.Data.quotaReward * reward * quotaMultiplier);
            Message = "Tarea completada. Producción registrada.";
        }

        void FinishRefusal(TaskInstance task, InteractionResult result)
        {
            task.Refusing = false;
            if (result == InteractionResult.Success)
            {
                task.Status = TaskStatus.Rejected;
                Pending = null;
                Message = "Rechazo conseguido.";
            }
            else
            {
                if (result == InteractionResult.Failure)
                    stats.Apply(-failureStress * happinessCostMultiplier, 0);
                Message = "La solicitud sigue pendiente. Puedes volver a decidir.";
            }
        }

        public void Tick(float delta)
        {
            TickInteraction(Assigning, InteractionKind.Assignment, delta);
            TickInteraction(Active, InteractionKind.Execution, delta);
            if (Pending != null && Pending.Refusing)
                TickInteraction(Pending, InteractionKind.Refusal, delta);
        }

        void TickInteraction(TaskInstance task, InteractionKind kind, float delta)
        {
            if (task == null)
                return;
            task.Remaining -= delta;
            if (task.Remaining <= 0)
                Resolve(task.Id, task.Day, kind, InteractionResult.Failure);
        }

        public void EndDay()
        {
            foreach (var task in tasks)
            {
                if (!task.IsFinal && task.Status != TaskStatus.Available)
                    task.Status = TaskStatus.Expired;
            }
            Assigning = null;
            Pending = null;
            Active = null;
        }

        public int Count(TaskStatus status)
        {
            int count = 0;
            foreach (var task in tasks)
                if (task.Status == status)
                    count++;
            return count;
        }
    }
}
