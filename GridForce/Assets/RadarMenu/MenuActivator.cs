using UnityEngine;
using System.Collections;

public class MenuActivator : MonoBehaviour
{
    public GameObject otherGameObject = null;

    void OnEnable()
    {
		if (null == this.otherGameObject)
			return;
		
        this.otherGameObject.SetActive(true);
    }

    void OnDisable()
    {
		if (null == this.otherGameObject)
			return;
		
        this.otherGameObject.SetActive(false);
    }
}
