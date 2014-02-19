using UnityEngine;
using System.Collections;

public class MusicOnOff : MonoBehaviour
{
    public GameObject musicOn = null;
    public GameObject musicOff = null;

    // Use this for initialization
    void Start()
    {
        this.SetMusicButton();
    }

    void OnMouseDown()
    {
        AudioSettings.playMusic = !(AudioSettings.playMusic);
        this.SetMusicButton();
    }

    void OnEnable()
    {
        this.SetMusicButton();
    }

    void SetMusicButton()
    {
        if (AudioSettings.playMusic)
        {
            this.musicOn.SetActive(true);
            this.musicOff.SetActive(false);
        }
        else
        {
            this.musicOn.SetActive(false);
            this.musicOff.SetActive(true);
        }
    }
}
