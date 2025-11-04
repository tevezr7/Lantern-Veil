using UnityEngine;

public class SwordEvents : MonoBehaviour 
{
    // Animation events call these 
    public void PAttackStartRelay() { Relay("PAttackStartEvent"); }
    public void PAttackEndRelay() { Relay("PAttackEndEvent"); }
    public void AttackEventRelay() { Relay("AttackEvent"); }
    public void AttackStartRelay() { Relay("AttackStartEvent"); }
    public void AttackEndRelay() { Relay("AttackEndEvent"); }
    public void PerfectStartRelay() { Relay("PerfectStart"); }
    public void PerfectEndRelay() { Relay("PerfectEnd"); }
    public void BlockEventRelay() { Relay("BlockEvent"); }   //  regular block trigger

    private void Relay(string method)
    {
        if (transform.parent != null)
            transform.parent.gameObject.SendMessageUpwards(method, SendMessageOptions.DontRequireReceiver);
    }
}
