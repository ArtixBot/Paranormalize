using Godot;
using Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UI;

public partial class AbilityInfoPanel : HBoxContainer
{
	private AbstractAbility _ability;
	public AbstractAbility Ability {
		get {return _ability;}
		set {_ability = value; UpdateDisplay();}
	}

	private AbilityDetailPanel abilityDetailPanel;
	private Control tooltipContainer;
	private readonly PackedScene tooltipScene = GD.Load<PackedScene>("res://Tactical/UI/Components/Tooltip.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		abilityDetailPanel = GetNode<AbilityDetailPanel>("AbilityDetailPanel");
		tooltipContainer = GetNode<Control>("TooltipContainer");
		this.Ability = new HaveAtThee();
	}
	
	private async void UpdateDisplay(){
		abilityDetailPanel.Ability = _ability;
		List<string> tooltips = ParseForTooltips(_ability.STRINGS.GetValueOrDefault("GENERIC", ""));
		for (int i = 0; i < _ability.BASE_DICE.Count; i++){
			Die die = _ability.BASE_DICE[i];
			tooltips = tooltips.Concat(ParseForTooltips(_ability.STRINGS.GetValueOrDefault(die.DieId, ""))).ToList();
		}
		HashSet<string> tooltipsToDisplay = tooltips.ToHashSet();
		foreach (string ID in tooltipsToDisplay){
			KeywordStrings keywordStrings = LocalizationLibrary.Instance.GetKeywordStrings(ID);
			string name = keywordStrings.NAME, desc = keywordStrings.DESC;

			Tooltip tooltipNode = (Tooltip) tooltipScene.Instantiate();
			tooltipNode.Modulate = new Color(0, 0, 0, 0);
			string tooltipString = $"[b]{name}[/b]\n{desc}";
			tooltipContainer.AddChild(tooltipNode);
			tooltipNode.Strings = new List<string>{tooltipString};

			await Task.Delay(1);
			Lerpables.FadeIn(tooltipNode, 0.25f);
		}
	}

	private List<string> ParseForTooltips(string s){
		List<string> tooltips = new();
		MatchCollection matches = new Regex(@"(?<=\{)(.*?)(?=\})").Matches(s);
		for (int i = 0; i < matches.Count; i++){
			Match match = matches[i];
			// Spawn a Keyword panel explaining what the keyword (e.g. Final, Fixed Cooldown) does.
			if (match.Value.Contains("Keyword")){
				string keywordID = match.Value.Split("|").Last();
				tooltips.Add(keywordID);
			}
        }
		return tooltips;
	}
}
