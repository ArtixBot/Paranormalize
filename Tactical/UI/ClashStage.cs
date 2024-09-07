using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class ClashStage : Control {

	public AbstractCharacter initiator;
	public AbstractAbility initiatorAbility;
	public Queue<Die[]> initiatorQueuedDice = new();
	private bool initiatorOnLeftHalf;

	public List<AbstractCharacter> targetData;
	public AbstractAbility targetAbility;
	public Queue<Die[]> targetQueuedDice = new();

	public Queue<(int step, CombatEventDieRolled dieRolled)> dieRollResultQueue = new();
	public Queue<(int step, CombatEventDamageTaken damageTaken)> damageRenderQueue = new();

	private int curStep;
	private int stepCount;		// The clash stage has X steps, and renders different stuff at each step.
								// Note that stepCount can be zero (e.g. a utility ability is activated), in which case the animation is pretty short.

	public TacticalScene tacticalSceneNode;

	public Sprite2D initatorSprite;
	public List<Sprite2D> targetSprites = new();
	public Dictionary<string, Sprite2D> dataToSpriteMap = new();
	public Dictionary<Sprite2D, List<Texture2D>> spriteQueuedPoses = new();

	public RichTextLabel lhsAbilityTitle;
	public RichTextLabel rhsAbilityTitle;
	public Control lhsDice;
	public Control rhsDice;

	public float delayBetweenPoses;
	public readonly PackedScene clashStageDie = GD.Load<PackedScene>("res://Tactical/UI/Abilities/ClashStageDie.tscn");

	private float UTILITY_ABILITY_STEP_DURATION = 2.0f;
	private float ABILITY_STEP_DURATION = 0.5f; 
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		initatorSprite = GetNode<Sprite2D>("Initiator");
		lhsAbilityTitle = GetNode<RichTextLabel>("Clash BG/LHS Ability Title");
		rhsAbilityTitle = GetNode<RichTextLabel>("Clash BG/RHS Ability Title");
		lhsDice = GetNode<Control>("Clash BG/LHS Ability Dice");
		rhsDice = GetNode<Control>("Clash BG/RHS Ability Dice");

		curStep = 0;
		stepCount = Math.Max(initiatorQueuedDice.Count, targetQueuedDice.Count);
		// Utility abilities last slightly longer in rendering.
		delayBetweenPoses = (stepCount == 0) ? UTILITY_ABILITY_STEP_DURATION : ABILITY_STEP_DURATION;
		
		// If the majority of targetSprites are to the right of the initiator, place the initiator on the left side; and vice-versa.
		// If initiator + all targetSprites are in the same lane, place the initiator on the left if the initiator is a player, else on the right for an enemy.
		int initiatorPos = initiator.Position;
		double targetAvgPos = targetData.Select(_ => _.Position).Average();
		initiatorOnLeftHalf = initiatorPos < targetAvgPos || (initiatorPos == targetAvgPos && initiator.CHAR_FACTION == CharacterFaction.PLAYER);

		RichTextLabel initiatorAbilityTitle = initiatorOnLeftHalf ? lhsAbilityTitle : rhsAbilityTitle;
		RichTextLabel reactorAbilityTitle = initiatorOnLeftHalf ? rhsAbilityTitle : lhsAbilityTitle;

		initiatorAbilityTitle.Text = initiatorOnLeftHalf ? "[right]" + initiatorAbility.NAME : initiatorAbility.NAME;
		if (targetAbility == null){
			reactorAbilityTitle.Text = "";
		} else {
			reactorAbilityTitle.Text = initiatorOnLeftHalf ? targetAbility.NAME : "[right]" + targetAbility.NAME;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private float timeSinceDelay;
	public override void _Process(double delta){
		if (curStep > stepCount) {
			this.QueueFree();
			return;
		}
		timeSinceDelay += (float) delta;
		if ((curStep == 0 && delayBetweenPoses != UTILITY_ABILITY_STEP_DURATION) || timeSinceDelay >= delayBetweenPoses) {
			timeSinceDelay = 0.0f;
			RenderDice();
			RenderPoses();
			// Logging info.
			Logging.Log($"Rendering step {curStep+1}:", Logging.LogLevel.INFO);
			if (initiatorQueuedDice.TryDequeue(out Die[] initatorDice)){
				string content = "";
				foreach (Die die in initatorDice){
					content += $"{die.DieType} ({die.MinValue} - {die.MaxValue}) | ";
				}
				Logging.Log("\tInitiator dice: " + content, Logging.LogLevel.INFO);
			}
			if (targetQueuedDice.TryDequeue(out Die[] targetDice)){
				string content = "";
				foreach (Die die in targetDice){
					content += $"{die.DieType} ({die.MinValue} - {die.MaxValue}) | ";
				}
				Logging.Log("\tTarget dice: " + content, Logging.LogLevel.INFO);
			}
			while (dieRollResultQueue.Count > 0 && dieRollResultQueue.First().step == curStep){
				(int _, CombatEventDieRolled dieRollResultEvent) = dieRollResultQueue.Dequeue();
				Logging.Log($"\tDie roll: {dieRollResultEvent.roller.CHAR_NAME} rolled a {dieRollResultEvent.rolledValue} on a(n) {dieRollResultEvent.die.DieType.ToString()} die.", Logging.LogLevel.INFO);
			}
			while (damageRenderQueue.Count > 0 && damageRenderQueue.First().step == curStep){
				(int _, CombatEventDamageTaken dmgEvent) = damageRenderQueue.Dequeue();
				Logging.Log($"\tCombat event: {dmgEvent.target.CHAR_NAME} took {(int)dmgEvent.damageTaken} damage (was Poise damage: {dmgEvent.isPoiseDamage}).", Logging.LogLevel.INFO);
			}
			curStep += 1;
		}
		// 	// Default to deleting once all animations have played out.
		// 	bool stageCompleted = true; 
		// 	foreach (KeyValuePair<Sprite2D, List<Texture2D>> spritePose in spriteQueuedPoses){
		// 		if (spritePose.Value.Count == 0) continue;
        //         _ = SpawnAfterimg(spritePose.Key, 0.25f);
		// 		spritePose.Key.Texture = spritePose.Value[0];
		// 		spritePose.Value.RemoveAt(0);
		// 		stageCompleted = false;		// If an animation played, don't delete the animations yet.
		// 	}
		// 	if (stageCompleted) {
		// 		initiatorQueuedDice.Clear();
		// 		targetQueuedDice.Clear();
		// 		await Task.Delay(TimeSpan.FromSeconds(0.5f));		// linger effect
		// 		try {
		// 			CanvasLayer animationStage = (CanvasLayer) GetParent();
		// 			animationStage.Visible = false;
		// 			QueueFree();
		// 		} catch (Exception){
		// 			Logging.Log("Could not get parent / queue free (clash stage itself was likely removed)", Logging.LogLevel.DEBUG);
		// 		}
		// 	}
	}

	// public void SetupStage(){
		// if (initiator == null) return;
		// tacticalSceneNode = (TacticalScene) GetParent().GetParent();
		// if (!IsInstanceValid(tacticalSceneNode)) return;
// 
		// dataToSpriteMap[initiator.CHAR_NAME] = initatorSprite;
// 
		// foreach(AbstractCharacter target in targetData){
		// 	if (target == initiator) continue;
        //     Sprite2D targetSprite = new(){
        //         Texture = tacticalSceneNode.characterToPoseMap[target].GetValueOrDefault("preclash", GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"))
        //     };
        //     AddChild(targetSprite);
		// 	targetSprites.Add(targetSprite);
		// 	dataToSpriteMap[target.CHAR_NAME] = targetSprite;
		// }
// 
		// initatorSprite.Texture = tacticalSceneNode.characterToPoseMap[initiator].GetValueOrDefault("preclash", GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"));
		// If the majority of targetSprites are to the right of the initiator, place the initiator on the left side; and vice-versa.
		// If initiator + all targetSprites are in the same lane, place the initiator on the left if the initiator is a player, else on the right for an enemy.
		// int initiatorPos = initiator.Position;
		// double targetAvgPos = targetData.Select(_ => _.Position).Average();
		// initiatorOnLeftHalf = initiatorPos < targetAvgPos || (initiatorPos == targetAvgPos && initiator.CHAR_FACTION == CharacterFaction.PLAYER);
// 
		// initatorSprite.Position = initiatorOnLeftHalf ? new Vector2(510, 400) : new Vector2(1510, 400);
		// initatorSprite.FlipH = !initiatorOnLeftHalf;
		// if (initiatorOnLeftHalf){
		// 	lhsAbilityTitle.Text = "[right][font n=Assets/AlegreyaSans-Regular.ttf s=42]" + CombatManager.combatInstance?.activeAbility.NAME + "[/font][/right]";
		// 	rhsAbilityTitle.Text = (CombatManager.combatInstance?.reactAbility != null) ? "[font n=Assets/AlegreyaSans-Regular.ttf s=42]" + CombatManager.combatInstance.reactAbility.NAME + "[/font]" : "";
		// } else {
		// 	rhsAbilityTitle.Text = "[font n=Assets/AlegreyaSans-Regular.ttf s=42]" + CombatManager.combatInstance?.activeAbility.NAME + "[/font]";
		// 	lhsAbilityTitle.Text = (CombatManager.combatInstance?.reactAbility != null) ? "[right][font n=Assets/AlegreyaSans-Regular.ttf s=42]" + CombatManager.combatInstance.reactAbility.NAME + "[/font][/right]" : "";
		// }
		// for (int i = 0; i < targetSprites.Count; i++){
		// 	targetSprites[i].Position = initiatorOnLeftHalf ? new Vector2(1510 + (100 * i), 400) : new Vector2(510 + (100 * i), 400);
		// 	targetSprites[i].FlipH = initiatorOnLeftHalf;
		// }
// 
	// 	RenderDice();
	// }

	private void RenderDice(){
		foreach (Node n in lhsDice.GetChildren() + rhsDice.GetChildren()){
			n.QueueFree();
		}

		Die[] initiatorDiceData = initiatorQueuedDice.ElementAtOrDefault(0);
		Die[] defenderDiceData = targetQueuedDice.ElementAtOrDefault(0);

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
				if (i == 0){
					dieNode.Scale = new Vector2(1.3f, 1.3f);
				}
				dieNode.Position = new Vector2(i * 60 * -initiatorDiceAddOnLeftSide, dieNode.Position.Y);
				dieNode.Die = defenderDiceData[i];
			}
		}
	}

	private void RenderPoses(){

	}

	public void QueueAnimation(AbstractCharacter character, string poseToSwapTo){
		Sprite2D charSprite = dataToSpriteMap.GetValueOrDefault(character.CHAR_NAME);
		if (charSprite == default) return;
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
