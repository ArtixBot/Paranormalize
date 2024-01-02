using Godot;
using System;
using System.Collections.Generic;

public partial class SelectTargetPanel : Panel
{
	private bool _requiresText;
	public bool RequiresText {
		get {return _requiresText;}
		set {_requiresText = value; ChangeLabelText();}
	}
	private List<AbstractCharacter> _chars = new();
	public List<AbstractCharacter> Chars {
		get {return _chars;}
		set {_chars = value; ChangeSelectables();}
	}
	private List<int> _lanes = new();
	public List<int> Lanes {
		get {return _lanes;}
		set {_lanes = value; ChangeSelectables();}
	}
	public AbstractAbility ability;

	private Label selectName;
	private readonly PackedScene abilityButton = GD.Load<PackedScene>("res://Tactical/UI/Abilities/AbilityButton.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_label_ready(){
		selectName = GetNode<Label>("Label");
	}

	private void ChangeLabelText(){
		if (selectName == null) return;
		if (RequiresText){
			selectName.Text = "Select a unit";
		} else {
			selectName.Text = "Select a lane";
		}
	}

	private void ChangeSelectables(){
		// Only one of Chars/Lanes is ever filled.
		for (int i = 0; i < Lanes.Count; i++){
			AbilityButton instance = (AbilityButton) abilityButton.Instantiate();
			instance.SetPosition(new Vector2(0, 100 + i * instance.Size.Y));
			instance.Text = $"Lane {Lanes[i]}";
			List<int> lanes = new List<int>{Lanes[i]};		// For some reason I can't just pipe the new list in the InputAbility() fn?
			instance.Pressed += () => CombatManager.InputAbility(ability, lanes);
			instance.Pressed += DeletePanel;

			this.AddChild(instance);
		}
		for (int i = 0; i < Chars.Count; i++){
			AbilityButton instance = (AbilityButton) abilityButton.Instantiate();
			instance.SetPosition(new Vector2(0, 100 + i * instance.Size.Y));
			instance.Text = $"{Chars[i].CHAR_NAME}";
			List<AbstractCharacter> chars = new List<AbstractCharacter>{Chars[i]};
			instance.Pressed += () => CombatManager.InputAbility(ability, chars);
			instance.Pressed += DeletePanel;

			this.AddChild(instance);
		}
	}

	private void DeletePanel(){
		this.QueueFree();
	}
}
