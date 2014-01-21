using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ErrorState : MonoBehaviour
{
    public GUISkin guiSkin = null;

    private bool displayConsole = false;
    private float consoleYPos = 0;
    private int consoleWidth = 355;
    private int consoleHeight = 100;
    private int boxHeight = 60;
    private string errorText = "";
    private List<ErrorGuiButton> buttonList = new List<ErrorGuiButton>();

	// Use this for initialization
	void Start()
    {
        DontDestroyOnLoad(this);
        this.consoleYPos = Screen.height;

        GameObject[] errorStates = GameObject.FindGameObjectsWithTag("ErrorState");
        if (errorStates.Length > 1)
            UnityEngine.Object.Destroy(this.gameObject);

        //this.Clear();
        //this.SetErrorMessage("Connecting...\nError: Connection to Server failed");
        //this.AddButton("Cancel", null);
        //this.AddButton("Retry", null);
        //this.Show();
	}
	
	// Update is called once per frame
	void Update()
    {
        if (this.displayConsole)
        {
            if (this.consoleYPos > Screen.height - this.consoleHeight - 5)
            {
                this.consoleYPos -= Time.deltaTime * 600;
                if (this.consoleYPos <= Screen.height - this.consoleHeight - 5)
                    this.consoleYPos = Screen.height - this.consoleHeight - 5;
            }
            else if (this.consoleYPos < Screen.height - this.consoleHeight - 5)
            {
                this.consoleYPos += Time.deltaTime * 600;
                if (this.consoleYPos >= Screen.height - this.consoleHeight - 5)
                    this.consoleYPos = Screen.height - this.consoleHeight - 5;
            }
        }
        else
        {
            if (this.consoleYPos < Screen.height)
            {
                this.consoleYPos += Time.deltaTime * 600;
                if (this.consoleYPos >= Screen.height)
                {
                    this.consoleYPos = Screen.height;
                    this.Clear();
                }
            }
        }	
	}

    void OnGUI()
    {
        GUI.skin = this.guiSkin;

        GUI.BeginGroup(new Rect(5, this.consoleYPos, this.consoleWidth, this.consoleHeight));

        GUI.Box(new Rect(0, this.consoleHeight - this.boxHeight, this.consoleWidth - 105, this.boxHeight), this.errorText);
        GUI.Label(new Rect(0, this.consoleHeight - this.boxHeight, this.consoleWidth - 105, this.boxHeight), this.errorText);
        
        bool active = (this.consoleYPos <= Screen.height - this.consoleHeight) && this.displayConsole;
        for (int i = 0; i < this.buttonList.Count; i++)
        {
            this.buttonList[i].OnGUI(active);
        }

        GUI.EndGroup();
    }

    public void Show()
    {
        this.displayConsole = true;
    }

    public void Hide()
    {
        this.displayConsole = false;
    }

    public void Clear()
    {
        this.buttonList.Clear();
        this.errorText = "";
    }

    public void AddButton(string buttonText, ErrorGuiButtonDelegate ButtonFunction)
    {
        int xPos = this.consoleWidth - 100;
        int yPos = this.consoleHeight - 30 - (this.buttonList.Count * 30);
        ErrorGuiButton guiButton = new ErrorGuiButton(new Rect(xPos, yPos, 100, 25), buttonText, ButtonFunction);
        guiButton.ButtonFunction += this.Hide;
        this.buttonList.Add(guiButton);
    }

    public void SetErrorMessage(string errorText)
    {
        this.errorText = errorText;
    }
}

public delegate void ErrorGuiButtonDelegate();

public struct ErrorGuiButton
{
    public Rect buttonRectangle;
    public string buttonText;
    public ErrorGuiButtonDelegate ButtonFunction;

    public ErrorGuiButton(Rect buttonRectangle, string buttonText, ErrorGuiButtonDelegate ButtonFunction)
    {
        this.buttonRectangle = buttonRectangle;
        this.buttonText = buttonText;
        this.ButtonFunction = ButtonFunction;
    }

    public void OnGUI(bool active)
    {
        if (GUI.Button(this.buttonRectangle, this.buttonText) && active && this.ButtonFunction != null)
            this.ButtonFunction();
    }
}
