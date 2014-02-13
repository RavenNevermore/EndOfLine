using UnityEngine;
using System.Collections;

public class TitleScreenClickBehavior : MonoBehaviour
{
    public GameObject nextMenu = null;

    // On mouse down
    void OnMouseDown()
    {
        this.nextMenu.SetActive(true);
        this.transform.parent.gameObject.SetActive(false);
    }
}
