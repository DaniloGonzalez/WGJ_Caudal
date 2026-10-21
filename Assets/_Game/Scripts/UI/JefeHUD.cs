using System.Collections;
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
        [SerializeField] TaskDecisionPanel taskDecisionPanel;
        [SerializeField] List<TaskCard> cards = new List<TaskCard>();
        Coroutine waveTransition;
        static readonly Vector2[] bossTaskPositions = { new Vector2(.64f, .78f), new Vector2(.58f, .68f), new Vector2(.67f, .58f) };
        static readonly Vector2[] workerTaskPositions = { new Vector2(.56f, .38f), new Vector2(.46f, .28f), new Vector2(.64f, .18f) };

        public void ShowDialogue(string text, float duration) { }

        public void Refresh(GameManager game)
        {
            if (game.State != GameState.Playing)
            {
                if (waveTransition != null)
                {
                    StopCoroutine(waveTransition);
                    waveTransition = null;
                    foreach (var card in cards)
                        if (card != null) card.ResetWaveTransition();
                }
                return;
            }

            if (waveTransition != null) return;
            if (game.Tasks.WaveTransitionPending)
            {
                waveTransition = StartCoroutine(AnimateWaveTransition(game));
                return;
            }

            RefreshCards(game);
            bool warning = game.Tasks.WaveTimeRemaining <= 3f;
            foreach (var card in cards)
                if (card != null) card.SetWaveWarning(warning);
        }

        void RefreshCards(GameManager game)
        {
            bool showPortrait = game.State == GameState.Playing;
            title.gameObject.SetActive(showPortrait);
            if (portrait != null && portrait.transform.parent != null)
                portrait.transform.parent.gameObject.SetActive(showPortrait);
            if (portrait != null) portrait.gameObject.SetActive(false);
            if (strategyLabel != null) strategyLabel.gameObject.SetActive(false);
            if (controls != null) controls.gameObject.SetActive(false);

            // La bandeja de la escena ocupa el Canvas y se posiciona según el rol.
            if (taskList != null)
            {
                var listRect = taskList as RectTransform;
                listRect.anchorMin = Vector2.zero;
                listRect.anchorMax = Vector2.one;
                listRect.offsetMin = Vector2.zero;
                listRect.offsetMax = Vector2.zero;
                var layout = taskList.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                if (layout != null) layout.enabled = false;
            }

            int count = game.Tasks.Tasks.Count;
            while (cards.Count < count)
                cards.Add(Instantiate(taskCardPrefab, taskList, false));
            for (int i = 0; i < cards.Count; i++)
            {
                if (i < count)
                {
                    var task = game.Tasks.Tasks[i];
                    cards[i].gameObject.SetActive(!task.IsFinal);
                    var rect = cards[i].transform as RectTransform;
                    Vector2 position = game.ActiveRole == CharacterRole.Boss ? bossTaskPositions[i % 3] : workerTaskPositions[i % 3];
                    rect.anchorMin = position;
                    rect.anchorMax = position;
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(310, 54);
                    cards[i].Bind(game, i, taskDecisionPanel);
                    cards[i].Refresh(task, i == game.Selected, false);
                }
                else
                {
                    cards[i].gameObject.SetActive(false);
                }
            }
        }

        IEnumerator AnimateWaveTransition(GameManager game)
        {
            const float duration = .25f;
            float direction = game.ActiveRole == CharacterRole.Boss ? 1f : -1f;
            float distance = taskList != null ? taskList.rect.width + 320f : 1600f;
            float elapsed = 0;

            foreach (var card in cards)
                if (card != null) card.SetWaveTransition(0, 1);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                foreach (var card in cards)
                    if (card != null) card.SetWaveTransition(direction * distance * t, 1f - t);
                yield return null;
            }

            if (game == null || game.State != GameState.Playing || !game.Tasks.BeginNextWave())
            {
                foreach (var card in cards)
                    if (card != null) card.ResetWaveTransition();
                waveTransition = null;
                yield break;
            }

            RefreshCards(game);
            foreach (var card in cards)
                if (card != null) card.SetWaveTransition(-direction * distance, 0);

            elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                foreach (var card in cards)
                    if (card != null) card.SetWaveTransition(-direction * distance * (1f - t), t);
                yield return null;
            }

            foreach (var card in cards)
                if (card != null) card.ResetWaveTransition();
            game.Tasks.FinishWaveTransition();
            waveTransition = null;
        }
    }
}
