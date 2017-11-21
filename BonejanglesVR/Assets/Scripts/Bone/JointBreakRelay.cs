using UnityEngine;
using System.Collections.Generic;

namespace com.EvolveVR.BonejanglesVR
{
    /// <summary>
    /// This is a small script to call methods on other scripts when a joint attached
    /// to this gameobject breaks; Other scripts who want the relay just implement
    /// JointBreakRelay.IJointBreak
    /// </summary>
    public class JointBreakRelay : MonoBehaviour
    {
        public interface IJointBreak
        {
            void OnJointBreak(float breakForce);
        }

        private List<IJointBreak> listeners;

        private void Awake() {
            listeners = new List<IJointBreak>();
        }

        public void AddListener(IJointBreak listener) {
            if(listener != null)
                listeners.Add(listener);
        }

        private void OnJointBreak(float breakForce)
        {
            foreach(IJointBreak listener in listeners)
                listener.OnJointBreak(breakForce);
        }
    }
}