using Godot;
using System;
using System.Collections.Generic;

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
        tooltipText = tooltipText.StripEdges();
        rtlNode.Text = "[font n='res://Assets/Inter-Regular.ttf' s=16]" + tooltipText + "[/font]";
        SetDeferred("size", new Vector2(450, 0));
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		rtlNode = GetNode<RichTextLabel>("Content");
	}
}
