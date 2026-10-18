using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class ResultsUI : MonoBehaviour
    {
        [SerializeField] Text label, controls;

        public void Refresh(GameManager game)
        {
            label.text = game.ResultText;
            controls.text = $"{game.Input.worker.confirm} / {game.Input.boss.confirm}: continuar";
        }
    }
}
