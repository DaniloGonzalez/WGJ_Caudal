using UnityEngine;
using UnityEngine.UI;
namespace Caudal
{
    public sealed class ResultsUI : MonoBehaviour
    {
        GameManager game;
        GameObject panel;
        Text label;
        public void Initialize(GameManager g, GameObject p, Text t)
        {
            game = g;
            panel = p;
            label = t;
        }
        void LateUpdate()
        {
            panel.SetActive(game.State == GameState.DaySummary || game.State == GameState.GameOver);
            label.text = game.ResultText;
        }
    }
}
