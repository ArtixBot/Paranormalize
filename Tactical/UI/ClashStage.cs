using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public enum PoseEnum {
	UNKNOWN,
	IDLE,		// Used for allies that are receiving a buff from another ally.
	CLASH_WINDUP,
	SLASH_MELEE,
	PIERCE_MELEE,
	BLUNT_MELEE,
	ELDRITCH_MELEE,
	SLASH_RANGED,
	PIERCE_RANGED,
	BLUNT_RANGED,
	ELDRITCH_RANGED,
	EVADE,
	BLOCK,
	UTILITY_BUFF,
	UTILITY_DEBUFF,
	DAMAGED
}

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
	public Dictionary<Sprite2D, Queue<Texture2D>> spriteQueuedPoses = new();

	public RichTextLabel lhsAbilityTitle;
	public RichTextLabel rhsAbilityTitle;
	public Control lhsDice;
	public Control rhsDice;

	public float delayBetweenPoses;
	public readonly PackedScene clashStageDie = GD.Load<PackedScene>("res://Tactical/UI/Abilities/ClashStageDie.tscn");

	private Sprite2D initiatorSprite;
	private List<Sprite2D> targetSprites = new();

	private float UTILITY_ABILITY_STEP_DURATION = 2.0f;
	private float ABILITY_STEP_DURATION = 0.5f; 
	private string NO_POSE_FOUND_PATH = "res://Sprites/Characters/no pose found.png";

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
		this.initiatorSprite = initiatorSprite;
		this.spriteQueuedPoses[this.initiatorSprite] = new();
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
			this.targetSprites.Add(targetSprite);
			this.spriteQueuedPoses[targetSprite] = new();
            AddChild(targetSprite);
			targetSprite.Position = initiatorOnLeftHalf ? npcSpawn.Position - new Vector2(100 * i, 0) : playerSpawn.Position + new Vector2(100 * i, 0);
			targetSprite.FlipH = initiatorOnLeftHalf;
		}

		if (this.targetSprites.Count != this.targetData.Count){
			GD.PrintErr("Sprite count != target data.");
			QueueFree();
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
			// Logging info.
			Logging.Log($"Rendering step {curStep+1}:", Logging.LogLevel.INFO);
			initiatorQueuedDice.TryDequeue(out Die[] initiatorDice);
			targetQueuedDice.TryDequeue(out Die[] targetDice);
			if ((initiatorDice == null && targetDice == null) || (initiatorDice.Length == 0 && targetDice.Length == 0)){
				GD.Print("Attacker/defender dice queues empty, ending rendering.");
				this.QueueFree();
				return;
			}
			if (initiatorDice != null){
				string content = "";
				foreach (Die die in initiatorDice){
					content += $"{die.DieType} ({die.MinValue} - {die.MaxValue}) | ";
				}
				Logging.Log("\tInitiator dice: " + content, Logging.LogLevel.INFO);
			}
			if (targetDice != null){
				string content = "";
				foreach (Die die in targetDice){
					content += $"{die.DieType} ({die.MinValue} - {die.MaxValue}) | ";
				}
				Logging.Log("\tTarget dice: " + content, Logging.LogLevel.INFO);
			}

			RenderDice();
			RenderPoses();
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

		int initiatorValue = 0, defenderValue = 0;
		while (dieRollResultQueue.Count > 0 && dieRollResultQueue.First().step == curStep){
			(int _, CombatEventDieRolled dieRollResultEvent) = dieRollResultQueue.Dequeue();
			Logging.Log($"\tDie roll: {dieRollResultEvent.roller.CHAR_NAME} rolled a {dieRollResultEvent.rolledValue} on a(n) {dieRollResultEvent.die.DieType} die.", Logging.LogLevel.INFO);
			if (dieRollResultEvent.roller == initiator){
				initiatorValue = dieRollResultEvent.rolledValue;
			} else {
				defenderValue = dieRollResultEvent.rolledValue;
			}
		}
		
		if (initiatorValue == defenderValue) {
			// QueueAnimation(this.initiatorSprite,
			// 	this.tacticalSceneNode.characterToPoseMap.GetValueOrDefault(this.initiator).GetValueOrDefault("clash_windup")
			// 	?? GD.Load<Texture2D>(NO_POSE_FOUND_PATH));
			// for(int i = 0; i < targetData.Count; i++){
			// 	QueueAnimation(this.targetSprites.ElementAt(i),
			// 	this.tacticalSceneNode.characterToPoseMap.GetValueOrDefault(this.targetData.ElementAt(i)).GetValueOrDefault("clash_windup")
			// 	?? GD.Load<Texture2D>(NO_POSE_FOUND_PATH));
			// }
		} else {
			bool initiatorWins = initiatorValue > defenderValue;
			Sprite2D attackerSprite = initiatorWins ? this.initiatorSprite : this.targetSprites[0];
			// Defender sprite is a list in order to handle cases where initiator performed an AoE ability (a defender can never react with an AoE ability)
			List<Sprite2D> defenderSprite = initiatorWins ? this.targetSprites : new List<Sprite2D>{this.initiatorSprite};
			// QueueAnimation(attackerSprite,)
			foreach (Sprite2D sprite in defenderSprite){
				QueueAnimation(sprite, PoseEnum.DAMAGED);
			}
		}


		if (initiatorDiceData != null){
			for (int i = 0; i < initiatorDiceData.Length; i++){
				UI.AbilityDie dieNode = (UI.AbilityDie) clashStageDie.Instantiate();
				initiatorSide.AddChild(dieNode);
				dieNode.Position = new Vector2(i * 60 * initiatorDiceAddOnLeftSide, dieNode.Position.Y);
				dieNode.Die = initiatorDiceData[i];

				if (i == 0){
					dieNode.Scale = new Vector2(1.5f, 1.5f);
					dieNode.DieRollValue = initiatorValue;
					dieNode.DieRange.Visible = false;
					dieNode.DieImage.Modulate = new Color(0.5f, 0.5f, 0.5f);
				}
			}
		}

		if (defenderDiceData != null){
			for (int i = 0; i < defenderDiceData.Length; i++){
				UI.AbilityDie dieNode = (UI.AbilityDie) clashStageDie.Instantiate();
				defenderSide.AddChild(dieNode);
				dieNode.Position = new Vector2(i * 60 * -initiatorDiceAddOnLeftSide, dieNode.Position.Y);
				dieNode.Die = defenderDiceData[i];

				if (i == 0){
					dieNode.Scale = new Vector2(1.5f, 1.5f);
					dieNode.DieRollValue = defenderValue;
					dieNode.DieRange.Visible = false;
					dieNode.DieImage.Modulate = new Color(0.5f, 0.5f, 0.5f);
				}
			}
		}

		while (damageRenderQueue.Count > 0 && damageRenderQueue.First().step == curStep){
			(int _, CombatEventDamageTaken dmgEvent) = damageRenderQueue.Dequeue();
			Logging.Log($"\tCombat event: {dmgEvent.target.CHAR_NAME} took {(int)dmgEvent.damageTaken} damage (was Poise damage: {dmgEvent.isPoiseDamage}).", Logging.LogLevel.INFO);
		}
	}

	private void RenderPoses(){
		foreach ((Sprite2D sprite, Queue<Texture2D> textures) in spriteQueuedPoses){
			textures.TryDequeue(out Texture2D newTexture);

			_ = SpawnAfterimg(sprite, 0.15f);
			sprite.Texture = newTexture;
		}
	}

	public void QueueAnimation(Sprite2D sprite, PoseEnum spriteType){
		if (!this.spriteQueuedPoses.ContainsKey(sprite)){
			// Should never happen, since all sprites should be in the dictionary as part of Ready().
			GD.PushWarning("Queued animation for non-existent sprite!");
		}
		// this.spriteQueuedPoses[sprite].Enqueue(poseToSwapTo);
		
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
