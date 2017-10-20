using UnityEngine;

[ExecuteInEditMode]
public class TestAngle : MonoBehaviour
{
    public Transform otherTransform;

    private void OnEnable()
    {
        
        //transform.rotation *= relative;

        for (int i = 0; i < 100; i++) {
            Quaternion rotDelta = Quaternion.FromToRotation(transform.up, otherTransform.up);
            transform.rotation *= rotDelta;
        }
        Quaternion relative = Quaternion.Inverse(transform.rotation) * otherTransform.rotation;
        Debug.Log(Vector3.Angle(otherTransform.forward, transform.forward));
        transform.rotation *= Quaternion.AngleAxis(relative.eulerAngles.y, transform.up);
        //float angle = Quaternion.Angle(transform.rotation, otherTransform.rotation);
    }


}
