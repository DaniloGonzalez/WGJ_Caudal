using UnityEngine;
using UnityEngine.EventSystems;

namespace Caudal
{
    public sealed class PointerHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Range(1f, 1.2f)] float scaleMultiplier = 1.06f;

        RectTransform target;
        Vector3 originalScale;

        void Awake()
        {
            target = transform as RectTransform;
            if (target != null)
                originalScale = target.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (target != null)
                target.localScale = originalScale * scaleMultiplier;
        }

        public void OnPointerExit(PointerEventData eventData) => ResetScale();

        void OnDisable() => ResetScale();

        void ResetScale()
        {
            if (target != null)
                target.localScale = originalScale;
        }
    }
}
