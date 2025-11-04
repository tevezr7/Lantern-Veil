using UnityEngine;

public class SpellEvents : MonoBehaviour
{
    public void SpellStart() { SRelay("SpellStartEvent"); }
    public void SpellEnd() { SRelay("SpellEndEvent"); }
    public void SpellCast() { SRelay("SpellCastEvent"); }
    private void SRelay(string method)
    {
        if (transform.parent != null)
            transform.parent.gameObject.SendMessageUpwards(method, SendMessageOptions.DontRequireReceiver);
    }
}
