using Caudal;
using UnityEngine;
using UnityEngine.UI;

public class ApprovalMinigame : TimedMinigame
{
    [SerializeField] private Text requestText;
    [SerializeField] private Text decisionText;
    [SerializeField] private Text progressText;
    [SerializeField] private Image approveCard;
    [SerializeField] private Image rejectCard;
    [SerializeField] private Image requestBackground;
    [SerializeField] private Sprite requestCardSprite;

    private bool[] shouldApprove;
    private int requestIndex;
    private int decision;

    protected override void OnMinigameStarted()
    {
        if (requestBackground != null && requestCardSprite != null)
            requestBackground.sprite = requestCardSprite;
        if (approveCard != null) approveCard.color = Color.white;
        if (rejectCard != null) rejectCard.color = Color.white;
        shouldApprove = new bool[5];
        for (int i = 0; i < shouldApprove.Length; i++) shouldApprove[i] = i < 3;
        for (int i = shouldApprove.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shouldApprove[i], shouldApprove[j]) = (shouldApprove[j], shouldApprove[i]);
        }
        requestIndex = 0;
        decision = 0;
        ShowRequest();
    }

    protected override void ReadInput()
    {
        if (PressedUp || PressedDown)
        {
            decision = 1 - decision;
            ShowDecision();
        }
        if (!PressedConfirm) return;

        bool correct = (decision == 0) == shouldApprove[requestIndex];
        if (correct)
        {
            SetFeedback("Decisión correcta.");
        }
        else
        {
            AddError("Solicitud mal resuelta.");
            if (ErrorCount >= ErrorLimit)
                return;
        }

        requestIndex++;
        if (requestIndex >= shouldApprove.Length)
        {
            Finish(InteractionResult.Success);
            return;
        }
        ShowRequest();
    }

    private void ShowRequest()
    {
        requestText.text = "Solicitud " + (requestIndex + 1) + " de 5\nDebe " + (shouldApprove[requestIndex] ? "APROBARSE" : "RECHAZARSE");
        if (progressText != null) progressText.text = "Procesadas: " + requestIndex + " / 5";
        decision = 0;
        ShowDecision();
    }

    private void ShowDecision()
    {
        decisionText.text = decision == 0 ? "APROBAR  ◀" : "RECHAZAR  ◀";
        approveCard.transform.localScale = decision == 0 ? Vector3.one * 1.08f : Vector3.one;
        rejectCard.transform.localScale = decision == 1 ? Vector3.one * 1.08f : Vector3.one;
        Highlight(decision == 0 ? approveCard : rejectCard);
    }
}
