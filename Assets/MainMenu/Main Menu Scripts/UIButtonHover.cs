using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float hoverScale = 1.06f;
    [SerializeField] float pressedScale = 0.98f;
    [SerializeField] float speed = 12f;

    Vector3 baseScale;
    float target = 1f;

    void Awake() => baseScale = transform.localScale;

    void Update()
    {
        var current = transform.localScale.x;
        var next = Mathf.Lerp(current, target, Time.unscaledDeltaTime * speed);
        transform.localScale = new Vector3(next, next, next) * baseScale.x;
    }

    public void OnPointerEnter(PointerEventData _) => target = hoverScale;
    public void OnPointerExit(PointerEventData _) => target = 1f;
    public void OnPointerDown(PointerEventData _) => target = pressedScale;
    public void OnPointerUp(PointerEventData _) => target = hoverScale;
}
