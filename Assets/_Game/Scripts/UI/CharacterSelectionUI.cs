using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class CharacterSelectionUI : MonoBehaviour
    {
        [SerializeField] CharacterCard workerCard, bossCard;
        [SerializeField] Text instructionsText;
        [SerializeField] Text dayText;
        GameManager game;

        public void Refresh(GameManager game)
        {
            this.game = game;
            workerCard.SetPointerOwner(this, false);
            bossCard.SetPointerOwner(this, true);
            workerCard.Refresh(game.WorkerCharacter, game.Input.boss, game.ActiveRole == CharacterRole.Worker, false, false);
            bossCard.Refresh(game.BossCharacter, game.Input.boss, game.ActiveRole == CharacterRole.Boss, true, false);
            if (instructionsText != null)
                instructionsText.gameObject.SetActive(false);
            if (dayText != null)
                dayText.text = $"DÍA:   {game.Days.CurrentDay + 1:00}";
        }

        public void SelectFromPointer(bool boss, bool confirm)
        {
            if (game == null) return;
            game.SetSelectionRole(boss ? CharacterRole.Boss : CharacterRole.Worker);
            if (confirm) game.ConfirmCharacter(boss);
        }
    }
}
