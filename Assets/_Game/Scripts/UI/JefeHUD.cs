using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class JefeHUD : MonoBehaviour
    {
        GameManager game;
        Text label;

        public void Initialize(GameManager game, Text label)
        {
            this.game = game;
            this.label = label;
        }

        void LateUpdate()
        {
            var keys = game.Input.boss;
            var assigning = game.Tasks.Assigning;
            if (assigning != null)
            {
                label.text = $"Asignando: {assigning.Data.taskName} ({assigning.Remaining:0}s)\n";
                if (game.Simulator)
                    label.text += $"SIMULADOR: {keys.confirm} éxito · {keys.cancel} fallo";
                else
                    label.text += "Esperando integración de minijuego";
                return;
            }

            string text = $"{game.Strategy} · {keys.alternate}: cambiar estrategia\n";
            int start = Mathf.Max(0, game.Selected - 1);
            int end = Mathf.Min(game.Tasks.Tasks.Count, start + 3);
            for (int i = start; i < end; i++)
            {
                var task = game.Tasks.Tasks[i];
                string cursor = i == game.Selected ? ">" : " ";
                text += $"{cursor} {task.Data.taskName} [{task.Status}]\n";
            }

            if (game.Tasks.Pending != null)
                text += "Solicitud pendiente: espera respuesta";
            else
                text += $"{keys.up}/{keys.down}: seleccionar · {keys.confirm}: asignar";
            label.text = text;
        }
    }
}
