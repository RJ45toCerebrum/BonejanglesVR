using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using VRTK;


public class StartSceneManager : MonoBehaviour
{
	public VRTK_InteractGrab leftHand;
	public VRTK_InteractGrab rightHand;
	public com.EvolveVR.BonejanglesVR.MouseSlide mouseSlide;

    public GameObject placeholderAudioQueue;
    public AudioSource audioSource;

    private void Start(){
        placeholderAudioQueue.SetActive(false);
    }

    public void StartGame(){
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        placeholderAudioQueue.SetActive(true);
        audioSource.Play();
		mouseSlide.InteractableObject.ForceStopInteracting ();
		mouseSlide.InteractableObject.enabled = false;
        yield return new WaitForSeconds(audioSource.clip.length + 1.0f);

        AsyncOperation loadAsync = SceneManager.LoadSceneAsync("Scenes/Main");
        yield return new WaitUntil(() => loadAsync.isDone);
    }
}
