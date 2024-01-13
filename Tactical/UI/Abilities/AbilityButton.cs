using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class AbilityButton : Button
{
	[Signal]
	public delegate void AbilitySelectedEventHandler(AbilityButton button);

	public bool clashBehavior;

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
		EmitSignal(nameof(AbilitySelected), this);		// LINK - Tactical\UI\ActiveCharInterfaceLayer.cs:51
    }

	public void SetEnabled(bool isEnabled){
		this.Disabled = !isEnabled;
	}

    private void UpdateDisplay(){
		this.Text = Ability.NAME;
		this.Disabled = !Ability.IsAvailable || (Ability.TYPE == AbilityType.REACTION && CombatManager.combatInstance.combatState != CombatState.AWAITING_CLASH_INPUT);
		cdImageNode.Visible = !Ability.IsAvailable;
		cdLabel.Text = !Ability.IsAvailable ? Ability.curCooldown.ToString() : "";
	}
}
