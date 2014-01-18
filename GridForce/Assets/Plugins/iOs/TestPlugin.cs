using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class TestPlugin : MonoBehaviour {

	[DllImport("__Internal")]
	private static extern void testFunction();

	void OnMouseDown(){
		testFunction();
		//this.setMyTexty("Bla");
	}

	public void setMyTexty(string texty){
		TextMesh txt = this.GetComponentInChildren<TextMesh>();
		txt.text = texty;
	}
}
