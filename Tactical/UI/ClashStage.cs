using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class ClashStage : Control {

	public AbstractCharacter initiatorData;
	public Die[] initiatorDiceData;
	public List<AbstractCharacter> targetData;
	public Die[] defenderDiceData;

	public TacticalScene tacticalSceneNode;

	public Sprite2D initiator;
	public List<Sprite2D> targets = new();
	public Dictionary<string, Sprite2D> dataToSpriteMap = new();
	public Dictionary<Sprite2D, List<Texture2D>> spriteQueuedPoses = new();
	public List<Die[]> initiatorQueuedDice = new();
	public List<Die[]> defenderQueuedDice = new();

	public RichTextLabel lhsAbilityTitle;
	public RichTextLabel rhsAbilityTitle;
	public Control lhsDice;
	public Control rhsDice;

	public readonly PackedScene clashStageDie = GD.Load<PackedScene>("res://Tactical/UI/Abilities/ClashStageDie.tscn");

	public float delayBetweenPoses = 0.5f;
	private float timeSinceDelay;

	private bool initiatorOnLeftHalf;
	private bool isSetup = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		initiator = GetNode<Sprite2D>("Initiator");
		lhsAbilityTitle = GetNode<RichTextLabel>("Clash BG/LHS Ability Title");
		rhsAbilityTitle = GetNode<RichTextLabel>("Clash BG/RHS Ability Title");
		lhsDice = GetNode<Control>("Clash BG/LHS Ability Dice");
		rhsDice = GetNode<Control>("Clash BG/RHS Ability Dice");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override async void _Process(double delta){
		timeSinceDelay += (float) delta;
		if (timeSinceDelay >= delayBetweenPoses) {
			timeSinceDelay = 0.0f;
			
			// Default to deleting once all animations have played out.
			bool stageCompleted = true; 
			foreach (KeyValuePair<Sprite2D, List<Texture2D>> spritePose in spriteQueuedPoses){
				if (spritePose.Value.Count == 0) continue;
                _ = SpawnAfterimg(spritePose.Key, 0.25f);
				spritePose.Key.Texture = spritePose.Value[0];
				spritePose.Value.RemoveAt(0);
				stageCompleted = false;		// If an animation played, don't delete the animations yet.
			}
			if (initiatorQueuedDice.Count > 0){
				initiatorDiceData = initiatorQueuedDice[0];
				initiatorQueuedDice.RemoveAt(0);
			}
			if (defenderQueuedDice.Count > 0){
				defenderDiceData = defenderQueuedDice[0];
				defenderQueuedDice.RemoveAt(0);
			}
			RenderDice();
			if (stageCompleted) {
				initiatorQueuedDice.Clear();
				defenderQueuedDice.Clear();
				await Task.Delay(TimeSpan.FromSeconds(0.5f));		// linger effect
				try {
					CanvasLayer animationStage = (CanvasLayer) GetParent();
					animationStage.Visible = false;
					QueueFree();
				} catch (Exception){
					Logging.Log("Could not get parent / queue free (clash stage itself was likely removed)", Logging.LogLevel.DEBUG);
				}
			}
		}
	}

	public void SetupStage(){
		if (initiatorData == null) return;
		tacticalSceneNode = (TacticalScene) GetParent().GetParent();
		if (!IsInstanceValid(tacticalSceneNode)) return;

		dataToSpriteMap[initiatorData.CHAR_NAME] = initiator;

		foreach(AbstractCharacter target in targetData){
			if (target == initiatorData) continue;
            Sprite2D targetSprite = new(){
                Texture = tacticalSceneNode.characterToPoseMap[target].GetValueOrDefault("preclash", GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"))
            };
            AddChild(targetSprite);
			targets.Add(targetSprite);
			dataToSpriteMap[target.CHAR_NAME] = targetSprite;
		}

		initiator.Texture = tacticalSceneNode.characterToPoseMap[initiatorData].GetValueOrDefault("preclash", GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"));
		// If the majority of targets are to the right of the initiator, place the initiator on the left side; and vice-versa.
		// If initiator + all targets are in the same lane, place the initiator on the left if the initiator is a player, else on the right for an enemy.
		int initiatorPos = initiatorData.Position;
		double targetAvgPos = targetData.Select(_ => _.Position).Average();
		initiatorOnLeftHalf = initiatorPos < targetAvgPos || (initiatorPos == targetAvgPos && initiatorData.CHAR_FACTION == CharacterFaction.PLAYER);

		initiator.Position = initiatorOnLeftHalf ? new Vector2(510, 400) : new Vector2(1510, 400);
		initiator.FlipH = !initiatorOnLeftHalf;
		if (initiatorOnLeftHalf){
			lhsAbilityTitle.Text = "[right][font n=Assets/AlegreyaSans-Regular.ttf s=42]" + CombatManager.combatInstance?.activeAbility.NAME + "[/font][/right]";
			rhsAbilityTitle.Text = (CombatManager.combatInstance?.reactAbility != null) ? "[font n=Assets/AlegreyaSans-Regular.ttf s=42]" + CombatManager.combatInstance.reactAbility.NAME + "[/font]" : "";
		} else {
			rhsAbilityTitle.Text = "[font n=Assets/AlegreyaSans-Regular.ttf s=42]" + CombatManager.combatInstance?.activeAbility.NAME + "[/font]";
			lhsAbilityTitle.Text = (CombatManager.combatInstance?.reactAbility != null) ? "[right][font n=Assets/AlegreyaSans-Regular.ttf s=42]" + CombatManager.combatInstance.reactAbility.NAME + "[/font][/right]" : "";
		}
		for (int i = 0; i < targets.Count; i++){
			targets[i].Position = initiatorOnLeftHalf ? new Vector2(1510 + (100 * i), 400) : new Vector2(510 + (100 * i), 400);
			targets[i].FlipH = initiatorOnLeftHalf;
		}

		RenderDice();
		isSetup = true;
	}

	public void RenderDice(){
		foreach (Node n in lhsDice.GetChildren() + rhsDice.GetChildren()){
			n.QueueFree();
		}

		Control initiatorSide = initiatorOnLeftHalf ? lhsDice : rhsDice;
		Control defenderSide = initiatorOnLeftHalf ? rhsDice : lhsDice;
		int initiatorDiceAddOnLeftSide = initiatorOnLeftHalf ? -1 : 1;
		if (initiatorDiceData != null){
			for (int i = 0; i < initiatorDiceData.Length; i++){
				UI.AbilityDie dieNode = (UI.AbilityDie) clashStageDie.Instantiate();
				initiatorSide.AddChild(dieNode);
				if (i == 0){
					dieNode.Scale = new Vector2(1.3f, 1.3f);
				}
				dieNode.Position = new Vector2(i * 60 * initiatorDiceAddOnLeftSide, dieNode.Position.Y);
				dieNode.Die = initiatorDiceData[i];
			}
		}

		if (defenderDiceData != null){
			for (int i = 0; i < defenderDiceData.Length; i++){
				UI.AbilityDie dieNode = (UI.AbilityDie) clashStageDie.Instantiate();
				defenderSide.AddChild(dieNode);
				dieNode.Position = new Vector2(i * 60 * -initiatorDiceAddOnLeftSide, dieNode.Position.Y);
				dieNode.Die = defenderDiceData[i];
			}
		}
	}

	public void QueueAnimation(AbstractCharacter character, string poseToSwapTo){
		Sprite2D charSprite = dataToSpriteMap[character.CHAR_NAME];
		Texture2D newPose;
		try {
			newPose = tacticalSceneNode.characterToPoseMap[character][poseToSwapTo];
		} catch (Exception){
			newPose = GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png");
		}

		if (!spriteQueuedPoses.ContainsKey(charSprite)){
			spriteQueuedPoses[charSprite] = new List<Texture2D>();
		}
		spriteQueuedPoses[charSprite].Add(newPose);
	}

	private async Task<bool> SpawnAfterimg(Sprite2D parent, float duration){
		float currentTime = 0f;
        Sprite2D afterimgSprite = new(){
            Texture = parent.Texture,
            FlipH = parent.FlipH,
            ZIndex = parent.ZIndex - 1
        };
        parent.AddChild(afterimgSprite);
		// Use vectors so we can use Godot's in-built Lerp function.
		Vector2 startModulateAlpha = new Vector2(1.0f, 0.0f);
		Vector2 endModulateAlpha = new Vector2(0f, 0.0f);
		Color afterimgColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		afterimgSprite.Modulate = afterimgColor;

		while (currentTime <= duration){
			float normalized = Math.Min((float)(currentTime / duration), 1.0f);
			afterimgColor.A = startModulateAlpha.Lerp(endModulateAlpha, normalized).X;
			afterimgSprite.Modulate = afterimgColor;

			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
		}
		afterimgSprite.QueueFree();
		return true;
	}
}
