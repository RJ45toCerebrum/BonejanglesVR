using UnityEngine;

public class HangSkull : MonoBehaviour {
    public Rigidbody rb;
    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "cranium") {
            other.transform.position = transform.position;
            SpringJoint sj = other.gameObject.AddComponent<SpringJoint>();
            sj.autoConfigureConnectedAnchor = true;
            sj.anchor = new Vector3(0, sj.anchor.y + 0.1f, 0);
            sj.spring = 220;
            sj.damper = 220;
            sj.connectedBody = rb;
        }
    }
}
