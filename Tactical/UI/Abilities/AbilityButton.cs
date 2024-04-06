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
	
	private ColorRect abilityIdColor;
	private TextureRect cdImageNode;
	private Label cdLabel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		abilityIdColor = GetNode<ColorRect>("ColorRect");
		cdImageNode = GetNode<TextureRect>("Cooldown Img");
		cdLabel = GetNode<Label>("Cooldown Img/Cooldown Label");
	}

    public override void _Pressed(){
        base._Pressed();
		// ANCHOR[id=emit-ability-selected]
		EmitSignal(nameof(AbilitySelected), this);
    }

    private void UpdateDisplay(){
		this.Text = " " + Ability.NAME;
		this.Disabled = !Ability.IsActivatable || (Ability.TYPE == AbilityType.REACTION && CombatManager.combatInstance.combatState != CombatState.AWAITING_CLASH_INPUT);
		cdImageNode.Visible = !Ability.IsActivatable;
		cdLabel.Text = !Ability.IsActivatable ? Ability.curCooldown.ToString() : "";

		switch (Ability.TYPE){
			case AbilityType.ATTACK:
				abilityIdColor.Color = new Color(!Disabled ? "#c42121" : "#c4212140");
				break;
			case AbilityType.REACTION:
				abilityIdColor.Color = new Color(!Disabled ? "#198ae6" : "#198ae640");
				break;
			case AbilityType.UTILITY:
				abilityIdColor.Color = new Color(!Disabled ? "#e6af19" : "#e6af1940");
				break;
			default:
				break;
		}
	}
}
