using UnityEngine;
using UnityEngine.UI;
namespace Caudal
{
    public sealed class TrabajadorHUD : MonoBehaviour
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
            var keys = game.Input.worker;
            var active = game.Tasks.Active;
            var pending = game.Tasks.Pending;
            if (active != null)
                label.text = $"Ejecutando: {active.Data.taskName} ({active.Remaining:0}s)\n" + Simulation(keys) + (pending != null ? $"\nEn espera: {pending.Data.taskName}" : "");
            else if (pending != null && pending.Refusing)
                label.text = $"Conversación de rechazo ({pending.Remaining:0}s)\n" + Simulation(keys);
            else if (pending != null)
                label.text = $"{pending.Data.taskName} · {(pending.Strategy == TaskStrategy.Pressure ? "PRESIÓN" : "NORMAL")}\n{pending.Data.description}\n{keys.confirm}: aceptar · {keys.cancel}: rechazar";
            else
                label.text = "Sin solicitudes pendientes\nPuedes decir que no. Cada decisión cuenta.";
        }
        string Simulation(RoleKeys keys) => game.Simulator ? $"SIMULADOR: {keys.confirm} éxito · {keys.cancel} fallo" : "Esperando integración de minijuego";
    }
}
