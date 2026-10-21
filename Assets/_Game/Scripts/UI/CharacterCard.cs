using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Caudal
{
    public sealed class CharacterCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] RawImage portrait;
        [SerializeField] Image cardImage;
        [SerializeField] Sprite normalCardSprite;
        [SerializeField] Text roleLabel, nameLabel, biographyLabel, bonusesLabel, controlsLabel;
        [SerializeField] Text previousArrow, nextArrow;
        [SerializeField] GameObject readyIndicator;
        [SerializeField, Range(1f, 1.1f)] float selectedScale = 1.035f;

        Vector3 baseScale;
        GameObject outlineObject;
        Texture2D outlineTexture;
        Sprite outlineSprite;
        bool selected;
        bool pointerOver;
        bool bossCard;
        CharacterSelectionUI selection;

        void Awake()
        {
            baseScale = transform.localScale;
            if (cardImage == null) cardImage = GetComponent<Image>();
            if (normalCardSprite == null && cardImage != null) normalCardSprite = cardImage.sprite;
            if (cardImage != null && normalCardSprite != null)
                cardImage.sprite = normalCardSprite;
            CreateSelectionOutline();
        }

        void Update()
        {
            bool highlighted = selected || pointerOver;
            Vector3 targetScale = baseScale * (highlighted ? selectedScale : 1f);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 12f);
            if (outlineObject != null && outlineObject.activeSelf != highlighted)
                outlineObject.SetActive(highlighted);
        }

        void CreateSelectionOutline()
        {
            if (cardImage == null || normalCardSprite == null || transform.parent == null) return;
            if (!CreateOutlineSprite()) return;

            var outline = new GameObject(name + " Outline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            outline.transform.SetParent(transform.parent, false);
            outline.transform.SetSiblingIndex(transform.GetSiblingIndex());

            var sourceRect = (RectTransform)transform;
            var outlineRect = (RectTransform)outline.transform;
            outlineRect.anchorMin = sourceRect.anchorMin;
            outlineRect.anchorMax = sourceRect.anchorMax;
            outlineRect.pivot = sourceRect.pivot;
            outlineRect.sizeDelta = sourceRect.sizeDelta;
            outlineRect.anchoredPosition3D = sourceRect.anchoredPosition3D;
            outlineRect.localRotation = sourceRect.localRotation;
            outlineRect.localScale = Vector3.one * 1.06f;

            var image = outline.GetComponent<Image>();
            image.sprite = outlineSprite;
            image.color = new Color32(181, 238, 247, 255);
            image.preserveAspect = cardImage.preserveAspect;
            image.raycastTarget = false;
            outlineObject = outline;
            outlineObject.SetActive(false);
        }

        bool CreateOutlineSprite()
        {
            var source = normalCardSprite.texture;
            if (source == null || !source.isReadable)
            {
                Debug.LogWarning("CharacterCard necesita la textura del sprite en modo Read/Write para dibujar el contorno.", this);
                return false;
            }

            var pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(255, 255, 255, pixels[i].a);

            outlineTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            outlineTexture.name = normalCardSprite.name + " Outline Mask";
            outlineTexture.hideFlags = HideFlags.DontSave;
            outlineTexture.filterMode = source.filterMode;
            outlineTexture.wrapMode = TextureWrapMode.Clamp;
            outlineTexture.SetPixels32(pixels);
            outlineTexture.Apply(false, true);

            var rect = normalCardSprite.rect;
            var pivot = new Vector2(normalCardSprite.pivot.x / rect.width, normalCardSprite.pivot.y / rect.height);
            outlineSprite = Sprite.Create(outlineTexture, rect, pivot, normalCardSprite.pixelsPerUnit);
            outlineSprite.name = normalCardSprite.name + " Outline Mask";
            outlineSprite.hideFlags = HideFlags.DontSave;
            return true;
        }

        void OnDestroy()
        {
            if (outlineObject != null) Destroy(outlineObject);
            if (outlineSprite != null) Destroy(outlineSprite);
            if (outlineTexture != null) Destroy(outlineTexture);
        }

        public void SetPointerOwner(CharacterSelectionUI owner, bool isBoss)
        {
            selection = owner;
            bossCard = isBoss;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pointerOver = true;
            if (selection != null) selection.SelectFromPointer(bossCard, false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerOver = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (selection != null) selection.SelectFromPointer(bossCard, true);
        }

        public void Refresh(CharacterData character, RoleKeys keys, bool ready, bool boss, bool canCycle)
        {
            selected = ready;
            if (readyIndicator != null) readyIndicator.SetActive(false);

            if (roleLabel != null) roleLabel.text = boss ? "JEFA" : "EMPLEADO";
            if (portrait != null) GameScreen.SetPortrait(portrait, character != null ? character.portrait : null);
            nameLabel.text = boss ? "JOSEFA" : "TOMAS";
            biographyLabel.text = character != null ? character.biography : "";
            if (bonusesLabel != null) bonusesLabel.text = character != null
                ? $"Cuota +{character.quotaBonusPercent:0}% · Productividad +{character.productivityBonusPercent:0}%\nCoste de felicidad -{character.happinessCostReductionPercent:0}%"
                : "";
            if (controlsLabel != null) controlsLabel.text = ready
                ? "PERSPECTIVA ELEGIDA"
                : $"← / → ELEGIR · {FormatKey(keys.confirm)} INICIAR";
            if (previousArrow != null) previousArrow.gameObject.SetActive(false);
            if (nextArrow != null) nextArrow.gameObject.SetActive(false);
        }

        static string FormatKey(Key key)
        {
            switch (key)
            {
                case Key.LeftArrow: return "←";
                case Key.RightArrow: return "→";
                case Key.UpArrow: return "↑";
                case Key.DownArrow: return "↓";
                case Key.Enter: return "ENTER";
                case Key.RightShift: return "SHIFT DER.";
                default: return key.ToString().ToUpperInvariant();
            }
        }
    }
}
