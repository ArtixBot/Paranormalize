using Godot;
using System;

public partial class Lane : TextureRect
{
	[Export]
	public int position;

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
		if (@event is InputEventMouseButton mouseButton && !mouseButton.IsPressed()){
			GD.Print($"Clicked on Lane {this.position} at {@event}");
		}
    }
}
