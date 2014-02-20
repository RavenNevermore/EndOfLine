using UnityEngine;
using System.Collections;

public class MuteMusik : MonoBehaviour {

	void Update () {
		if (null != this.audio){
			if (this.audio.enabled &&
					!AudioSettings.playMusic){
				this.audio.enabled = false;
			} else if (!this.audio.enabled &&
						AudioSettings.playMusic){
				this.audio.enabled = true;
			}
		}
	}
}
