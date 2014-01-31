using UnityEngine;
using System.Collections;

public class DriverInput : MonoBehaviour
{

	enum TouchDirection{
		LEFT,RIGHT,UP,NONE
	}

    public float swipeSpeed = 1500.0f;     // Speed of touch input swipe
    public PlayerAction playerAction;
    private int fingerId = -1;      // Id of first finger touching screen
	private TouchDirection lastDirection;
	
	// Update is called once per frame
	void Update ()
    {
#if UNITY_STANDALONE || UNITY_EDITOR

        if (this.playerAction == PlayerAction.None)
        {
            if (Input.GetButtonDown("Left"))
            {
                this.playerAction = PlayerAction.TurnLeft;
            }
            else if (Input.GetButtonDown("Right"))
            {
                this.playerAction = PlayerAction.TurnRight;
            }
            else if (Input.GetButtonDown("Item"))
            {
                this.playerAction = PlayerAction.UseItem;
            }
            else if (Input.GetButtonDown("Cancel"))
            {
                Application.Quit();
            }
        }

#endif

        foreach (Touch touch in Input.touches)
        {
            if (this.fingerId >= 0 && touch.fingerId != this.fingerId)
                continue;

            if (touch.phase == TouchPhase.Moved && 
			    	touch.deltaPosition.magnitude / touch.deltaTime >= this.swipeSpeed /*&& 
			    	this.fingerId != touch.fingerId*/)
            {
				TouchDirection direction = this.getTouchDirection(touch);
				if (null == this.lastDirection ||
				    !this.lastDirection.Equals(direction)){
					this.lastDirection = direction;

					if (direction.Equals(TouchDirection.LEFT)){
						this.playerAction = PlayerAction.TurnLeft;
					} else if (direction.Equals(TouchDirection.RIGHT)){
						this.playerAction = PlayerAction.TurnRight;
					} else if (direction.Equals(TouchDirection.UP)){
						this.playerAction = PlayerAction.UseItem;
					}
				}

                /*if (direction > -1.0f && direction < 1.0f)
                {
                    if (touch.deltaPosition.x < 0.0f)
                        this.playerAction = PlayerAction.TurnLeft;
                    else
                        this.playerAction = PlayerAction.TurnRight;
                }
                else if (touch.deltaPosition.y > 0.0f)
                    this.playerAction = PlayerAction.UseItem;
				*/
                this.fingerId = touch.fingerId;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled && this.fingerId != touch.fingerId)
            {
                this.fingerId = -1;
				this.lastDirection = TouchDirection.NONE;
            }
        }
	}

	TouchDirection getTouchDirection(Touch touch){
		TouchDirection direction = TouchDirection.NONE;
		if (touch.deltaPosition.x != 0.0f){
			float dirValue = touch.deltaPosition.y / touch.deltaPosition.x;

			Debug.Log("-- "+touch.deltaPosition.x+" --");

			if (dirValue > -1.0f && dirValue < 1.0f){
				if (touch.deltaPosition.x < 0.0f)
					direction = TouchDirection.LEFT;
				else
					direction = TouchDirection.RIGHT;
			} else if (touch.deltaPosition.y > 0.0f){
				direction = TouchDirection.UP;
			}
		}
		return direction;
	}

}



// Defines player action
public enum PlayerAction
{
    None,
    TurnLeft,
    TurnRight,
    UseItem
}