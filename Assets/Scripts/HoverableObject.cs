using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoverableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent OnPointerEnterEvent, OnPointerExitEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent.Invoke();
    }
}
