using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.EvolveVR.BonejanglesVR
{
	[RequireComponent(typeof(AudioSource))]
	public class JointNodeAudio : MonoBehaviour 
	{
		private static JointNodeAudio jointNodeAudio;

		public AudioSource audioSource;
		public AudioClip boneSnappedClip;
		public AudioClip boneUnsnappedClip;

		public static JointNodeAudio Instance 
		{
			get { return jointNodeAudio; }
		}

		private void Awake()
		{
			if (jointNodeAudio)
				Destroy (this);

			jointNodeAudio = this;

			foreach (JointNode jn in FindObjectsOfType<JointNode>()) {
				jn.OnBoneSnapped += OnBoneSnapped;
				jn.OnBoneUnsnapped += OnBoneUnsnapped;
			}
		}

		private void OnBoneSnapped(JointNode jn){
			transform.position = jn.transform.position;
			audioSource.clip = boneSnappedClip;
			audioSource.Play ();
		}

		private void OnBoneUnsnapped(JointNode jn){
			transform.position = jn.transform.position;
			audioSource.clip = boneUnsnappedClip;
			audioSource.Play ();
		}
	}
}