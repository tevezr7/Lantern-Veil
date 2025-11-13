using UnityEngine;

public class SpellEvents : MonoBehaviour
{
    public void SpellStart() => SRelay("SpellStartEvent");
    public void SpellEnd() => SRelay("SpellEndEvent");
    public void SpellCast() => SRelay("SpellCastEvent");

    private void SRelay(string method)
    {
        // if there's a parent, start there; if not, start at self
        var origin = transform.parent ? transform.parent.gameObject : gameObject;
        // Broadcast down the hierarchy so children receive it
        origin.BroadcastMessage(method, SendMessageOptions.DontRequireReceiver);
    }
}
