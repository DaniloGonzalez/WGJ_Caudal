using UnityEngine;
using UnityEngine.UI;
namespace Caudal
{
    public sealed class SharedHUD : MonoBehaviour
    {
        GameManager game;
        Text label;
        public void Initialize(GameManager g, Text t)
        {
            game = g;
            label = t;
        }
        void LateUpdate()
        {
            int time = Mathf.CeilToInt(game.Days.TimeRemaining);
            label.text = $"DÍA {game.Days.CurrentDay}/{game.Days.TotalDays}     TIEMPO {time / 60:00}:{time % 60:00}     CUOTA {game.Days.QuotaProgress:0}/{game.Days.QuotaTarget:0}" + (game.Simulator ? "\nPROTOTIPO · RESULTADOS SIMULADOS" : "");
        }
    }
}
