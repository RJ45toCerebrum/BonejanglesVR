using UnityEngine;
using VRTK;
using System.Collections;

namespace com.EvolveVR.BonejanglesVR
{
    [RequireComponent(typeof(VRTK_SnapDropZone))]
    public class LegoManager : MonoBehaviour
    {
        private VRTK_SnapDropZone snapDropZone;
        private bool legoSpawnable = true;
        public float minTimeBetweenSpawns = 2f;
        private int numLegosSpawned = 0;


        public int NumLegosSpawned
        {
            get { return numLegosSpawned; }
        }


        private void Awake() {
            snapDropZone = GetComponent<VRTK_SnapDropZone>();
            snapDropZone.ObjectUnsnappedFromDropZone += LegoInstanced;
        }

        private void LegoInstanced(object sender, SnapDropZoneEventArgs e)
        {
            // I do this only because disabling the snapdropzone or 
            // turning off highlight zone or setting cloneOnUnsnap to false doesnt work
            if (legoSpawnable)
                StartCoroutine(LegoSpawnTimer());
            else
                Destroy(e.snappedObject);
        }

        IEnumerator LegoSpawnTimer()
        {
            numLegosSpawned++;
            legoSpawnable = false;
            yield return new WaitForSeconds(minTimeBetweenSpawns);
            legoSpawnable = true;
        }

    }
}