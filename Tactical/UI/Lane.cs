using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using UI;

public partial class Lane : Area2D
{
	[Signal]
	public delegate void LaneSelectedEventHandler(Lane lane);

	public int laneNum;
	public ObservableCollection<CharacterUI> characters = new();

	private bool _IsClickable;
	public bool IsClickable {
		get {return _IsClickable;}
		set {_IsClickable = value; ClickableArea.InputPickable = IsClickable;}		// LINK - Tactical\UI\GUIOrchestrator.cs:59
	}

	public Area2D ClickableArea;
	public override void _Ready() {
		ClickableArea = this;
		IsClickable = false;
		characters.CollectionChanged += BalanceLane;
	}

    private void BalanceLane(object sender, NotifyCollectionChangedEventArgs e)
    {
		if (e.Action == NotifyCollectionChangedAction.Add){
			foreach(CharacterUI node in e.NewItems){
				GD.Print("Added character node ", node.Character.CHAR_NAME, " to lane ", laneNum);
				_ = CharEnterLane(node);
			}
		}
		if (e.Action == NotifyCollectionChangedAction.Remove){
			foreach(CharacterUI node in e.OldItems){
				GD.Print("Removed character node ", node.Character.CHAR_NAME, " from lane ", laneNum);
				_ = CharLeaveLane(node);
			}
		}
		int playerCount = this.characters.Where(character => character.Character.CHAR_FACTION == CharacterFaction.PLAYER).Count();
		int neutralCount = this.characters.Where(character => character.Character.CHAR_FACTION == CharacterFaction.NEUTRAL).Count();
		int enemyCount = this.characters.Where(character => character.Character.CHAR_FACTION == CharacterFaction.ENEMY).Count();
		int totalCount = playerCount + neutralCount + enemyCount;
    }

	private async Task<bool> CharEnterLane(CharacterUI character){
		float currentTime = 0f;
		while (currentTime <= 0.25f){
			float normalizedTransparency = Math.Min((float)(currentTime / 0.25f), 1.0f);
			character.Modulate = new Color(character.Modulate, normalizedTransparency);

			await Task.Delay(1);
			currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
		}
		return true;
	}

	private async Task<bool> CharLeaveLane(CharacterUI character){
		// TODO: Unify this with ClashStage's SpawnAfterimg since it's effectively the exact same function.
		// Have a overarching "SpriteAnimator" node to track GetProcessDeltaTime?
		float currentTime = 0f;
        Sprite2D afterimgSprite = (Sprite2D)character.Sprite.Duplicate();
        this.AddChild(afterimgSprite);
		// Use vectors so we can use Godot's in-built Lerp function.
		Vector2 startModulateAlpha = new Vector2(1.0f, 0.0f);
		Vector2 endModulateAlpha = new Vector2(0f, 0.0f);
		Color afterimgColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		afterimgSprite.Modulate = afterimgColor;

		while (currentTime <= 0.25f){
			float normalized = Math.Min((float)(currentTime / 0.25f), 1.0f);
			afterimgColor.A = startModulateAlpha.Lerp(endModulateAlpha, normalized).X;
			afterimgSprite.Modulate = afterimgColor;

			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
		}
		afterimgSprite.QueueFree();
		return true;
	} 

    public void _on_input_event(Viewport viewport, InputEvent @event, int shape_idx){
        if (IsClickable && @event is InputEventMouseButton && @event.IsPressed() == false){
			EmitSignal(nameof(LaneSelected), this);
		}
    }

}
