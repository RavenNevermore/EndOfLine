using UnityEngine;
using System.Collections.Generic;

public class PlayerSounds : MonoBehaviour {

	public AudioSource turnSound;
	public AudioSource boostSound;

	public AudioClip[] soundClips;

	List<AudioSource> sources;

	void Start(){
		this.sources = new List<AudioSource>();
		if (null == this.soundClips || this.soundClips.Length == 0){
			this.sources.Add(turnSound);
		} else {
			foreach (AudioClip clip in this.soundClips){
				AudioSource source = (AudioSource) Instantiate(this.turnSound);
				source.clip = clip;
                source.transform.parent = this.transform;
				this.sources.Add(source);
			}
		}
	}

	public void OnPlayerTurned(){
		if (null == this.turnSound)
			return;

		foreach (AudioSource source in this.sources){
			if (source.isPlaying)
				continue;
            if (AudioSettings.playSounds)
			    source.Play();
			break;
		}
	}

	public void OnBoostStarted(){
		if (null == this.boostSound)
			return;
        if (AudioSettings.playSounds)
		    this.boostSound.Play();
	}
}
