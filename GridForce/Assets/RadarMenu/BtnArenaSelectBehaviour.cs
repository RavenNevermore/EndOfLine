using UnityEngine;
using System.Collections;

public class BtnArenaSelectBehaviour : AbstractMenuBehaviour
{
	public string arenaName;

	void Start()
    {
		TextMesh text = this.GetComponentInChildren<TextMesh>();
		if (null != text)
        {
			text.text = this.arenaName.Substring(0, Mathf.Min(10, this.arenaName.Length));
            if (text.text.Length < this.arenaName.Length)
                text.text += "...";
		}
	}

	void OnMouseDown()
    {
		this.gameState.arenaName = this.arenaName;
		Debug.Log("Selected Arena: " + this.arenaName);

		this.switchToMenu("03_select_vehicle");
	}
}
