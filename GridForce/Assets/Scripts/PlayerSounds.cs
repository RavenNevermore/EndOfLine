using UnityEngine;
using System.Collections.Generic;

public class PlayerSounds : MonoBehaviour {

	public AudioSource turnSound;
	public AudioSource boostSound;
	public AudioSource killPlayerSound;
	public AudioSource sawbladeSound;
	public AudioSource shieldSound;
	public AudioSource mineSound;

	public void OnPlayerTurned(){
		if (null == this.turnSound)
			return;

        if (AudioSettings.playSounds)
		    turnSound.Play();
	}

	public void OnBoostStarted(){
		if (null == this.boostSound)
			return;
        if (AudioSettings.playSounds)
		    this.boostSound.Play();
	}
	
	public void OnPlayerDied(){
        if (AudioSettings.playSounds)
		    this.killPlayerSound.Play();
	}
	
	public void OnSawbladeStarted(){
        if (AudioSettings.playSounds)
		    this.sawbladeSound.Play();
	}
	
	public void OnShieldStarted(){
        if (AudioSettings.playSounds)
		    this.shieldSound.Play();
	}
	
	public void OnMineCharged(){
        if (AudioSettings.playSounds)
		    this.mineSound.Play();
	}
	
}
