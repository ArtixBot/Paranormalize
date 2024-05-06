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
	
	private readonly PackedScene abilityDie = GD.Load<PackedScene>("res://Tactical/UI/Components/AbilityDie.tscn");
	private Control nodeToAddDiceTo;
	private RichTextLabel abilityInfo;
	private Label abilityName;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		_abilityDesc = GetNode<RichTextLabel>("VBoxContainer/Ability Desc");
		abilityInfo = GetNode<RichTextLabel>("VBoxContainer/Ability Info");
		abilityName = GetNode<Label>("VBoxContainer/Ability Name");

		nodeToAddDiceTo = GetNode<Control>("VBoxContainer/ScrollContainer/VBoxContainer");
	}

	private void UpdateDescriptions(){
		abilityName.Text = _ability.NAME;
		string rangeText = (_ability.TYPE == AbilityType.REACTION) ? "" : $"\t\t[img=24]res://Sprites/range.png[/img] {_ability.MIN_RANGE} - {_ability.MAX_RANGE}";
		abilityInfo.Text = $"[font n='res://Assets/Jost-Medium.ttf' s=16]{_ability.TYPE}"  + $"\t\t[img=24]res://Sprites/cooldown.png[/img] {_ability.BASE_CD}" + rangeText;
		AbilityDesc = "[font n='res://Assets/Inter-Regular.ttf' s=16]" + _ability.STRINGS.GetValueOrDefault("GENERIC", "") + "[/font]";

		for (int i = 0; i < _ability.BASE_DICE.Count; i++){
            AbilityDie node = (AbilityDie) abilityDie.Instantiate();
			nodeToAddDiceTo.AddChild(node);

			node.Die = _ability.BASE_DICE[i];
			node.DieDesc =  _ability.STRINGS.GetValueOrDefault(node.Die.DieId, "");
		}
	}
}
