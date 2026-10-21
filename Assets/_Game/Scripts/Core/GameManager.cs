using UnityEngine;
namespace Caudal
{
    public enum GameState
    {
        DayStart, StartScreen, IntroScreen, Playing, DaySummary, GameOver, CharacterSelection, Quitting
    }
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] bool developmentSimulator = true;
        const float endingThreshold = 50f;
        [SerializeField] CharacterData[] workerCharacters;
        [SerializeField] CharacterData[] bossCharacters;
        int workerIndex, bossIndex;
        int roleSelection;
        int inputTransitionFrame = -1;
        public CharacterRole ActiveRole => roleSelection == 0 ? CharacterRole.Boss : CharacterRole.Worker;
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
        public bool NextDayBossSelected => roleSelection == 0;
        public TaskStrategy Strategy { get; private set; }
        public bool Simulator => developmentSimulator;
        public string ResultText { get; private set; } = "";
        public int CharacterCount(bool boss) => (boss ? bossCharacters : workerCharacters)?.Length ?? 0;
        public event System.Action<CharacterRole, string, float> OnDialogue;
        public void Initialize(StatsManager stats, DayManager days, TaskManager tasks, InputRouter input)
        {
            Stats = stats;
            Days = days;
            Tasks = tasks;
            Input = input;
            tasks.Initialize(stats, days);
            tasks.OnBacklogUrgencyReached += () => ShowDialogue(DialogueTrigger.BacklogUrgency, CharacterRole.Worker);
            tasks.OnInteractionResolved += HandleInteractionResolved;
        }

        public void Restart()
        {
            Tasks.EndDay();
            Tasks.ClearTaskBoards();
            Tasks.SetCharacters(null, null);
            Stats.ResetStats();
            Days.ResetDays();
            workerIndex = bossIndex = 0;
            roleSelection = 1;
            WorkerReady = BossReady = false;
            State = GameState.CharacterSelection;
        }

        public void ShowStartScreen()
        {
            Tasks.EndDay();
            Tasks.ClearTaskBoards();
            Tasks.SetCharacters(null, null);
            Stats.ResetStats();
            Days.ResetDays();
            workerIndex = bossIndex = 0;
            roleSelection = 1;
            WorkerReady = BossReady = false;
            State = GameState.StartScreen;
        }

        public void StartFromTitle()
        {
            if (State == GameState.StartScreen)
            {
                State = GameState.IntroScreen;
                inputTransitionFrame = Time.frameCount;
            }
        }

        public void ContinueFromIntro()
        {
            if (State == GameState.IntroScreen)
            {
                State = GameState.CharacterSelection;
                inputTransitionFrame = Time.frameCount;
            }
        }

        public void QuitGame()
        {
            State = GameState.Quitting;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
            if (State != GameState.CharacterSelection || BossCharacter == null || WorkerCharacter == null) return;
            roleSelection = boss ? 0 : 1;
            WorkerReady = BossReady = true;
            Tasks.SetCharacters(WorkerCharacter, BossCharacter);
            BeginDay();
        }

        public void CancelCharacter(bool boss)
        {
            if (State != GameState.CharacterSelection) return;
            roleSelection = boss ? 0 : 1;
        }

        public void SetSelectionRole(CharacterRole role)
        {
            if (State != GameState.CharacterSelection) return;
            roleSelection = role == CharacterRole.Boss ? 0 : 1;
        }

        public void SelectTask(int index)
        {
            if (State != GameState.Playing || Tasks == null || index < 0 || index >= Tasks.Tasks.Count) return;
            Selected = index;
        }

        public void SelectNextDayCharacter(bool boss)
        {
            if (State != GameState.DaySummary || Days.CurrentDay >= Days.TotalDays) return;
            roleSelection = boss ? 0 : 1;
        }

        public void ContinueFromDaySummary()
        {
            if (State != GameState.DaySummary) return;
            if (Days.CurrentDay >= Days.TotalDays)
                ShowEnding();
            else
                BeginDay();
        }

        void ReadCharacterInput(bool boss)
        {
            var keys = Input.boss;
            if (Input.Pressed(keys.left) || Input.Pressed(keys.right)) roleSelection = 1 - roleSelection;
            if (Input.Pressed(keys.confirm)) ConfirmCharacter(roleSelection == 0);
        }

        void BeginDay()
        {
            Days.StartDay();
            Tasks.StartWaveDay(ActiveRole);
            Selected = 0;
            Strategy = TaskStrategy.Normal;
            State = GameState.Playing;
        }

        void Update()
        {
            // La intro todavía no ha arrancado la partida.
            if (Input == null || State == GameState.DayStart)
                return;

            if (State == GameState.Quitting)
                return;

            if (Time.frameCount == inputTransitionFrame)
                return;

            if (State == GameState.StartScreen || State == GameState.IntroScreen)
                return;

            if (State == GameState.CharacterSelection)
            {
                ReadCharacterInput(true);
                return;
            }

            if (State != GameState.Playing)
            {
                ReadContinueInput();
                return;
            }

            if (Tasks.DayEnding)
            {
                if (!Tasks.HasActiveMinigame) CloseDay();
                return;
            }

            // Las animaciones de cambio de oleada no consumen tiempo de jornada.
            if (Tasks.WaveTransitionPending) return;

            if (Days.Tick(Time.deltaTime))
            {
                Tasks.EndWaveDay();
                if (!Tasks.HasActiveMinigame) CloseDay();
                return;
            }

            Tasks.Tick(Time.deltaTime);
            // Durante la jornada, selección y decisiones se hacen desde la UI con el mouse.
        }

        void ReadContinueInput()
        {
            if (State == GameState.Quitting)
                return;

            if (State == GameState.DaySummary)
            {
                if (Input.Pressed(Input.boss.left) || Input.Pressed(Input.boss.right))
                    roleSelection = 1 - roleSelection;
                if (Input.Pressed(Input.boss.confirm)) ContinueFromDaySummary();
                return;
            }

            if (!Input.Pressed(Input.boss.confirm)) return;

            if (State == GameState.GameOver)
                Restart();
        }

        void CloseDay()
        {
            Tasks.EndDay();
            Days.RecordClosingStats(Stats.Happiness, Stats.Productivity);
            ResultText = $"DÍA {Days.CurrentDay} TERMINADO\n\nOleadas: {Tasks.WaveNumber}\nCompletadas: {Tasks.CompletedThisDay}  ·  Rechazadas: {Tasks.RejectedThisDay}  ·  Fallidas: {Tasks.FailedThisDay}  ·  Vencidas: {Tasks.ExpiredThisDay}\nBienestar: {Stats.Happiness:0}  ·  Productividad: {Stats.Productivity:0}\n\nEnter para continuar";
            State = GameState.DaySummary;
        }

        void HandleInteractionResolved(TaskInstance task, InteractionKind kind, InteractionResult result)
        {
            if (task == null || result == InteractionResult.Cancelled) return;
            bool success = result == InteractionResult.Success;
            DialogueTrigger trigger = kind switch
            {
                InteractionKind.Assignment or InteractionKind.Execution => success ? DialogueTrigger.TaskSuccess : DialogueTrigger.TaskFailure,
                _ => success ? DialogueTrigger.RefusalSuccess : DialogueTrigger.RefusalFailure
            };
            ShowDialogue(trigger, task.Owner);
        }

        void ShowDialogue(DialogueTrigger trigger, CharacterRole defaultRole)
        {
            CharacterData speaker = defaultRole == CharacterRole.Boss ? BossCharacter : WorkerCharacter;
            if (speaker == null || speaker.dialogueLines == null)
                return;

            var matchingLines = System.Array.FindAll(speaker.dialogueLines,
                line => line != null && line.trigger == trigger && !string.IsNullOrWhiteSpace(line.text));
            if (matchingLines.Length == 0) return;

            var line = matchingLines[UnityEngine.Random.Range(0, matchingLines.Length)];
            OnDialogue?.Invoke(defaultRole, line.text, Mathf.Max(1, line.duration));
            if (Application.isPlaying && speaker.speechClip != null)
                AudioSource.PlayClipAtPoint(speaker.speechClip, Vector3.zero);
        }
        void ShowEnding()
        {
            // El final refleja el estado al cerrar el viernes, no el promedio semanal.
            float happiness = Stats.Happiness;
            float productivity = Stats.Productivity;
            string title, body;
            if (happiness >= endingThreshold && productivity >= endingThreshold)
            {
                title = "Best Place to Work";
                body = "Encontraron espacio para producir y cuidar a las personas.";
            }
            else if (happiness < endingThreshold && productivity >= endingThreshold)
            {
                title = "Éxito miserable";
                body = "Las metas se cumplieron, pero el bienestar quedó atrás.";
            }
            else if (happiness >= endingThreshold && productivity < endingThreshold)
            {
                title = "Oficina feliz en crisis";
                body = "El bienestar no alcanzó para sostener la producción.";
            }
            else
            {
                title = "Colapso de oficina";
                body = "La oficina terminó sin energía y sin resultados.";
            }
            ResultText = $"{title}\n\n{body}\n\nValores finales del viernes:\nBienestar: {happiness:0}\nProductividad: {productivity:0}\n\nClic para reiniciar";
            State = GameState.GameOver;
        }
    }
}
