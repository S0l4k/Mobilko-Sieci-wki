using UnityEngine;

using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("UI Elements")]
    public RectTransform background;
    public RectTransform handle;

    [Header("Settings")]
    public float handleRange = 50f; // Max odleg³oœæ uchwytu od œrodka BG

    private Vector2 input = Vector2.zero;

    public Vector2 Direction => input;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out pos);

        pos = Vector2.ClampMagnitude(pos, handleRange);
        handle.anchoredPosition = pos;

        input = pos / handleRange; // Normalizowany wektor -1 do 1
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
