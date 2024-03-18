using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace UI;

public partial class AbilityDetailPanel : Control
{
	private AbstractAbility _ability;
	public AbstractAbility Ability {
		get {return _ability;}
		set {_ability = value; UpdateDescriptions();}
	}

	private RichTextLabel _abilityDesc;
	public string AbilityDesc {
		get {return _abilityDesc?.Text;}
		set {_abilityDesc.Text = value;}
	}
	
	private readonly PackedScene abilityDie = GD.Load<PackedScene>("res://Tactical/UI/Abilities/AbilityDie.tscn");

	private RichTextLabel abilityInfo;
	private Label abilityName;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		_abilityDesc = GetNode<RichTextLabel>("Ability Desc");
		abilityInfo = GetNode<RichTextLabel>("Ability Info");
		abilityName = GetNode<Label>("Ability Name");
	}

	private async void UpdateDescriptions(){
		abilityName.Text = _ability.NAME;
		string rangeText = (_ability.TYPE == AbilityType.REACTION) ? "" : $"\t\t[img=24]res://Sprites/range.png[/img] {_ability.MIN_RANGE} - {_ability.MAX_RANGE}";
		abilityInfo.Text = $"[font n='res://Assets/Jost-Medium.ttf' s=16]{_ability.TYPE}"  + $"\t\t[img=24]res://Sprites/cooldown.png[/img] {_ability.BASE_CD}" + rangeText;
		AbilityDesc = "[font n='res://Assets/Inter-Regular.ttf' s=16]" + _ability.STRINGS.GetValueOrDefault("GENERIC", "") + "[/font]";

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);	// RTL node does not update size until after delay.
		int offsetY = 100 + (int)_abilityDesc.Size.Y;	// Starts at +100 for earlier content (title, range/cooldown).

		for (int i = 0; i < _ability.BASE_DICE.Count; i++){
            AbilityDie node = (AbilityDie) abilityDie.Instantiate();
			AddChild(node);
			node.SetPosition(new Vector2(10, offsetY));

			node.Die = _ability.BASE_DICE[i];
			node.DieDesc = "[font n='res://Assets/Inter-Regular.ttf' s=16]" + _ability.STRINGS.GetValueOrDefault(node.Die.DieId, "") + "[/font]";

			// Auto-calculate based on line count since that doesn't invoke a GUI delay when DieDesc is updated.
			offsetY += (int)Math.Max(50, node._dieDesc.GetLineCount() * 20);
		}
	}
}
