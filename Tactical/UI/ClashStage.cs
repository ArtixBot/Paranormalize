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

	public Dictionary<UI.CharacterUI, Sprite2D> uiToSpriteMap = new();			// This is filled in _Ready() of this class.
	public Dictionary<UI.CharacterUI, Queue<Texture2D>> uiQueuedPoses = new();	// This is filled in TacticalScene.cs' event listeners (which occurs earlier than _Ready() of this class)
	public Dictionary<Sprite2D, Queue<Texture2D>> spriteToQueuedPoses = new();

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
		// Player characters are always on the left side, and NPCs on the right side.
		Node2D playerSpawn = GetNode<Node2D>("Left Spawn Point");
		Node2D npcSpawn = GetNode<Node2D>("Right Spawn Point");

		lhsAbilityTitle = GetNode<RichTextLabel>("Clash BG/LHS Ability Title");
		rhsAbilityTitle = GetNode<RichTextLabel>("Clash BG/RHS Ability Title");
		lhsDice = GetNode<Control>("Clash BG/LHS Ability Dice");
		rhsDice = GetNode<Control>("Clash BG/RHS Ability Dice");

		curStep = 0;
		stepCount = Math.Max(initiatorQueuedDice.Count, targetQueuedDice.Count);
		// Utility abilities last slightly longer in rendering.
		delayBetweenPoses = (stepCount == 0) ? UTILITY_ABILITY_STEP_DURATION : ABILITY_STEP_DURATION;
		
		initiatorOnLeftHalf = initiator.CHAR_FACTION == CharacterFaction.PLAYER;
		RichTextLabel initiatorAbilityTitle = initiatorOnLeftHalf ? lhsAbilityTitle : rhsAbilityTitle;
		RichTextLabel reactorAbilityTitle = initiatorOnLeftHalf ? rhsAbilityTitle : lhsAbilityTitle;

		initiatorAbilityTitle.Text = initiatorOnLeftHalf ? "[right]" + initiatorAbility.NAME : initiatorAbility.NAME;
		if (targetAbility == null){
			reactorAbilityTitle.Text = "";
		} else {
			reactorAbilityTitle.Text = initiatorOnLeftHalf ? targetAbility.NAME : "[right]" + targetAbility.NAME;
		}

		// Initialize sprites + positions.
		tacticalSceneNode = (TacticalScene) GetParent().GetParent();
		Sprite2D initiatorSprite = new(){
			Texture = tacticalSceneNode.characterToPoseMap[initiator].GetValueOrDefault("clash_windup", GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"))
		};
		uiToSpriteMap[tacticalSceneNode.characterToNodeMap[initiator]] = initiatorSprite;
		AddChild(initiatorSprite);
		initiatorSprite.Position = initiatorOnLeftHalf ? playerSpawn.Position : npcSpawn.Position;
		initiatorSprite.FlipH = !initiatorOnLeftHalf;

		for(int i = 0; i < targetData.Count; i++){
			AbstractCharacter target = targetData[i];
			if (target == initiator) continue;
            Sprite2D targetSprite = new(){
                Texture = tacticalSceneNode.characterToPoseMap[target].GetValueOrDefault("clash_windup", GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"))
            };
			uiToSpriteMap[tacticalSceneNode.characterToNodeMap[target]] = targetSprite;
            AddChild(targetSprite);
			targetSprite.Position = initiatorOnLeftHalf ? npcSpawn.Position - new Vector2(100 * i, 0) : playerSpawn.Position + new Vector2(100 * i, 0);
			targetSprite.FlipH = initiatorOnLeftHalf;
		}

		foreach (UI.CharacterUI ui in uiToSpriteMap.Keys){
			Sprite2D sprite = uiToSpriteMap[ui];
			spriteToQueuedPoses[sprite] = uiQueuedPoses[ui];
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
				Logging.Log($"\tDie roll: {dieRollResultEvent.roller.CHAR_NAME} rolled a {dieRollResultEvent.rolledValue} on a(n) {dieRollResultEvent.die.DieType} die.", Logging.LogLevel.INFO);
			}
			while (damageRenderQueue.Count > 0 && damageRenderQueue.First().step == curStep){
				(int _, CombatEventDamageTaken dmgEvent) = damageRenderQueue.Dequeue();
				Logging.Log($"\tCombat event: {dmgEvent.target.CHAR_NAME} took {(int)dmgEvent.damageTaken} damage (was Poise damage: {dmgEvent.isPoiseDamage}).", Logging.LogLevel.INFO);
			}
			curStep += 1;
		}
	}

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
		foreach ((Sprite2D sprite, Queue<Texture2D> textures) in spriteToQueuedPoses){
			textures.TryDequeue(out Texture2D newTexture);

			_ = SpawnAfterimg(sprite, 0.15f);
			sprite.Texture = newTexture;
		}
	}

	public void QueueAnimation(UI.CharacterUI ui, Texture2D poseToSwapTo){
		if (!uiQueuedPoses.ContainsKey(ui)){
			uiQueuedPoses[ui] = new Queue<Texture2D>();
		}
		uiQueuedPoses[ui].Enqueue(poseToSwapTo);
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
