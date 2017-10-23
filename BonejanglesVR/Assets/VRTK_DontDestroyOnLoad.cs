using UnityEngine;
using VRTK;
using System.Collections;

namespace com.EvolveVR.BonejanglesVR
{
    public class VRTK_DontDestroyOnLoad : MonoBehaviour
    {
        // Use this for initialization
        private void Start() {
            StartCoroutine(DontDestroyAutogens());
        }

        private IEnumerator DontDestroyAutogens()
        {
            yield return new WaitForSeconds(2.0f);
            foreach (VRTK_TransformFollow tf in FindObjectsOfType<VRTK_TransformFollow>())
                DontDestroyOnLoad(tf.gameObject);
            foreach (VRTK_PlayerObject po in FindObjectsOfType<VRTK_PlayerObject>()) {
                Transform parentT = po.transform.parent;
                if(parentT == null) // means its root
                    DontDestroyOnLoad(po.gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}