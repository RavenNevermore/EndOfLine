﻿using UnityEngine;
using System.Collections;

public class AbstractMenuBehaviour : MonoBehaviour
{
	public void switchToMenu(string name)
    {
		this.switchToMenu(name, true);
	}

	public void switchToMenu(string name, bool findReturnPath)
    {
		string backPath = null;
		if (findReturnPath)
        {
			backPath = this.transform.parent.gameObject.name;
            if (backPath != null && backPath.Contains("beacon"))
                backPath = this.transform.parent.parent.gameObject.name;
		}

		GameObject root = GameObject.Find("root");
        if (root != null)
        {
            MenuSelectSwitch menuSwitch = root.GetComponent<MenuSelectSwitch>();
            menuSwitch.switchMenu(name, backPath);
        }
	}

	public MenuState gameState
    {
		get
        {
			return GameObject.FindObjectOfType<MenuState>();
		}
	}
}
