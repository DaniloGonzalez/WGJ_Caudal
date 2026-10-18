using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class CharacterSelectionUI : MonoBehaviour
    {
        [SerializeField] CharacterCard workerCard, bossCard;
        [SerializeField] Text instructionsText;

        public void Refresh(GameManager game)
        {
            workerCard.Refresh(game.WorkerCharacter, game.Input.worker, game.WorkerReady, false, game.CharacterCount(false) > 1);
            bossCard.Refresh(game.BossCharacter, game.Input.boss, game.BossReady, true, game.CharacterCount(true) > 1);
            if (instructionsText != null)
                instructionsText.text = "ELIGE CON LAS TECLAS DE CADA TARJETA · AMBOS CONFIRMAN PARA INICIAR";
        }
    }
}
