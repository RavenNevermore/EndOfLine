using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public unsafe class TestPlugin : MonoBehaviour {

	[DllImport("__Internal")]
	private static extern void testFunction();

	[DllImport("__Internal")]
	private static extern void other_test(string text);
	

	void OnMouseDown(){
		testFunction();
		//this.setMyTexty("Bla");
	}

	public static void otherTest(){
		string text = "What's up";
		StringBuilder sb = new StringBuilder("What the hell");
			TestPlugin.other_test("Everything goes!");

	}

	public void setMyTexty(string texty){
		TextMesh txt = this.GetComponentInChildren<TextMesh>();
		txt.text = texty;
	}
}
