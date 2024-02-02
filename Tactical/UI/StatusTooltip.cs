using Godot;
using System;
using System.Text.RegularExpressions;

// Needs to be attached to Tooltip.tscn.
public partial class StatusTooltip : PanelContainer
{
	public RichTextLabel rtlNode;
	public AbstractStatusEffect effect {
		set {UpdateText(value);}
	}

	private void UpdateText(AbstractStatusEffect value){
		if (IsInstanceValid(rtlNode)){
			rtlNode.Text = value.DESC;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		rtlNode = GetNode<RichTextLabel>("Text");
	}

	string ParseTooltip(string s){
        MatchCollection matches = new Regex(@"(?<=\{)(.*?)(?=\})").Matches(s);
        for (int i = 0; i < matches.Count; i++){
            Match match = matches[i];
            
            // Handle [STACKS], [STACKS*X] and [STACKS/X]. [STACKS*3], for example, will convert 1 stack to a value of 3.
            if (match.Value.Contains("stacks")){
                // Regex re = new Regex(@"\d+");
                // string prefix = "<style=\"Scalable\">";
                // string suffix = "</style>";
                // Match stackMultiplier = re.Match(match.Value);
                // if (stackMultiplier.Success){   
                //     int mult = int.Parse(stackMultiplier.Value);
                //     s = match.Value.Contains("*") ? s.Replace(match.Value, prefix + (argRef.stacks * mult).ToString() + suffix) : s.Replace(match.Value, prefix + (argRef.stacks / mult).ToString() + suffix) ;
                // } else {
                //     s = s.Replace(match.Value, prefix + (argRef.stacks).ToString() + suffix);
                // }
            }
            else if (match.Value.Contains("owner")){
                // s = s.Replace(match.Value, argRef.OWNER.NAME);
            } 
        }
        return s;
    }
}
