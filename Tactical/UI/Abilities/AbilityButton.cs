using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class AbilityButton : Button
{
	[Signal]
	public delegate void AbilitySelectedEventHandler(AbilityButton button);

	private AbstractAbility _ability;
	public AbstractAbility Ability {
		get {return _ability;}
		set {_ability = value; UpdateDisplay();}
	}
	
	private TextureRect cdImageNode;
	private Label cdLabel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		cdImageNode = GetNode<TextureRect>("Cooldown Img");
		cdLabel = GetNode<Label>("Cooldown Img/Cooldown Label");
	}

    public override void _Pressed(){
        base._Pressed();
		// ANCHOR[id=emit-ability-selected]
		EmitSignal(nameof(AbilitySelected), this);
    }

    private void UpdateDisplay(){
		this.Text = Ability.NAME;
		this.Disabled = !Ability.IsActivatable || (Ability.TYPE == AbilityType.REACTION && CombatManager.combatInstance.combatState != CombatState.AWAITING_CLASH_INPUT);
		cdImageNode.Visible = !Ability.IsActivatable;
		cdLabel.Text = !Ability.IsActivatable ? Ability.curCooldown.ToString() : "";
	}
}
