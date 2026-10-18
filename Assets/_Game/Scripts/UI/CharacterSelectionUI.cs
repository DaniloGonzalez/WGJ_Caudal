using UnityEngine;

namespace Caudal
{
    public sealed class CharacterSelectionUI : MonoBehaviour
    {
        [SerializeField] CharacterCard workerCard, bossCard;

        public void Refresh(GameManager game)
        {
            workerCard.Refresh(game.WorkerCharacter, game.Input.worker, game.WorkerReady, false);
            bossCard.Refresh(game.BossCharacter, game.Input.boss, game.BossReady, true);
        }
    }
}
