using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using Localization;

public partial class StatusTooltip : PanelContainer
{
	public RichTextLabel rtlNode;
    private List<AbstractStatusEffect> _effects;
	public List<AbstractStatusEffect> Effects {
        get {return _effects;}
		set {_effects = value; UpdateText(_effects);}
	}

    private readonly Dictionary<StatusEffectType, string> statusToColorMap = new(){
        {StatusEffectType.BUFF, "#4cf"},
        {StatusEffectType.CONDITION, "#ffae00"},
        {StatusEffectType.DEBUFF, "#f74040"}
    };

	private void UpdateText(List<AbstractStatusEffect> effects){
        if (effects == null || effects.Count == 0 || !IsInstanceValid(rtlNode)) return;
        string effectText = "";
        foreach (AbstractStatusEffect effect in effects){
            string parsedName = ParseTooltip(effect.NAME, effect);
            string parsedDesc = ParseTooltip(effect.DESC, effect);
            effectText += $"[color={statusToColorMap[effect.TYPE]}]{parsedName}[/color]\n{parsedDesc}\n\n";
        }
        rtlNode.Text = "[font n='res://Assets/Inter-Regular.ttf' s=16]" + effectText + "[/font]";
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		rtlNode = GetNode<RichTextLabel>("Text");
        this.Size = new Vector2(450, 500);
	}

	string ParseTooltip(string s, AbstractStatusEffect effect){
        MatchCollection matches = new Regex(@"(?<=\{)(.*?)(?=\})").Matches(s);
        string prefix = $"[color={statusToColorMap[effect.TYPE]}]";
        string suffix = "[/color]";
        for (int i = 0; i < matches.Count; i++){
            Match match = matches[i];
            
            if (match.Value.Contains("stacks")){
                s = s.Replace("{" + match.Value + "}", prefix + effect.STACKS + suffix);
            }
            else if (match.Value.Contains("owner")){
                s = s.Replace("{" + match.Value + "}", effect.OWNER.CHAR_NAME);
            } else {    // Check for custom field w/ reflection. E.g. staggered condition uses "UNSTAGGER_ROUND" in effects.json. Check for an equivalent in ConditionStaggered.
                string customValue = effect.GetType().GetField(match.Value).GetValue(effect).ToString();
                s = s.Replace("{" + match.Value + "}", customValue);
            }
        }
        return s;
    }
}
