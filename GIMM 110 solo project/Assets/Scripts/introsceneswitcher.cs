using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class introsceneswitcher : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene("main");
    }
}