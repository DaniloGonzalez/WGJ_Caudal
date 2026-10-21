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
        [SerializeField, Min(2)] int backlogUrgencyThreshold = 2;
        [SerializeField, Min(1)] int maxBacklogPerRole = 3;
        [SerializeField, Min(0), Tooltip("Balance provisional: coste de Bienestar por una tarea necesaria que vence.")]
        float missedTaskHappinessPenalty = 5;
        [Header("Oleadas sincronizadas")]
        [SerializeField, Min(1)] float waveDuration = 15f;
        const float taskStatChange = 5f;

        readonly List<TaskInstance> tasks = new List<TaskInstance>();
        readonly Dictionary<CharacterRole, List<TaskInstance>> taskBoards = new Dictionary<CharacterRole, List<TaskInstance>>();
        readonly List<string> expiredTasksThisDay = new List<string>();
        readonly HashSet<TaskData> previousWaveTasks = new HashSet<TaskData>();
        int nextId;
        StatsManager stats;
        DayManager days;
        float quotaMultiplier = 1;
        float productivityMultiplier = 1;
        float happinessCostMultiplier = 1;
        bool backlogDialogueShown;
        CharacterRole activeRole;
        bool waveMode, waveExpired;
        bool waveTransitionPending, dayEnding;
        int waveNumber;
        float waveTimeRemaining;
        public int FailedThisDay { get; private set; }
        public int ExpiredThisDay { get; private set; }
        public int InactiveCompletedThisDay { get; private set; }
        public int CompletedThisDay { get; private set; }
        public int RejectedThisDay { get; private set; }
        public int WaveNumber => waveNumber;
        public float WaveTimeRemaining => waveTimeRemaining;
        public int WaveDifficulty => Mathf.Clamp(waveNumber, 1, 3);
        public bool WaveTransitionPending => waveTransitionPending;
        public bool DayEnding => dayEnding;
        public bool HasActiveMinigame => Active != null;
        public string ExpiredSummary => expiredTasksThisDay.Count == 0 ? "Ninguna" : string.Join(", ", expiredTasksThisDay);

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
        public int BacklogCount(CharacterRole role)
        {
            if (!taskBoards.TryGetValue(role, out var board)) return 0;
            int count = 0;
            foreach (var task in board)
                if (!task.IsFinal) count++;
            return count;
        }

        public string BacklogSummary(CharacterRole role)
        {
            if (!taskBoards.TryGetValue(role, out var board) || board.Count == 0) return "Sin pendientes";
            var names = new List<string>();
            foreach (var task in board)
            {
                if (task.IsFinal) continue;
                string urgency = task.Age >= 2 ? " (crítica)" : task.Age == 1 ? " (urgente)" : "";
                names.Add(task.Data.taskName + urgency);
            }
            return names.Count == 0 ? "Sin pendientes" : string.Join(", ", names);
        }
        public TaskInstance Assigning { get; private set; }
        public TaskInstance Pending { get; private set; }
        public TaskInstance Active { get; private set; }
        public float ProductivityMultiplier => productivityMultiplier;
        public string Message { get; private set; } = "";
        public int UnresolvedCount
        {
            get
            {
                int count = 0;
                foreach (var task in tasks)
                    if (IsUnresolved(task)) count++;
                return count;
            }
        }
        public bool BacklogUrgent => UnresolvedCount >= backlogUrgencyThreshold;
        public TaskInstance OldestUnresolvedTask
        {
            get
            {
                TaskInstance oldest = null;
                foreach (var task in tasks)
                    if (IsUnresolved(task) && (oldest == null || task.Id < oldest.Id)) oldest = task;
                return oldest;
            }
        }

        public event Action<TaskInstance, InteractionKind> OnInteractionStarted;
        public event Action<TaskInstance, InteractionKind, InteractionResult> OnInteractionResolved;
        public event Action OnBacklogUrgencyReached;

        public void Initialize(StatsManager stats, DayManager days)
        {
            this.stats = stats;
            this.days = days;
        }

        public void StartDay() => StartDay(CharacterRole.Worker);

        public void StartWaveDay(CharacterRole role)
        {
            waveMode = true;
            activeRole = role;
            Assigning = Pending = Active = null;
            tasks.Clear();
            previousWaveTasks.Clear();
            FailedThisDay = ExpiredThisDay = InactiveCompletedThisDay = 0;
            CompletedThisDay = RejectedThisDay = 0;
            expiredTasksThisDay.Clear();
            waveNumber = 1;
            waveTimeRemaining = waveDuration;
            waveExpired = waveTransitionPending = dayEnding = false;
            Message = "Resuelve las tres tareas de la oleada.";
            CreateWave();
        }

        void CreateWave()
        {
            tasks.Clear();
            var productivityTasks = new List<TaskData>();
            var happinessTasks = new List<TaskData>();
            var minigames = new HashSet<GameObject>();
            foreach (var data in catalog)
            {
                if (data == null || data.owner != activeRole) continue;
                var prefab = data.GetMinigamePrefab(activeRole);
                if (prefab == null || !minigames.Add(prefab)) continue;
                if (data.affectedStat == TaskStat.Productivity)
                    productivityTasks.Add(data);
                else
                    happinessTasks.Add(data);
            }

            int productivityCount = activeRole == CharacterRole.Boss ? 2 : 1;
            int happinessCount = 3 - productivityCount;
            if (productivityTasks.Count < productivityCount || happinessTasks.Count < happinessCount)
            {
                Debug.LogError($"TaskManager: faltan minijuegos para completar el balance de Productividad/Bienestar de {activeRole}.", this);
                return;
            }

            var templates = new List<TaskData>(3);
            templates.AddRange(SelectWaveTasks(productivityTasks, productivityCount));
            templates.AddRange(SelectWaveTasks(happinessTasks, happinessCount));
            Shuffle(templates);

            for (int i = 0; i < templates.Count; i++)
                tasks.Add(new TaskInstance(++nextId, days.CurrentDay, templates[i], 0, waveNumber));

            previousWaveTasks.Clear();
            foreach (var task in templates)
                previousWaveTasks.Add(task);
        }

        List<TaskData> SelectWaveTasks(List<TaskData> candidates, int count)
        {
            var available = new List<TaskData>();
            foreach (var candidate in candidates)
                if (!previousWaveTasks.Contains(candidate))
                    available.Add(candidate);

            // Reuse an earlier task only if the role has too few alternatives.
            if (available.Count < count)
                available = new List<TaskData>(candidates);

            var selected = new List<TaskData>(count);
            for (int i = 0; i < count; i++)
            {
                int index = UnityEngine.Random.Range(0, available.Count);
                selected.Add(available[index]);
                available.RemoveAt(index);
            }
            return selected;
        }

        static void Shuffle(List<TaskData> items)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }

        public bool BeginNextWave()
        {
            if (!waveTransitionPending || dayEnding) return false;
            waveNumber++;
            waveTimeRemaining = waveDuration;
            waveExpired = false;
            CreateWave();
            return tasks.Count == 3;
        }

        public void FinishWaveTransition()
        {
            if (!dayEnding) waveTransitionPending = false;
        }

        void RequestWaveTransition()
        {
            if (waveMode && !dayEnding) waveTransitionPending = true;
        }

        bool AllWaveTasksFinal()
        {
            if (tasks.Count == 0) return false;
            foreach (var task in tasks)
                if (!task.IsFinal) return false;
            return true;
        }

        void ApplyTaskStat(TaskInstance task, float amount)
        {
            if (task.Data.affectedStat == TaskStat.Happiness) stats.Apply(amount, 0);
            else stats.Apply(0, amount);
        }

        void FailAvailableWaveTasks()
        {
            foreach (var task in tasks)
            {
                if (task.IsFinal || task == Active) continue;
                task.Status = TaskStatus.Expired;
                ExpiredThisDay++;
                expiredTasksThisDay.Add(task.Data.taskName);
                ApplyTaskStat(task, -taskStatChange);
            }
        }

        public void EndWaveDay()
        {
            if (!waveMode || dayEnding) return;
            dayEnding = true;
            waveTransitionPending = false;
            waveTimeRemaining = 0;
            FailAvailableWaveTasks();
            Message = "Jornada terminada.";
        }

        public void StartDay(CharacterRole role)
        {
            waveMode = false;
            previousWaveTasks.Clear();
            waveTransitionPending = dayEnding = waveExpired = false;
            activeRole = role;
            Assigning = null;
            Pending = null;
            Active = null;
            tasks.Clear();
            FailedThisDay = 0;
            ExpiredThisDay = 0;
            InactiveCompletedThisDay = 0;
            CompletedThisDay = RejectedThisDay = 0;
            expiredTasksThisDay.Clear();
            if (!taskBoards.TryGetValue(role, out var board))
            {
                board = new List<TaskInstance>();
                taskBoards.Add(role, board);
            }
            for (int i = board.Count - 1; i >= 0; i--)
            {
                var task = board[i];
                if (task.IsFinal)
                {
                    board.RemoveAt(i);
                    continue;
                }
                if (task.Age > 2)
                {
                    ExpireTask(task);
                    continue;
                }
                task.Day = days.CurrentDay;
                if (task.Status == TaskStatus.InProgress || task.Status == TaskStatus.Failed)
                    task.Status = TaskStatus.Available;
                tasks.Add(task);
            }
            backlogDialogueShown = false;
            Message = "Selecciona una tarea para comenzar.";

            var available = new List<TaskData>();
            var ids = new HashSet<string>();
            foreach (var data in catalog)
            {
                if (data == null)
                    continue;
                if (data.owner != role)
                    continue;
                string id = string.IsNullOrEmpty(data.taskId) ? data.name : data.taskId;
                bool alreadyOnBoard = false;
                foreach (var task in tasks)
                    if (task.Data == data) { alreadyOnBoard = true; break; }
                if (!alreadyOnBoard && ids.Add(id))
                    available.Add(data);
            }

            // Sacamos cada elegida de la bolsa para no repetirla ese día.
            int boardLimit = Mathf.Min(tasksPerDay, maxBacklogPerRole);
            while (tasks.Count < boardLimit && available.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, available.Count);
                var task = new TaskInstance(++nextId, days.CurrentDay, available[index]);
                tasks.Add(task);
                board.Add(task);
                available.RemoveAt(index);
            }
        }

        public bool StartOwnTask(int index)
        {
            if (Active != null || dayEnding || waveTransitionPending || index < 0 || index >= tasks.Count) return false;
            var task = tasks[index];
            if (task.Owner != activeRole || task.Status != TaskStatus.Available) return false;
            Active = task;
            task.Status = TaskStatus.InProgress;
            task.Remaining = task.Data.executionTimeLimit;
            Message = "Resolviendo tarea.";
            OnInteractionStarted?.Invoke(task, InteractionKind.Execution);
            return true;
        }

        public bool RejectOwnTask(int index)
        {
            if (Active != null || dayEnding || waveTransitionPending || index < 0 || index >= tasks.Count) return false;
            var task = tasks[index];
            if (task.Owner != activeRole || task.Status != TaskStatus.Available) return false;
            if (waveMode)
            {
                task.Status = TaskStatus.Rejected;
                RejectedThisDay++;
                ApplyTaskStat(task, -taskStatChange);
                Message = "Tarea rechazada.";
                if (AllWaveTasksFinal()) RequestWaveTransition();
                return true;
            }
            task.Status = task.Data.category == TaskCategory.Required ? TaskStatus.Available : TaskStatus.Rejected;
            if (task.Data.rejectionHappinessChange != 0)
                stats.Apply(task.Data.rejectionHappinessChange, 0);
            Message = task.Data.category == TaskCategory.Required
                ? "La obligación queda pendiente."
                : "Tarea rechazada.";
            return true;
        }

        public bool Assign(int index, TaskStrategy strategy)
        {
            if (waveMode) return false;
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
            CheckBacklogUrgency();
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
            CheckBacklogUrgency();
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
            CheckBacklogUrgency();
        }

        // Entrada para el simulador y los futuros minijuegos.
        public bool Resolve(int id, int day, InteractionKind kind, InteractionResult result)
        {
            if (waveMode)
            {
                var waveTask = Active;
                if (kind != InteractionKind.Execution || waveTask == null || waveTask.Id != id ||
                    waveTask.Day != day || waveTask.IsFinal) return false;

                Active = null;
                bool success = result == InteractionResult.Success;
                waveTask.Status = success ? TaskStatus.Completed : TaskStatus.Failed;
                if (success)
                {
                    CompletedThisDay++;
                    ApplyTaskStat(waveTask, taskStatChange);
                    Message = "Tarea completada.";
                }
                else
                {
                    FailedThisDay++;
                    ApplyTaskStat(waveTask, -taskStatChange);
                    Message = "Tarea incumplida.";
                }

                OnInteractionResolved?.Invoke(waveTask, kind, result);
                if (!dayEnding && (waveExpired || AllWaveTasksFinal())) RequestWaveTransition();
                return true;
            }

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
            OnInteractionResolved?.Invoke(task, kind, result);
            CheckBacklogUrgency();

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
                FailedThisDay++;
                task.Status = task.Data.keepAfterFailure ? TaskStatus.Available : TaskStatus.Failed;
                stats.Apply(-failureStress * happinessCostMultiplier, 0);
                Message = task.Data.keepAfterFailure ? "Falló, pero puede intentarse de nuevo." : "Tarea fallida. Sin producción.";
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
            stats.Apply(-task.Data.happinessCost * stress * happinessCostMultiplier, 0);
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
            if (waveMode)
            {
                if (dayEnding || waveTransitionPending || waveExpired) return;
                waveTimeRemaining = Mathf.Max(0, waveTimeRemaining - delta);
                if (waveTimeRemaining <= 0)
                {
                    waveExpired = true;
                    FailAvailableWaveTasks();
                    // An opened minigame keeps its own clock; unavailable tasks expire once.
                    if (Active == null)
                        RequestWaveTransition();
                }
                return;
            }
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
            if (waveMode)
            {
                EndWaveDay();
                return;
            }
            var assigning = Assigning;
            var active = Active;
            var refusing = Pending != null && Pending.Refusing ? Pending : null;
            foreach (var task in tasks)
            {
                if (task.IsFinal)
                {
                    taskBoards[activeRole].Remove(task);
                    continue;
                }
                task.Age++;
                if (task.Age > 2) ExpireTask(task);
                else task.Status = TaskStatus.Available;
            }
            if (assigning != null) OnInteractionResolved?.Invoke(assigning, InteractionKind.Assignment, InteractionResult.Cancelled);
            if (active != null) OnInteractionResolved?.Invoke(active, InteractionKind.Execution, InteractionResult.Cancelled);
            if (refusing != null) OnInteractionResolved?.Invoke(refusing, InteractionKind.Refusal, InteractionResult.Cancelled);
            Assigning = null;
            Pending = null;
            Active = null;
            CheckBacklogUrgency();
        }

        void ExpireTask(TaskInstance task)
        {
            if (task == null || task.IsFinal) return;
            task.Status = TaskStatus.Expired;
            if (taskBoards.TryGetValue(task.Owner, out var board)) board.Remove(task);
            ExpiredThisDay++;
            string ownerName = task.Owner == CharacterRole.Boss ? "Josefa" : "Tomás";
            expiredTasksThisDay.Add(task.Data.taskName + " (" + ownerName + ")");
            if (task.Data.category == TaskCategory.Required && missedTaskHappinessPenalty > 0)
                stats.Apply(-missedTaskHappinessPenalty, 0);
            Message = "Tarea incumplida: " + task.Data.taskName + ".";
        }

        public void ClearTaskBoards()
        {
            waveMode = false;
            previousWaveTasks.Clear();
            waveExpired = waveTransitionPending = dayEnding = false;
            waveNumber = 0;
            waveTimeRemaining = 0;
            tasks.Clear();
            taskBoards.Clear();
            expiredTasksThisDay.Clear();
            ExpiredThisDay = 0;
            FailedThisDay = 0;
            InactiveCompletedThisDay = 0;
            CompletedThisDay = RejectedThisDay = 0;
            Assigning = null;
            Pending = null;
            Active = null;
        }

        public string ResolveInactiveRole(CharacterRole role)
        {
            if (waveMode)
                return role == CharacterRole.Boss
                    ? "Josefa no recibió tareas durante esta jornada."
                    : "Tomás no recibió tareas durante esta jornada.";

            if (!taskBoards.TryGetValue(role, out var board))
            {
                board = new List<TaskInstance>();
                taskBoards.Add(role, board);
            }

            TaskInstance task = null;
            foreach (var item in board)
                if (!item.IsFinal) { task = item; break; }
            if (task == null)
            {
                var candidates = new List<TaskData>();
                foreach (var data in catalog)
                    if (data != null && data.owner == role) candidates.Add(data);
                if (candidates.Count == 0) return role == CharacterRole.Boss ? "Josefa no tenía tareas disponibles." : "Tomás no tenía tareas disponibles.";
                task = new TaskInstance(++nextId, days.CurrentDay, candidates[UnityEngine.Random.Range(0, candidates.Count)]);
                board.Add(task);
            }

            task.ResolvedAutomatically = true;
            bool success = stats.Productivity >= 35 && stats.Happiness >= 30 && UnityEngine.Random.value > .25f;
            string actor = role == CharacterRole.Boss ? "Josefa" : "Tomás";
            if (success)
            {
                task.Status = TaskStatus.Completed;
                InactiveCompletedThisDay++;
                stats.Apply(-task.Data.happinessCost * .5f, 0);
                days.AddProduction(task.Data.quotaReward * .5f * quotaMultiplier);
                board.Remove(task);
                return actor + " avanzó: " + task.Data.taskName + ".";
            }

            task.Age++;
            if (task.Age > 2)
            {
                ExpireTask(task);
                return actor + " dejó incumplida: " + task.Data.taskName + ".";
            }
            task.Status = TaskStatus.Available;
            return actor + " dejó pendiente: " + task.Data.taskName + (task.Age == 2 ? " (crítica)." : " (urgente).");
        }

        static bool IsUnresolved(TaskInstance task) => task.Status == TaskStatus.Assigning ||
            task.Status == TaskStatus.PendingResponse || task.Status == TaskStatus.Accepted || task.Status == TaskStatus.InProgress;

        void CheckBacklogUrgency()
        {
            if (UnresolvedCount >= backlogUrgencyThreshold)
            {
                if (!backlogDialogueShown)
                {
                    backlogDialogueShown = true;
                    OnBacklogUrgencyReached?.Invoke();
                }
            }
            else backlogDialogueShown = false;
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
