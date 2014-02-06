using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[Serializable]
[AddComponentMenu( "Daikon Forge/Examples/Game Menu/List Field" )]
public class DemoListField : DemoItemBase
{

	#region Public events 

	/// <summary>
	/// Raised whenever the SelectedIndex value is changed
	/// </summary>
	public event EventHandler SelectedIndexChanged;

	#endregion 

	#region Public fields

	public List<string> Items = new List<string>();
	public List<string> ItemData = new List<string>();

	public int SelectedIndex = 0;

	#endregion 

	#region Public properties 

	/// <summary>
	/// Returns the display text of the current selection
	/// </summary>
	public string SelectedItemText
	{
		get { return Items[ SelectedIndex ]; }
	}

	/// <summary>
	/// Returns the data value of the current selection
	/// </summary>
	public string SelectedItemData
	{
		get { return ItemData[ SelectedIndex ]; }
	}

	#endregion 

	#region Monobehavior events

	void Start()
	{
		showSelection();
		updateButtons();
	}

	#endregion 

	#region dfControl events 

	protected override void OnEnterFocus( dfControl sender, dfFocusEventArgs args )
	{
		base.OnEnterFocus( sender, args );
		updateButtons();
	}

	protected override void OnLeaveFocus( dfControl sender, dfFocusEventArgs args )
	{
		base.OnLeaveFocus( sender, args );
		updateButtons();
	}

	private void OnMouseWheel( dfControl sender, dfMouseEventArgs args )
	{

		if( args.Used )
			return;

		args.Use();

		if( args.WheelDelta > 0 )
			SelectPrevious();
		else
			SelectNext();

	}

	private void OnClick( dfControl sender, dfMouseEventArgs args )
	{

		var prevButton = this.Control.Find( "Prev Button" );
		var nextButton = this.Control.Find( "Next Button" );

		if( args.Source == prevButton )
		{
			SelectPrevious();
		}
		else if( args.Source == nextButton )
		{
			SelectNext();
		}

	}

	private void OnKeyDown( dfControl sender, dfKeyEventArgs args )
	{

		if( args.KeyCode == KeyCode.LeftArrow )
		{
			SelectPrevious();
		}
		else if( args.KeyCode == KeyCode.RightArrow )
		{
			SelectNext();
		}

	}

	#endregion 

	#region Private utlity methods 

	private void SelectPrevious()
	{
		selectIndex( Mathf.Max( 0, SelectedIndex - 1 ) );
	}

	private void SelectNext()
	{
		selectIndex( Mathf.Min( SelectedIndex + 1, Items.Count - 1 ) );
	}

	private void selectIndex( int index )
	{

		if( index != SelectedIndex )
		{

			SelectedIndex = index;

			showSelection();

			if( SelectedIndexChanged != null )
			{
				SelectedIndexChanged( this, EventArgs.Empty );
			}

			SendMessage( "OnSelectedItemChanged", this, SendMessageOptions.DontRequireReceiver );

		}

	}

	private void updateButtons()
	{

		var prevButton = this.Control.Find( "Prev Button" );
		if( prevButton != null )
		{
			prevButton.IsVisible = Control.ContainsFocus && SelectedIndex > 0;
		}

		var nextButton = this.Control.Find( "Next Button" );
		if( nextButton != null )
		{
			nextButton.IsVisible = Control.ContainsFocus && SelectedIndex < Items.Count - 1;
		}

	}

	private void showSelection()
	{

		var label = this.Control.Find( "Item" ) as dfLabel;
		if( label != null )
		{
			label.Text = SelectedIndex >= 0 && SelectedIndex <= Items.Count - 1 ? Items[ SelectedIndex ] : "";
		}

		updateButtons();

	}

	#endregion 

}
