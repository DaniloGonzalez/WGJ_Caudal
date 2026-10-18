using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class TrabajadorHUD : MonoBehaviour
    {
        [SerializeField] Text title, label, controls;
        [SerializeField] RawImage portrait;
        [SerializeField] TaskCard assignedTaskCard;
        string dialogue;
        float dialogueUntil;

        void Awake()
        {
            if (assignedTaskCard == null)
                assignedTaskCard = GetComponentInChildren<TaskCard>(true);
        }

        public void ShowDialogue(string text, float duration)
        {
            dialogue = text;
            dialogueUntil = Time.unscaledTime + duration;
        }

        public void Refresh(GameManager game)
        {
            var character = game.WorkerCharacter;
            title.text = character != null ? "TRABAJADOR · " + character.characterName : "TRABAJADOR";
            GameScreen.SetPortrait(portrait, character != null ? character.portrait : null);
            var keys = game.Input.worker;
            var active = game.Tasks.Active;
            var pending = game.Tasks.Pending;
            var assigned = active ?? pending;
            if (assignedTaskCard == null)
                assignedTaskCard = GetComponentInChildren<TaskCard>(true);
            if (assignedTaskCard != null)
            {
                assignedTaskCard.gameObject.SetActive(assigned != null);
                if (assigned != null)
                    assignedTaskCard.Refresh(assigned, false, false);
            }
            controls.text = $"{keys.confirm}: aceptar · {keys.cancel}: rechazar";
            if (active != null)
            {
                label.text = $"Ejecutando: {active.Data.taskName} ({active.Remaining:0}s)" +
                    (pending != null ? $"\nEn espera: {pending.Data.taskName}" : "");
                controls.text = Simulation(game, keys);
            }
            else if (pending != null && pending.Refusing)
            {
                label.text = $"Conversación de rechazo ({pending.Remaining:0}s)";
                controls.text = Simulation(game, keys);
            }
            else if (pending != null)
                label.text = $"{pending.Data.taskName} · {(pending.Strategy == TaskStrategy.Pressure ? "PRESIÓN" : "NORMAL")}\n{pending.Data.description}";
            else
            {
                label.text = "Sin solicitudes pendientes";
                controls.text = string.Empty;
            }
            if (Time.unscaledTime < dialogueUntil) controls.text = dialogue;
        }

        static string Simulation(GameManager game, RoleKeys keys) => game.Simulator
            ? $"SIMULADOR: {keys.confirm} éxito · {keys.cancel} fallo"
            : "Esperando integración de minijuego";
    }
}
