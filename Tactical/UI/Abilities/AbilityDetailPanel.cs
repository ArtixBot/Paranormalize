using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;
using Localization;

namespace UI;

/// <summary>
/// Contains information about an ability.
/// Also spawns keyword panels based on description markers, e.g. {Status|STATUS_ID} or {Keyword|KEYWORD_ID} will spawn a description panel indicating what STATUS_ID does.
/// </summary>
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
		_abilityDesc = GetNode<RichTextLabel>("VBoxContainer/ScrollContainer/VBoxContainer/Ability Desc");
		abilityInfo = GetNode<RichTextLabel>("VBoxContainer/Ability Info");
		abilityName = GetNode<Label>("VBoxContainer/Ability Name");

		nodeToAddDiceTo = GetNode<Control>("VBoxContainer/ScrollContainer/VBoxContainer");
	}

	private void UpdateDescriptions(){
		abilityName.Text = _ability.NAME;
		string rangeText = (_ability.TYPE == AbilityType.REACTION) ? "" : $"\t\t[img=24]res://Sprites/range.png[/img] {_ability.MIN_RANGE} - {_ability.MAX_RANGE}";
		abilityInfo.Text = $"[font n='res://Assets/Jost-Medium.ttf' s=16]{_ability.TYPE}"  + $"\t\t[img=24]res://Sprites/cooldown.png[/img] {_ability.BASE_CD}" + rangeText;
		AbilityDesc = ParseCustomTags(_ability.STRINGS.GetValueOrDefault("GENERIC", ""));

		for (int i = 0; i < _ability.BASE_DICE.Count; i++){
            AbilityDie node = (AbilityDie) abilityDie.Instantiate();
			nodeToAddDiceTo.AddChild(node);

			node.Die = _ability.BASE_DICE[i];
			node.DieDesc = ParseCustomTags(_ability.STRINGS.GetValueOrDefault(node.Die.DieId, ""));
		}
	}

	private string ParseCustomTags(string s){
        MatchCollection matches = new Regex(@"(?<=\{)(.*?)(?=\})").Matches(s);
		for (int i = 0; i < matches.Count; i++){
            Match match = matches[i];
			// e.g.: Convert {Slash|9-13} to [color][b]Slash 9-13[/b][/color].
			// TODO: This presumably will not work well for internationalized strings.
            if (match.Value.Contains("Slash") || match.Value.Contains("Pierce") || match.Value.Contains("Blunt") || match.Value.Contains("Eldritch")){
				string[] attackTypeAndRange = match.Value.Split("|");
				string attackType = attackTypeAndRange.First();
				string damageRange = attackTypeAndRange.Last();

				string replacementString = "[color=#FF4E50][b]" + attackType + " " + damageRange + "[/b][/color]";
                s = s.Replace("{" + match.Value + "}", replacementString);
            }
			// e.g.: Convert {Cond|On activate} to [color]On activate[/color].
			// TODO: This presumably will not work well for internationalized strings.
			else if (match.Value.Contains("Cond")){
				string textToHighlight = match.Value.Split("|").Last();
				string replacementString = "[color=#4cf]" + textToHighlight + "[/color]";
                s = s.Replace("{" + match.Value + "}", replacementString);
			}
			else if (match.Value.Contains("Keyword")){
                string textToHighlight = match.Value.Split("|").Last();
				string keywordString = LocalizationLibrary.Instance.GetKeywordStrings(textToHighlight).NAME;
				string replacementString = "[color=#ffcb70]" + keywordString + "[/color]";
				s = s.Replace("{" + match.Value + "}", replacementString);
			}
        }
		return s;
	}
}
