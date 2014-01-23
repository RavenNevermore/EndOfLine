using UnityEngine;
using System.Collections;

public class CountdownDigitBehavior : MonoBehaviour
{
    public CountdownDigitBehavior nextDigit = null;
    public GameState gameState = null;
    public float timePassed = 0.0f;

    private GUITexture guiTextureComponent = null;
    private Rect textureDimensions;
    private Color textureColor;
    bool playedSound = false;

	// Use this for initialization
	void Start ()
    {
        this.guiTextureComponent = this.GetComponent<GUITexture>();
        this.textureDimensions = this.guiTextureComponent.pixelInset;
        this.textureColor = this.guiTextureComponent.color;

        this.guiTextureComponent.pixelInset = new Rect(this.textureDimensions.xMin * 2.0f, this.textureDimensions.yMin * 2.0f, this.textureDimensions.width * 2.0f, this.textureDimensions.height * 2.0f);
        this.guiTextureComponent.color = new Color(this.textureColor.r, this.textureColor.g, this.textureColor.b, 0.0f);
	}
	
	// Update is called once per frame
	void Update ()
    {
        this.timePassed += Time.deltaTime;

        if (this.gameState == null)
        {
            if (this.timePassed < 0.5f)
            {
                float size = 2.0f - (this.timePassed * 2.0f);
                float colorAlpha = this.textureColor.a * Mathf.Clamp((this.timePassed * 2.0f), 0.0f, 1.0f);

                this.guiTextureComponent.pixelInset = new Rect(this.textureDimensions.xMin * size, this.textureDimensions.yMin * size, this.textureDimensions.width * size, this.textureDimensions.height * size);
                this.guiTextureComponent.color = new Color(this.textureColor.r, this.textureColor.g, this.textureColor.b, colorAlpha);
            }
            else if (this.timePassed >= 0.5f && this.timePassed < 0.75f)
            {
                if (!(this.playedSound))
                {
                    // Insert sound effect here

                    this.playedSound = true;
                }

                this.guiTextureComponent.pixelInset = this.textureDimensions;
                this.guiTextureComponent.color = this.textureColor;
            }
            else
            {
                float colorAlpha = this.textureColor.a * Mathf.Clamp((0.25f - (this.timePassed - 0.75f)) * 4.0f, 0.0f, 1.0f);

                this.guiTextureComponent.pixelInset = this.textureDimensions;
                this.guiTextureComponent.color = new Color(this.textureColor.r, this.textureColor.g, this.textureColor.b, colorAlpha);
            }


            if (this.timePassed > 1.0f)
            {
                this.nextDigit.timePassed = this.timePassed - 1.0f;
                this.nextDigit.gameObject.SetActive(true);
                this.gameObject.SetActive(false);
            }

        }
        else
        {
            if (this.timePassed < 0.25f)
            {
                float size = 2.0f - (this.timePassed * 4.0f);
                float colorAlpha = this.textureColor.a * Mathf.Clamp((this.timePassed * 4.0f), 0.0f, 1.0f);

                this.guiTextureComponent.pixelInset = new Rect(this.textureDimensions.xMin * size, this.textureDimensions.yMin * size, this.textureDimensions.width * size, this.textureDimensions.height * size);
                this.guiTextureComponent.color = new Color(this.textureColor.r, this.textureColor.g, this.textureColor.b, colorAlpha);
            }
            else if (this.timePassed >= 0.25f && this.timePassed < 0.75f)
            {
                if (!(this.playedSound))
                {
                    // Insert sound effect here

                    this.gameState.CountdownOver();
                    this.playedSound = true;
                }
                this.guiTextureComponent.pixelInset = this.textureDimensions;
                this.guiTextureComponent.color = this.textureColor;
            }
            else
            {
                float size = 2.0f - ((0.25f - (this.timePassed - 0.75f)) * 4.0f);
                float colorAlpha = this.textureColor.a * Mathf.Clamp((0.25f - (this.timePassed - 0.75f)) * 4.0f, 0.0f, 1.0f);

                this.guiTextureComponent.pixelInset = new Rect(this.textureDimensions.xMin * size, this.textureDimensions.yMin * size, this.textureDimensions.width * size, this.textureDimensions.height * size);
                this.guiTextureComponent.color = new Color(this.textureColor.r, this.textureColor.g, this.textureColor.b, colorAlpha);
            }

            if (this.timePassed > 1.0f)
            {
                this.transform.parent.gameObject.SetActive(false);
                this.gameObject.SetActive(false);
            }
        }

	}
}
