using UnityEngine;
using System.Collections;

public class PlayerArrowScript : MonoBehaviour
{
    public Color arrowColor = Color.grey;
    public Transform target = null;
    public GameObject childObject = null;
    public float minDistance = 1.0f;
    public float maxDistance = 50.0f;
    public float minAlpha = 0.0005f;
    public float maxAlpha = 0.35f;
    private Material childMaterial = null;
    private bool targetActive = false;

	// Use this for initialization
	void Start ()
    {
        if (this.childObject == null)
        this.childObject = this.transform.GetChild(0).gameObject;
        this.childMaterial = this.childObject.GetComponent<MeshRenderer>().material;

        //this.childMaterial.SetColor("_Color", new Color(this.arrowColor.r, this.arrowColor.g, this.arrowColor.b, this.arrowColor.a));
        this.childMaterial.SetColor("_TintColor", new Color(this.arrowColor.r, this.arrowColor.g, this.arrowColor.b, this.arrowColor.a));
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (this.target != null)
        {
            if (!(this.targetActive))
            {
                this.childObject.SetActive(true);
                this.targetActive = true;
            }

            Vector3 targetVector = this.target.transform.position - this.transform.position;

            float distance = targetVector.magnitude;

            float colorA = maxAlpha;
            if (distance > maxDistance)
                colorA = minAlpha;
            else if (distance <= maxDistance && distance > minDistance)
                colorA = Mathf.Max((1.0f - ((distance - minDistance) / (maxDistance - minDistance))) * maxAlpha, minAlpha);

            //this.childMaterial.SetColor("_Color", new Color(this.arrowColor.r, this.arrowColor.g, this.arrowColor.b, this.arrowColor.a));
            this.childMaterial.SetColor("_TintColor", new Color(this.arrowColor.r, this.arrowColor.g, this.arrowColor.b, this.arrowColor.a * colorA));

            if (targetVector != Vector3.zero)
                this.transform.rotation = Quaternion.LookRotation(targetVector);
        }
        else
        {
            if (this.targetActive)
            {
                this.childObject.SetActive(false);
                this.targetActive = false;
            }
        }	
	}
}
