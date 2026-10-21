using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    // Los prefabs definen el aspecto; este componente presenta el estado de la partida.
    public sealed class GameScreen : MonoBehaviour
    {
        [SerializeField] TrabajadorHUD worker;
        [SerializeField] JefeHUD boss;
        [SerializeField] SharedHUD shared;
        [SerializeField] StatsGraphUI graph;
        [SerializeField] CharacterSelectionUI selection;
        [SerializeField] ResultsUI results;
        [SerializeField] StartScreenUI startScreen;
        [SerializeField] GameObject taskWindows;
        [SerializeField] TaskDecisionPanel taskDecisionPanel;
        GameManager game;

        public void Initialize(GameManager gameManager)
        {
            if (game != null) game.OnDialogue -= HandleDialogue;
            game = gameManager;
            game.OnDialogue += HandleDialogue;
            startScreen.Initialize(gameManager);
            results.Initialize(gameManager);
            Refresh();
        }

        void OnDestroy()
        {
            if (game != null) game.OnDialogue -= HandleDialogue;
        }

        void HandleDialogue(CharacterRole role, string text, float duration)
        {
            if (role == CharacterRole.Boss) boss.ShowDialogue(text, duration);
            else worker.ShowDialogue(text, duration);
        }

        void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (game == null || game.Input == null) return;
            bool choosing = game.State == GameState.CharacterSelection;
            bool showingStartScreen = game.State == GameState.StartScreen;
            bool showingResults = game.State == GameState.DaySummary || game.State == GameState.GameOver;
            bool showGame = game.State == GameState.Playing || showingResults;
            if (taskWindows != null) taskWindows.SetActive(game.State == GameState.Playing);
            if (game.State != GameState.Playing && taskDecisionPanel != null)
                taskDecisionPanel.Hide();
            worker.gameObject.SetActive(showGame);
            boss.gameObject.SetActive(showGame);
            shared.gameObject.SetActive(game.State == GameState.Playing);
            graph.gameObject.SetActive(game.State == GameState.Playing);
            selection.gameObject.SetActive(choosing);
            startScreen.gameObject.SetActive(showingStartScreen);
            results.gameObject.SetActive(showingResults);
            if (showingStartScreen) startScreen.Refresh();
            if (choosing) selection.Refresh(game);
            if (showGame)
            {
                worker.Refresh(game);
                boss.Refresh(game);
                shared.Refresh(game);
                graph.Refresh(game);
            }
            if (showingResults) results.Refresh(game);
        }

        public static void SetPortrait(RawImage image, Texture texture)
        {
            image.enabled = texture != null;
            if (image.texture == texture) return;
            image.texture = texture;
            if (texture != null)
                image.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
        }
    }
}
