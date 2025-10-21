using UnityEngine;

public class SwordEvents : MonoBehaviour //PLACEHOLDER TIL ARMS ARE IMPLEMENTED
{
    // Animation events call these 
    public void AttackEventRelay() { Relay("AttackEvent"); }
    public void PerfectStartRelay() { Relay("PerfectStart"); }
    public void PerfectEndRelay() { Relay("PerfectEnd"); }
    public void BlockEventRelay() { Relay("BlockEvent"); }   //  regular block trigger

    private void Relay(string method)
    {
        if (transform.parent != null)
            transform.parent.gameObject.SendMessageUpwards(method, SendMessageOptions.DontRequireReceiver);
    }
}
