using UnityEngine;
using UnityEditor;
using com.EvolveVR.BonejanglesVR;

[ExecuteInEditMode]
public class ModifyJointComponentValues : MonoBehaviour
{
    public float connectedMassScale = 2.0f;
    public float massScale = 5.0f;
    public float massOfBones = 3.0f;

    private void OnEnable()
    {
        Debug.Log("Running Editor Script");
        foreach (JointNode jn in FindObjectsOfType<JointNode>()) {
            Joint j = jn.GetComponent<Joint>();
            if (j) {
                j.connectedMassScale = connectedMassScale;
                j.massScale = massScale;
            }
        }

        foreach(GameObject bone in GameObject.FindGameObjectsWithTag("Bone")) {
            Rigidbody rb = bone.GetComponent<Rigidbody>();
            if(rb)
                rb.mass = massOfBones;
        }
    }
}
