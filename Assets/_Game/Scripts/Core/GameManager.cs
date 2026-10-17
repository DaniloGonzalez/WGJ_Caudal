using UnityEngine;
namespace Caudal
{
    public enum GameState
    {
        DayStart, Playing, DaySummary, GameOver, CharacterSelection
    }
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] bool developmentSimulator = true;
        [SerializeField] float missedQuotaProductivityPenalty = 5, endingLow = 40, endingHigh = 60;
        [SerializeField] CharacterData[] workerCharacters;
        [SerializeField] CharacterData[] bossCharacters;
        int workerIndex, bossIndex;
        public bool WorkerReady { get; private set; }
        public bool BossReady { get; private set; }
        public CharacterData WorkerCharacter => GetCharacter(false);
        public CharacterData BossCharacter => GetCharacter(true);

        public GameState State { get; private set; } = GameState.DayStart;
        public StatsManager Stats { get; private set; }
        public DayManager Days { get; private set; }
        public TaskManager Tasks { get; private set; }
        public InputRouter Input { get; private set; }
        public int Selected { get; private set; }
        public TaskStrategy Strategy { get; private set; }
        public bool Simulator => developmentSimulator;
        public string ResultText { get; private set; } = "";
        public void Initialize(StatsManager stats, DayManager days, TaskManager tasks, InputRouter input)
        {
            Stats = stats;
            Days = days;
            Tasks = tasks;
            Input = input;
            tasks.Initialize(stats, days);
        }

        public void Restart()
        {
            Tasks.EndDay();
            Tasks.SetCharacters(null, null);
            Stats.ResetStats();
            Days.ResetDays();
            workerIndex = bossIndex = 0;
            WorkerReady = BossReady = false;
            State = GameState.CharacterSelection;
        }

        CharacterData GetCharacter(bool boss)
        {
            var characters = boss ? bossCharacters : workerCharacters;
            int index = boss ? bossIndex : workerIndex;
            if (characters == null || index >= characters.Length) return null;
            var character = characters[index];
            var role = boss ? CharacterRole.Boss : CharacterRole.Worker;
            return character != null && character.role == role ? character : null;
        }

        public void SelectCharacter(bool boss, int direction)
        {
            if (State != GameState.CharacterSelection) return;
            if (boss ? BossReady : WorkerReady) return;
            var characters = boss ? bossCharacters : workerCharacters;
            if (characters == null || characters.Length == 0) return;
            int index = boss ? bossIndex : workerIndex;
            index = (index + direction % characters.Length + characters.Length) % characters.Length;
            if (boss) bossIndex = index;
            else workerIndex = index;
        }

        public void ConfirmCharacter(bool boss)
        {
            if (State != GameState.CharacterSelection || GetCharacter(boss) == null) return;
            if (boss) BossReady = true;
            else WorkerReady = true;
            if (!WorkerReady || !BossReady) return;

            Tasks.SetCharacters(WorkerCharacter, BossCharacter);
            BeginDay();
        }

        public void CancelCharacter(bool boss)
        {
            if (State != GameState.CharacterSelection) return;
            if (boss) BossReady = false;
            else WorkerReady = false;
        }

        void ReadCharacterInput(bool boss)
        {
            var keys = boss ? Input.boss : Input.worker;
            if (Input.Pressed(keys.cancel))
            {
                CancelCharacter(boss);
                return;
            }
            if (Input.Pressed(keys.up)) SelectCharacter(boss, -1);
            if (Input.Pressed(keys.down)) SelectCharacter(boss, 1);
            if (Input.Pressed(keys.confirm)) ConfirmCharacter(boss);
        }

        void BeginDay()
        {
            Days.StartDay();
            Tasks.StartDay();
            Selected = 0;
            Strategy = TaskStrategy.Normal;
            State = GameState.Playing;
        }

        void Update()
        {
            // La intro todavía no ha arrancado la partida.
            if (Input == null || State == GameState.DayStart)
                return;

            if (State == GameState.CharacterSelection)
            {
                ReadCharacterInput(false);
                ReadCharacterInput(true);
                return;
            }

            if (State != GameState.Playing)
            {
                ReadContinueInput();
                return;
            }

            if (Days.Tick(Time.deltaTime))
            {
                CloseDay();
                return;
            }

            Tasks.Tick(Time.deltaTime);
            ReadBossInput();
            ReadWorkerInput();
        }

        void ReadContinueInput()
        {
            if (!Input.Pressed(Input.boss.confirm) && !Input.Pressed(Input.worker.confirm))
                return;

            if (State == GameState.GameOver)
                Restart();
            else if (Days.CurrentDay >= Days.TotalDays)
                ShowEnding();
            else
                BeginDay();
        }

        void ReadBossInput()
        {
            var keys = Input.boss;
            if (Tasks.Assigning != null)
            {
                Simulate(Tasks.Assigning, InteractionKind.Assignment, keys);
                return;
            }

            if (Input.Pressed(keys.up))
                Selected--;
            if (Input.Pressed(keys.down))
                Selected++;
            Selected = Mathf.Clamp(Selected, 0, Mathf.Max(0, Tasks.Tasks.Count - 1));

            if (Input.Pressed(keys.alternate))
            {
                if (Strategy == TaskStrategy.Normal)
                    Strategy = TaskStrategy.Pressure;
                else
                    Strategy = TaskStrategy.Normal;
            }

            if (Input.Pressed(keys.confirm))
                Tasks.Assign(Selected, Strategy);
        }

        void ReadWorkerInput()
        {
            var keys = Input.worker;
            // Cada pulsación hace una sola cosa: resolver, aceptar o rechazar.
            if (Tasks.Active != null)
            {
                Simulate(Tasks.Active, InteractionKind.Execution, keys);
                return;
            }

            if (Tasks.Pending != null && Tasks.Pending.Refusing)
            {
                Simulate(Tasks.Pending, InteractionKind.Refusal, keys);
                return;
            }

            if (Input.Pressed(keys.confirm))
                Tasks.Accept();
            else if (Input.Pressed(keys.cancel))
                Tasks.Reject();
        }

        void Simulate(TaskInstance task, InteractionKind kind, RoleKeys keys)
        {
            if (!developmentSimulator)
                return;

            if (Input.Pressed(keys.confirm))
                Tasks.Resolve(task.Id, task.Day, kind, InteractionResult.Success);
            else if (Input.Pressed(keys.cancel))
                Tasks.Resolve(task.Id, task.Day, kind, InteractionResult.Failure);
        }

        void CloseDay()
        {
            Tasks.EndDay();
            bool met = Days.CloseDay();
            if (!met)
                Stats.Apply(0, -missedQuotaProductivityPenalty);
            ResultText = $"DÍA {Days.CurrentDay} TERMINADO\n\nCuota: {Days.QuotaProgress:0}/{Days.QuotaTarget:0} — {(met ? "Cumplida" : "Incumplida")}\nCompletadas: {Tasks.Count(TaskStatus.Completed)}  ·  Rechazadas: {Tasks.Count(TaskStatus.Rejected)}\nFallidas: {Tasks.Count(TaskStatus.Failed)}  ·  Expiradas: {Tasks.Count(TaskStatus.Expired)}\nFelicidad: {Stats.Happiness:0}  ·  Productividad: {Stats.Productivity:0}\n\nConfirmar para continuar";
            State = GameState.DaySummary;
        }
        void ShowEnding()
        {
            float happiness = Stats.Happiness;
            float productivity = Stats.Productivity;
            string title, body;
            if (happiness >= endingHigh && productivity >= endingHigh)
            {
                title = "Oficina ideal";
                body = "Encontraron espacio para producir y cuidar a las personas.";
            }
            else if (happiness <= endingLow && productivity >= endingHigh)
            {
                title = "Burnout del trabajador";
                body = "Las metas se cumplieron, pero el bienestar quedó atrás.";
            }
            else if (happiness >= endingHigh && productivity <= endingLow)
            {
                title = "Quiebra de la empresa";
                body = "El bienestar no alcanzó para sostener la producción.";
            }
            else if (happiness <= endingLow && productivity <= endingLow)
            {
                title = "Colapso de la oficina";
                body = "La oficina terminó sin energía y sin resultados.";
            }
            else
            {
                title = "Sobrevivieron otra semana";
                body = "Quedan decisiones por mejorar para encontrar el equilibrio.";
            }
            ResultText = $"{title}\n\n{body}\n\nFelicidad {happiness:0} · Productividad {productivity:0}\nCuotas incumplidas: {Days.MissedQuotas}\n\nConfirmar para reiniciar";
            State = GameState.GameOver;
        }
    }
}
