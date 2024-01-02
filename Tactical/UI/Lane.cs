using Godot;
using System;

public partial class Lane : Control
{
	[Signal]
	public delegate void LaneSelectedEventHandler(Lane lane);

	[Export]
	public int position;

	private bool _IsClickable;
	public bool IsClickable {
		get {return _IsClickable;}
		set {_IsClickable = value; ClickableArea.InputPickable = IsClickable;}		// LINK - Tactical\UI\GUIOrchestrator.cs:59
	}

	public Area2D ClickableArea;
	public override void _Ready() {
		ClickableArea = GetNode<Area2D>("Area2D");
		IsClickable = false;
	}

    public void _on_area_2d_input_event(Viewport viewport, InputEvent @event, int shape_idx){
        if (IsClickable && @event is InputEventMouseButton && @event.IsPressed() == false){
			EmitSignal(nameof(LaneSelected), this);
		}
    }
}
