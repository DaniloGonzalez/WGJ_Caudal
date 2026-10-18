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
        GameManager game;

        public void Initialize(GameManager gameManager)
        {
            if (game != null) game.OnDialogue -= HandleDialogue;
            game = gameManager;
            game.OnDialogue += HandleDialogue;
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
            bool showingResults = game.State == GameState.DaySummary || game.State == GameState.GameOver;
            bool showGame = game.State == GameState.Playing || showingResults;
            worker.gameObject.SetActive(showGame);
            boss.gameObject.SetActive(showGame);
            shared.gameObject.SetActive(showGame);
            graph.gameObject.SetActive(showGame);
            selection.gameObject.SetActive(choosing);
            results.gameObject.SetActive(showingResults);
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
