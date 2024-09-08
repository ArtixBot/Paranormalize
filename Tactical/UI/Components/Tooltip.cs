using Godot;
using System;
using System.Collections.Generic;

public partial class Tooltip : PanelContainer
{
	public RichTextLabel rtlNode;
    private List<string> _strings = new();
    public List<string> Strings {
        get {return _strings;}
		set {_strings = value; UpdateText(_strings);}
	}

	private void UpdateText(List<string> strings){
        if (strings == null || !IsInstanceValid(rtlNode)) return;
        string tooltipText = "";
        foreach (string str in strings){
            tooltipText += $"{str}";
        }
        rtlNode.Text = tooltipText;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		rtlNode = GetNode<RichTextLabel>("Content");
	}
}
