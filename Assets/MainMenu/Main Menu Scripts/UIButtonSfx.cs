using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData _) => UIAudio.I?.PlayHover();
    public void OnPointerClick(PointerEventData _) => UIAudio.I?.PlayClick();
}
