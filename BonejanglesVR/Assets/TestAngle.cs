using UnityEngine;
using com.EvolveVR.BonejanglesVR;

[ExecuteInEditMode]
public class TestAngle : MonoBehaviour
{
    private void OnEnable()
    {
        //transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        //1) zero out the x and the z axis
        float yAxisAngle = transform.eulerAngles.y;

        // 2) find the nearest multiple of 90 degrees;
        float a = RoundDown(yAxisAngle, 90);
        float b = RoundUp(yAxisAngle, 90);
        float c = Mathf.Abs(a - yAxisAngle);
        float d = Mathf.Abs(b - yAxisAngle);
        Debug.Log("A: " + a.ToString());
        Debug.Log("B: " + b.ToString());
        float nearest90DegreeRotation = c <= d ? a : b;

        transform.rotation = Quaternion.Euler(0, nearest90DegreeRotation, 0);
    }

    private float RoundDown(float v, int multiple)
    {
        float a = Mathf.Floor(v / multiple);
        float b = a * multiple;
        return b;
    }

    private float RoundUp(float v, int multiple)
    {
        if (multiple == 0)
            return 0;

        var remainder = Mathf.Abs(v) % multiple;
        if (remainder == 0)
            return v;
        if (v < 0)
            return -(Mathf.Abs(v) - remainder);
        else
            return v + multiple - remainder;
    }
}
