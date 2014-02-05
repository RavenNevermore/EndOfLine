using UnityEngine;
using System.Collections;

public class ExtendedBehaviour : MonoBehaviour {

	public void ShoutMessage(string name){
		this.ShoutMessage(name, null);
	}

	public void ShoutMessage(string name, object value){
		Object[] objs = GameObject.FindObjectsOfType(typeof(Transform));
		foreach (Object obj in objs){
			Transform t = (Transform) obj;
			if (null == t.parent){
				if (null == value)
					t.gameObject.SendMessage(name);
				else
					t.gameObject.SendMessage(name, value);
			}
		}
	}
}
