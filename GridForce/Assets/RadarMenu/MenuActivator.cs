using UnityEngine;
using System.Collections;

public class MenuActivator : MonoBehaviour
{
    public GameObject otherGameObject = null;

    void OnEnable()
    {
        this.otherGameObject.SetActive(true);
    }

    void OnDisable()
    {
        this.otherGameObject.SetActive(false);
    }
}
