using UnityEngine;
using System.Collections.Generic;

namespace com.EvolveVR.BonejanglesVR
{
    // this is a general trigger even system so that you can capture
    // trigger events from different game objects in easy way
    public class GeneralTriggerEvent : MonoBehaviour
    {
        public enum FilterType {Name, Tag};
        public FilterType filterType;
        public string filterInfo;

        public Collider[] ignoreColliders;
        private HashSet<Collider> ignoredColliders;

        public delegate void TriggerEnterEvent(Collider other);
        public delegate void TriggerStayEvent(Collider other);
        public delegate void TriggerExitEvent(Collider other);
        private int numOnTriggerEnterSubs = 0;
        private event TriggerEnterEvent onTriggerEnter;
        private int numOnTriggerStaySubs = 0;
        private event TriggerStayEvent onTriggerStay;
        private int numOnTriggerExitSubs = 0;
        private event TriggerExitEvent onTriggerExit;

        private void Start()
        {
            Collider collider = GetComponent<Collider>();
            if (!collider) 
                Debug.LogError("The GeneralTriggerEvent object will not work without a collider that is a trigger");
            else if (!collider.isTrigger) 
                Debug.LogError("The Collider on GameObejct with a GeneralTriggerEvent component must be a trigger");

            ignoredColliders = new HashSet<Collider>();
            if (ignoreColliders.Length != 0) {
                foreach (Collider c in ignoreColliders)
                    ignoredColliders.Add(c);
            }
        }

        public void SubOnTriggerEnterEvent(TriggerEnterEvent callback)
        {
            if (callback != null) {
                onTriggerEnter += callback;
                numOnTriggerEnterSubs++;
            }
        }

        public void SubOnTriggerExitEvent(TriggerExitEvent callback)
        {
            if (callback != null) {
                onTriggerExit += callback;
                numOnTriggerExitSubs++;
            }
        }

        public void SubOnTriggerStayEvent(TriggerStayEvent callback)
        {
            if (callback != null) {
                onTriggerStay += callback;
                numOnTriggerStaySubs++;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (numOnTriggerEnterSubs == 0 && ignoredColliders.Contains(other))
                return;

            if(filterType == FilterType.Name) {
                if (other.name == filterInfo) {
                    if(numOnTriggerEnterSubs > 0)
                        onTriggerEnter(other);
                }
            }
            else {
                if (other.tag == filterInfo) {
                    if (numOnTriggerEnterSubs > 0)
                        onTriggerEnter(other);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (numOnTriggerStaySubs == 0 && ignoredColliders.Contains(other))
                return;

            if (filterType == FilterType.Name) {
                if (other.name == filterInfo) {
                    if(numOnTriggerStaySubs > 0)
                        onTriggerStay(other);
                }
            }
            else {
                if (other.tag == filterInfo) {
                    if(numOnTriggerStaySubs > 0)
                        onTriggerStay(other);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (numOnTriggerExitSubs == 0 && ignoredColliders.Contains(other))
                return;

            if (filterType == FilterType.Name) {
                if (other.name == filterInfo) {
                    if(numOnTriggerExitSubs > 0)
                        onTriggerExit(other);
                }
            }
            else {
                if (other.tag == filterInfo) {
                    if (numOnTriggerExitSubs > 0)
                        onTriggerExit(other);
                }
            }
        }

        public void AddIgnoreCollider(Collider other) {
            ignoredColliders.Add(other);
        }
    }
}