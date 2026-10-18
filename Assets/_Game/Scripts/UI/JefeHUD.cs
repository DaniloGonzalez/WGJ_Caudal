using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Caudal
{
    public sealed class JefeHUD : MonoBehaviour
    {
        [SerializeField] Text title, strategyLabel, controls;
        [SerializeField] RawImage portrait;
        [SerializeField] RectTransform taskList;
        [SerializeField] TaskCard taskCardPrefab;
        [SerializeField] List<TaskCard> cards = new List<TaskCard>();

        public void Refresh(GameManager game)
        {
            var character = game.BossCharacter;
            title.text = character != null ? "JEFE · " + character.characterName : "JEFE";
            GameScreen.SetPortrait(portrait, character != null ? character.portrait : null);
            var keys = game.Input.boss;
            var assigning = game.Tasks.Assigning;
            strategyLabel.text = assigning != null
                ? $"Asignando: {assigning.Data.taskName} ({assigning.Remaining:0}s)"
                : $"{game.Strategy} · {keys.alternate}: cambiar estrategia";
            if (assigning != null)
                controls.text = game.Simulator
                    ? $"SIMULADOR: {keys.confirm} éxito · {keys.cancel} fallo"
                    : "Esperando integración de minijuego";
            else
                controls.text = game.Tasks.Pending != null
                    ? "Solicitud pendiente: espera respuesta"
                    : $"{keys.up}/{keys.down}: seleccionar · {keys.confirm}: asignar";

            // Tres filas reutilizadas, igual que la ventana del catálogo original.
            int start = Mathf.Max(0, game.Selected - 1);
            int count = Mathf.Min(3, game.Tasks.Tasks.Count - start);
            while (cards.Count < count)
                cards.Add(Instantiate(taskCardPrefab, taskList, false));
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].gameObject.SetActive(i < count);
                if (i < count) cards[i].Refresh(game.Tasks.Tasks[start + i], start + i == game.Selected);
            }
        }
    }
}
