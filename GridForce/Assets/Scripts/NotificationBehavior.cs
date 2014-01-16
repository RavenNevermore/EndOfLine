using UnityEngine;
using System.Collections;

public class NotificationBehavior : MonoBehaviour
{
    public float timeOnScreen = 5.0f;

	// Use this for initialization
	void Start ()
    {
        if (timeOnScreen > 0.0f)
            UnityEngine.Object.Destroy(this.gameObject, this.timeOnScreen);	
	}
}
