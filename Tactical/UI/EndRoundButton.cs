using Godot;
using System;

public partial class EndRoundButton : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		this.Pressed += EndRound;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void EndRound(){
		CombatManager cm = GetNode<CombatManager>("/root/TacticalScene/CombatManager");
		cm?.ChangeCombatState(CombatState.ROUND_END);
	}
}
