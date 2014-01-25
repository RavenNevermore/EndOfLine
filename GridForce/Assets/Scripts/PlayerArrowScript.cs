using UnityEngine;
using System.Collections;

public class PlayerArrowScript : MonoBehaviour
{
    public Color arrowColor = Color.grey;
    public Transform target = null;
    public GameObject childObject = null;
    private Material childMaterial = null;
    private bool active = false;
    private Color currentColor;

	// Use this for initialization
	void Start ()
    {
        if (this.childObject == null)
        this.childObject = this.transform.GetChild(0).gameObject;
        this.childMaterial = this.childObject.GetComponent<MeshRenderer>().material;

        this.currentColor = this.arrowColor;
        this.childMaterial.SetColor("_Color", new Color(this.arrowColor.r, this.arrowColor.g, this.arrowColor.b, this.arrowColor.a));
        //this.childMaterial.SetColor("_TintColor", new Color(this.arrowColor.r, this.arrowColor.g, this.arrowColor.b, this.arrowColor.a));
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (this.target != null)
        {
            if (!(this.active))
            {
                this.childObject.SetActive(true);
                this.active = true;
            }

            if (this.currentColor != this.arrowColor)
            {
                this.childMaterial.SetColor("_Color", new Color(this.arrowColor.r, this.arrowColor.g, this.arrowColor.b, this.arrowColor.a));
                //this.childMaterial.SetColor("_TintColor", new Color(this.arrowColor.r, this.arrowColor.g, this.arrowColor.b, this.arrowColor.a));
                this.currentColor = this.arrowColor;
            }

            Vector3 targetVector = this.target.transform.position - this.transform.position;
            if (targetVector != Vector3.zero)
                this.transform.rotation = Quaternion.LookRotation(targetVector);
        }
        else
        {
            if (this.active)
            {
                this.childObject.SetActive(false);
                this.active = false;
            }
        }	
	}
}
