using UnityEngine;
using UnityEngine.UI;
using VRTK;
using System.Collections;

namespace com.EvolveVR.BonejanglesVR
{
    [RequireComponent(typeof(VRTK_SnapDropZone))]
    public class LegoManager : MonoBehaviour
    {
		private bool firstLegoSpawned = true;

        private VRTK_SnapDropZone snapDropZone;
        private bool legoSpawnable = true;
        public float minTimeBetweenSpawns = 2f;
        private int numLegosSpawned = 0;

		[SerializeField]
		private GameObject legoTipPrefab;
		private BoxCollider boxCollider;

		public Image readyImage;


        public int NumLegosSpawned
        {
            get { return numLegosSpawned; }
        }


        private void Awake() {
            snapDropZone = GetComponent<VRTK_SnapDropZone>();
            snapDropZone.ObjectUnsnappedFromDropZone += LegoInstanced;
			boxCollider = GetComponent<BoxCollider> ();
        }

        private void LegoInstanced(object sender, SnapDropZoneEventArgs e)
        {
            // I do this only because disabling the snapdropzone or 
            // turning off highlight zone or setting cloneOnUnsnap to false doesnt work
			if (legoSpawnable)
				StartCoroutine (LegoSpawnTimer (e.snappedObject));
            else
                Destroy(e.snappedObject);
        }

		IEnumerator LegoSpawnTimer(GameObject lego)
        {
            numLegosSpawned++;
            legoSpawnable = false;
			readyImage.color = Color.red;
            yield return new WaitForSeconds(minTimeBetweenSpawns);
			legoSpawnable = true;
			readyImage.color = Color.green;
			BoxCollider c = lego.GetComponent<BoxCollider> ();
			if (boxCollider.bounds.Intersects (c.bounds))
				Destroy (lego);
			else if (firstLegoSpawned) 
			{
				GameObject tip = Instantiate (legoTipPrefab);
				var t = lego.transform.Find ("CentroidPrimitive");
				tip.gameObject.transform.SetParent (t);
				tip.transform.position = t.position;
				Vector3 pv = tip.transform.localPosition;
				pv.y += 0.15f;
				tip.transform.localPosition = pv;
				firstLegoSpawned = false;
				Destroy (tip, 10f);
			}
        }
    }
}