using Godot;
using System;
using System.Collections.Generic;

public partial class ClashStage : Control {

	public Sprite2D initiator;
	public List<Sprite2D> targets;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		initiator = GetNode<Sprite2D>("Initiator");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
