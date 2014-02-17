using UnityEngine;
using System.Collections;

public class GameInitializer : MonoBehaviour
{
	public MenuState debugMenuState;

    public GameObject previewState;

    public GameObject previewHostObject;
    public GameObject previewClientObject;

	// Use this for initialization
	void Start ()
    {
		Debug.Log("Initializzzing");
		MenuState lastMenuState = GameObject.FindObjectOfType<MenuState>();
		if (null == lastMenuState)
			lastMenuState = this.debugMenuState;
		Debug.Log("Menu State: "+lastMenuState);

		if (null == lastMenuState)
			this.previewState.SetActive(false);
		else
        {
			if (MenuState.GameType.HOST.Equals(lastMenuState.type))
            {
                this.previewHostObject.GetComponentInChildren<StartHostedGameBehaviour>().SetHostGame(lastMenuState);
				this.previewClientObject.SetActive(false);
				this.previewHostObject.SetActive(true);

                if (lastMenuState.selectingControls)
                    this.previewHostObject.GetComponentInChildren<StartHostedGameBehaviour>().HideAll();
			}
            else
            {
                this.previewClientObject.GetComponentInChildren<StartHostedGameBehaviour>().SetClientGame(lastMenuState);
				this.previewHostObject.SetActive(false);
                this.previewClientObject.SetActive(true);

                if (lastMenuState.selectingControls)
                    this.previewClientObject.GetComponentInChildren<StartHostedGameBehaviour>().HideAll();
            }

			this.previewState.SetActive(true);
		}
	}
}
