using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class SpriteButton : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClick; // Kéo thả trong Inspector



    public void OnPointerClick(PointerEventData eventData)
    {
        onClick.Invoke();
    }
}