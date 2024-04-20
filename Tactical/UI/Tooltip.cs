using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using Localization;

public partial class Tooltip : PanelContainer
{
	public RichTextLabel rtlNode;
    private List<string> _strings;
    public List<string> Strings {
        get {return _strings;}
		set {_strings = value; UpdateText(_strings);}
	}

	private void UpdateText(List<string> strings){
        if (strings == null || !IsInstanceValid(rtlNode)) return;
        string tooltipText = "";
        foreach (String str in strings){
            tooltipText += $"{str}\n\n";
        }
        rtlNode.Text = "[font n='res://Assets/Inter-Regular.ttf' s=16]" + tooltipText + "[/font]";
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		rtlNode = GetNode<RichTextLabel>("Text");
        this.Size = new Vector2(450, 500);
	}
}
