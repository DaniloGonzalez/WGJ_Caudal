using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SealContractsMinigame : TimedMinigame, IMinigameDragReceiver
{
    public override string ControlsHint => "Arrastra el sello hasta la zona verde de cada contrato.";

    [SerializeField] private RectTransform stamp;
    [SerializeField] private Image[] contracts;
    [SerializeField] private Image[] validZones;
    [SerializeField] private Sprite approvedContractSprite;
    [SerializeField] private TMP_Text progressText;

    private Sprite[] originalContracts;
    private bool[] sealedContracts;
    private int sealedCount;
    private int currentContract;
    private bool dragging;
    private int contractCount;

    protected override void Awake()
    {
        base.Awake();
        EnsureFourthContract();
    }

    protected override void OnMinigameStarted()
    {
        sealedCount = 0;
        currentContract = 0;
        dragging = false;
        contractCount = Difficulty == 1 ? 2 : Difficulty == 2 ? 3 : 4;
        var handle = stamp.GetComponent<MinigameDragHandle>();
        handle.RememberHome();
        handle.RestoreHome();
        if (originalContracts == null)
        {
            originalContracts = new Sprite[contracts.Length];
            for (int i = 0; i < contracts.Length; i++) originalContracts[i] = contracts[i].sprite;
        }
        sealedContracts = new bool[contractCount];
        stamp.gameObject.SetActive(true);

        float zoneScale = Difficulty == 1 ? 1f : Difficulty == 2 ? .78f : .58f;
        for (int i = 0; i < contracts.Length; i++)
        {
            bool visible = i < contractCount;
            contracts[i].gameObject.SetActive(visible);
            contracts[i].sprite = originalContracts[i];
            validZones[i].gameObject.SetActive(visible);
            validZones[i].rectTransform.localScale = Vector3.one * zoneScale;
            RectTransform cardRect = contracts[i].rectTransform;
            float x = contractCount == 2 ? (i == 0 ? .28f : .72f) : .14f + .72f * i / (contractCount - 1f);
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(x, .53f);
            cardRect.sizeDelta = new Vector2(contractCount == 4 ? 215f : 270f, 270f);
        }

        BindDragHandles();
        SetSelectionHighlight(validZones.Length > 0 ? validZones[0] : null);
        ShowProgress();
    }

    private void BindDragHandles()
    {
        foreach (var handle in GetComponentsInChildren<MinigameDragHandle>(true))
        {
            handle.Receiver = this;
            handle.Index = 0;
        }
    }

    public void DragStarted(MinigameDragHandle handle, PointerEventData eventData)
    {
        if (handle.Rect != stamp || sealedCount >= contractCount) return;
        dragging = true;
        SetSelectionHighlight(stamp.GetComponent<Image>());
        var group = handle.GetComponent<CanvasGroup>();
        if (group != null) group.blocksRaycasts = false;
        handle.BeginMove((RectTransform)transform, eventData);
    }

    public void DragMoved(MinigameDragHandle handle, PointerEventData eventData)
    {
        if (!dragging || handle.Rect != stamp) return;
        handle.Move(eventData);
    }

    public void DragEnded(MinigameDragHandle handle, PointerEventData eventData)
    {
        if (!dragging || handle.Rect != stamp) return;
        dragging = false;
        var group = handle.GetComponent<CanvasGroup>();
        if (group != null) group.blocksRaycasts = true;

        currentContract = -1;
        for (int i = 0; i < contractCount; i++)
            if (!sealedContracts[i] && RectTransformUtility.RectangleContainsScreenPoint(
                validZones[i].rectTransform, eventData.position, eventData.pressEventCamera))
            { currentContract = i; break; }
        handle.RestoreHome();
        if (currentContract >= 0)
        {
            contracts[currentContract].sprite = approvedContractSprite;
            contracts[currentContract].preserveAspect = true;
            validZones[currentContract].gameObject.SetActive(false);
            sealedCount++;
            sealedContracts[currentContract] = true;
            SetFeedback("Contrato sellado.");
            ShowProgress();
            if (sealedCount >= contractCount)
            {
                Finish(Caudal.InteractionResult.Success);
                return;
            }
            SetSelectionHighlight(validZones[System.Array.FindIndex(sealedContracts, value => !value)]);
        }
        else
        {
            AddError("Coloca el sello en la zona verde.");
            SetSelectionHighlight(validZones[System.Array.FindIndex(sealedContracts, value => !value)]);
        }

    }

    private void ShowProgress()
    {
        progressText.text = "Contratos sellados: " + sealedCount + " / " + contractCount;
    }

    private void EnsureFourthContract()
    {
        if (contracts.Length >= 4 || contracts.Length == 0) return;
        int oldLength = contracts.Length;
        var expandedContracts = new Image[oldLength + 1];
        var expandedZones = new Image[oldLength + 1];
        System.Array.Copy(contracts, expandedContracts, oldLength);
        System.Array.Copy(validZones, expandedZones, oldLength);
        GameObject copy = Instantiate(contracts[0].gameObject, contracts[0].transform.parent);
        copy.name = "Contract 4";
        expandedContracts[oldLength] = copy.GetComponent<Image>();
        expandedZones[oldLength] = FindValidZone(copy.transform);
        if (expandedZones[oldLength] == null)
        {
            Destroy(copy);
            return;
        }
        contracts = expandedContracts;
        validZones = expandedZones;
    }

    private static Image FindValidZone(Transform root)
    {
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
            if (image != root.GetComponent<Image>() && image.name.StartsWith("Valid zone"))
                return image;
        return null;
    }
}
