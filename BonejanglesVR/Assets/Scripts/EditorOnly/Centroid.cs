using UnityEngine;

[ExecuteInEditMode]
public class Centroid : MonoBehaviour
{
    public Transform[] ts;
    public Transform centroid;

    public bool isDebug = false;
	
	// Update is called once per frame
	void Update ()
    {
        if (!isDebug)
            return;

        Vector3 v = new Vector3();
        foreach (Transform t in ts) {
            v.x += t.position.x;
            v.y += t.position.y;
            v.z += t.position.z;
        }

        v /= ts.Length;
        centroid.position = v;

    }
}
