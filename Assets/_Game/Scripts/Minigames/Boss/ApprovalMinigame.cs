using Caudal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ApprovalMinigame : TimedMinigame, IPointerClickHandler
{
    public override string ControlsHint => "Lee cada solicitud y haz clic en Aprobar o Rechazar.";
    [SerializeField] private Text requestText;
    [SerializeField] private Text decisionText;
    [SerializeField] private Text progressText;
    [SerializeField] private Image approveCard;
    [SerializeField] private Image rejectCard;
    [SerializeField] private Image requestBackground;
    [SerializeField] private Sprite requestCardSprite;

    private bool[] shouldApprove;
    private string[] requestTexts;
    private int requestIndex;
    private int decision;

    protected override void OnMinigameStarted()
    {
        ConfigureLayout();
        if (requestBackground != null)
        {
            // El placeholder tiene líneas que compiten con el texto; se usa una tarjeta plana.
            requestBackground.sprite = null;
            requestBackground.color = new Color(.94f, .88f, .72f, 1f);
            requestBackground.preserveAspect = true;
        }
        if (approveCard != null) { approveCard.color = Color.white; approveCard.raycastTarget = true; }
        if (rejectCard != null) { rejectCard.color = Color.white; rejectCard.raycastTarget = true; }
        shouldApprove = new[] { true, true, true, false, false };
        requestTexts = new[]
        {
            "Ana cubrirá mi turno del viernes. ¿Autorizas el cambio?",
            "El cliente corrigió el total. ¿Actualizo y reenvío el informe?",
            "Interrumpieron mi pausa con una llamada de trabajo. ¿Puedo recuperarla?",
            "¿Autorizas compartir la nómina completa en el canal general?",
            "¿Borro las horas extra registradas para cerrar el mes?"
        };
        for (int i = shouldApprove.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shouldApprove[i], shouldApprove[j]) = (shouldApprove[j], shouldApprove[i]);
            (requestTexts[i], requestTexts[j]) = (requestTexts[j], requestTexts[i]);
        }
        requestIndex = 0;
        decision = 0;
        ShowRequest();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        var target = eventData.pointerPressRaycast.gameObject != null ? eventData.pointerPressRaycast.gameObject.transform : eventData.pointerCurrentRaycast.gameObject?.transform;
        if (target == null) return;
        if (target == approveCard.transform || target.IsChildOf(approveCard.transform)) SubmitDecision(0);
        else if (target == rejectCard.transform || target.IsChildOf(rejectCard.transform)) SubmitDecision(1);
    }

    private void SubmitDecision(int choice)
    {
        bool correct = (choice == 0) == shouldApprove[requestIndex];
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
        SetFeedback("");
        requestText.text = "SOLICITUD " + (requestIndex + 1) + "/" + requestTexts.Length + "\n\n" + requestTexts[requestIndex];
        if (progressText != null) progressText.text = "Procesadas: " + requestIndex + " / 5";
        decision = 0;
        ShowDecision();
    }

    private void ShowDecision()
    {
        decisionText.text = decision == 0 ? "◀ APROBAR" : "RECHAZAR ▶";
        approveCard.transform.localScale = decision == 0 ? Vector3.one * 1.08f : Vector3.one;
        rejectCard.transform.localScale = decision == 1 ? Vector3.one * 1.08f : Vector3.one;
        Highlight(decision == 0 ? approveCard : rejectCard);
    }

    private void ConfigureLayout()
    {
        SetAnchors(requestBackground != null ? requestBackground.rectTransform : null, new Vector2(.255f, .42f), new Vector2(.745f, .84f));
        SetAnchors(requestText != null ? requestText.rectTransform : null, new Vector2(.27f, .44f), new Vector2(.73f, .82f));
        SetAnchors(FeedbackLabel != null ? FeedbackLabel.rectTransform : null, new Vector2(.03f, .36f), new Vector2(.97f, .42f));
        SetAnchors(approveCard != null ? approveCard.rectTransform : null, new Vector2(.14f, .09f), new Vector2(.45f, .36f));
        SetAnchors(rejectCard != null ? rejectCard.rectTransform : null, new Vector2(.55f, .09f), new Vector2(.86f, .36f));
        SetAnchors(progressText != null ? progressText.rectTransform : null, new Vector2(.03f, .00f), new Vector2(.97f, .08f));

        if (requestText != null)
        {
            requestText.fontSize = 12;
            requestText.resizeTextForBestFit = true;
            requestText.resizeTextMinSize = 12;
            requestText.resizeTextMaxSize = 14;
            requestText.fontStyle = FontStyle.Bold;
            requestText.alignment = TextAnchor.MiddleCenter;
            requestText.color = Color.black;
            requestText.horizontalOverflow = HorizontalWrapMode.Wrap;
            requestText.verticalOverflow = VerticalWrapMode.Truncate;
        }

        if (decisionText != null)
        {
            decisionText.gameObject.SetActive(false);
        }

        if (FeedbackLabel != null)
        {
            FeedbackLabel.fontSize = 10;
            FeedbackLabel.resizeTextForBestFit = true;
            FeedbackLabel.resizeTextMinSize = 9;
            FeedbackLabel.resizeTextMaxSize = 10;
            FeedbackLabel.alignment = TextAnchor.MiddleCenter;
            FeedbackLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        if (progressText != null)
        {
            progressText.fontSize = 11;
            progressText.resizeTextForBestFit = true;
            progressText.resizeTextMinSize = 9;
            progressText.resizeTextMaxSize = 11;
            progressText.alignment = TextAnchor.MiddleCenter;
        }
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        if (rect == null) return;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
