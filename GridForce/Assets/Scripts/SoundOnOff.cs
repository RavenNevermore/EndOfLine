using UnityEngine;
using System.Collections;

public class SoundOnOff : MonoBehaviour
{
    public GameObject soundOn = null;
    public GameObject soundOff = null;

	// Use this for initialization
	void Start ()
    {
        this.SetSoundButton();
	}

    void OnMouseDown()
    {
        AudioSettings.playSounds = !(AudioSettings.playSounds);
        this.SetSoundButton();
    }

    void OnEnable()
    {
        this.SetSoundButton();
    }

    void SetSoundButton()
    {
        if (AudioSettings.playSounds)
        {
            this.soundOn.SetActive(true);
            this.soundOff.SetActive(false);
        }
        else
        {
            this.soundOn.SetActive(false);
            this.soundOff.SetActive(true);
        }
    }
}


public static class AudioSettings
{
    public static bool playSounds = true;
    public static bool playMusic = true;
}