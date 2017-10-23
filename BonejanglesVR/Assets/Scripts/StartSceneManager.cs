using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class StartSceneManager : MonoBehaviour
{
    GameObject leftHand;
    GameObject rightHand;

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
        yield return new WaitForSeconds(audioSource.clip.length + 1.0f);


        AsyncOperation loadAsync = SceneManager.LoadSceneAsync("Scenes/Main");
        yield return new WaitUntil(() => loadAsync.isDone);
    }
}
