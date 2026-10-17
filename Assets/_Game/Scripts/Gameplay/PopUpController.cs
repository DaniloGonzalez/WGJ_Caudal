using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Caudal;

public class PopUpController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private RectTransform minigameContainer;
    [SerializeField] private Button closeButton;

    private Action<InteractionResult> onCompleteCallback;
    private GameObject activeMinigameInstance;

    public void Setup(TaskInstance task, InteractionKind kind, GameObject minigamePrefab, Action<InteractionResult> onComplete)
    {
        titleText.text = task.Data.taskName;
        onCompleteCallback = onComplete;
        
        if (minigamePrefab != null && minigameContainer != null) {
            activeMinigameInstance = Instantiate(minigamePrefab, minigameContainer, false);

            if (activeMinigameInstance.TryGetComponent<IMinigame>(out var minigame))
            {
                minigame.OnFinished += FinishMinigame;
            }
        }

        if (closeButton != null) {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => FinishMinigame(InteractionResult.Cancelled));
        }
    }

    public void FinishMinigame(InteractionResult result)
    {
        onCompleteCallback?.Invoke(result);
        ClosePopUp();
    }

    public void ClosePopUp() {
        Destroy(gameObject);
    }
}
