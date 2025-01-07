using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public enum PoseEnum {
	UNKNOWN,
	IDLE,		// Used for allies that are receiving a buff from another ally.
	ICON,		// Used for turn icons.
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
	private bool showcasingUtility;		// True if the initiator ability is a utility ability (no clashing).

	public TacticalScene tacticalSceneNode;

	public Dictionary<Sprite2D, UI.CharacterUI> spriteToUiDataMap = new();

	public RichTextLabel lhsAbilityTitle;
	public RichTextLabel rhsAbilityTitle;
	public Control lhsDice;
	public Control rhsDice;

	public readonly PackedScene clashStageDie = GD.Load<PackedScene>("res://Tactical/UI/Abilities/ClashStageDie.tscn");

	private Sprite2D initiatorSprite;
	private List<Sprite2D> targetSprites = new();

	private float UTILITY_DURATION = 1.5f;
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
		showcasingUtility = initiatorAbility.TYPE == AbilityType.UTILITY;
		
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
			Texture = tacticalSceneNode.characterToPoseMap[initiator].GetValueOrDefault(PoseEnum.CLASH_WINDUP, GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"))
		};
		this.initiatorSprite = initiatorSprite;
		this.spriteToUiDataMap[this.initiatorSprite] = tacticalSceneNode.characterToNodeMap[initiator]; 

		AddChild(initiatorSprite);
		initiatorSprite.Position = initiatorOnLeftHalf ? playerSpawn.Position : npcSpawn.Position;
		initiatorSprite.FlipH = !initiatorOnLeftHalf;

		for(int i = 0; i < targetData.Count; i++){
			AbstractCharacter target = targetData[i];
			if (target == initiator) continue;
            Sprite2D targetSprite = new(){
                Texture = tacticalSceneNode.characterToPoseMap[target].GetValueOrDefault(PoseEnum.CLASH_WINDUP, GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"))
            };
			this.targetSprites.Add(targetSprite);
			this.spriteToUiDataMap[targetSprite] = tacticalSceneNode.characterToNodeMap[target];
            
			AddChild(targetSprite);
			targetSprite.Position = initiatorOnLeftHalf ? npcSpawn.Position - new Vector2(100 * i, 0) : playerSpawn.Position + new Vector2(100 * i, 0);
			targetSprite.FlipH = initiatorOnLeftHalf;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private float timeSinceLastPose = 0.0f;
	public override void _Process(double delta){
		timeSinceLastPose += (float) delta;
		// Utility ability rendering case (the simple one).
		if (showcasingUtility && timeSinceLastPose > UTILITY_DURATION){
			this.QueueFree();
			return;
		}

		// Attack/clash ability rendering case (the not simple one).
		if (timeSinceLastPose >= ABILITY_STEP_DURATION) {
			timeSinceLastPose = 0.0f;
			// Logging info.
			Logging.Log($"Rendering step {curStep+1}:", Logging.LogLevel.INFO);
			Die[] initiatorDice = initiatorQueuedDice.FirstOrDefault();
			Die[] targetDice = targetQueuedDice.FirstOrDefault();
			if ((initiatorDice == null && targetDice == null) || (initiatorDice?.Length == 0 && targetDice?.Length == 0)){
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

			RenderNextStep();
			curStep += 1;

			initiatorQueuedDice.TryDequeue(out _);
			targetQueuedDice.TryDequeue(out _);
		}
	}

	private static PoseEnum GetPoseFromDie(in Die die){
        return die?.DieType switch
        {
            DieType.SLASH => die.HasTag(DieTags.RANGED) ? PoseEnum.SLASH_RANGED : PoseEnum.SLASH_MELEE,
            DieType.PIERCE => die.HasTag(DieTags.RANGED) ? PoseEnum.PIERCE_RANGED : PoseEnum.PIERCE_MELEE,
            DieType.BLUNT => die.HasTag(DieTags.RANGED) ? PoseEnum.BLUNT_RANGED : PoseEnum.BLUNT_MELEE,
            DieType.ELDRITCH => die.HasTag(DieTags.RANGED) ? PoseEnum.ELDRITCH_RANGED : PoseEnum.ELDRITCH_MELEE,
            DieType.BLOCK => PoseEnum.BLOCK,
            DieType.EVADE => PoseEnum.EVADE,
            DieType.UNIQUE => PoseEnum.UNKNOWN,		// TODO: Figure out what to use here instead.
            _ => PoseEnum.UNKNOWN,
        };
    }

	private void RenderNextStep(){
		foreach (Node n in lhsDice.GetChildren() + rhsDice.GetChildren()){
			n.QueueFree();
		}

		Die[] initiatorDiceData = initiatorQueuedDice.ElementAtOrDefault(0);
		Die[] defenderDiceData = targetQueuedDice.ElementAtOrDefault(0);

		Control initiatorSide = initiatorOnLeftHalf ? lhsDice : rhsDice;
		Control defenderSide = initiatorOnLeftHalf ? rhsDice : lhsDice;
		int initiatorDiceAddOnLeftSide = initiatorOnLeftHalf ? -1 : 1;

		int initiatorValue = 0, defenderValue = 0;
		Die initiatorDie = null, defenderDie = null;
		while (dieRollResultQueue.Count > 0 && dieRollResultQueue.First().step == curStep){
			(int _, CombatEventDieRolled dieRollResultEvent) = dieRollResultQueue.Dequeue();
			Logging.Log($"\tDie roll: {dieRollResultEvent.roller.CHAR_NAME} rolled a {dieRollResultEvent.rolledValue} on a(n) {dieRollResultEvent.die.DieType} die.", Logging.LogLevel.INFO);
			if (dieRollResultEvent.roller == initiator){
				initiatorValue = dieRollResultEvent.rolledValue;
				initiatorDie = dieRollResultEvent.die;
			} else {
				defenderValue = dieRollResultEvent.rolledValue;
				defenderDie = dieRollResultEvent.die;
			}
		}
		
		if (initiatorValue == defenderValue) {
			ChangePose(this.initiatorSprite, GetPoseFromDie(initiatorDie));
			ChangePose(this.targetSprites.First(), GetPoseFromDie(defenderDie));
		} else {
			bool initiatorWins = initiatorValue > defenderValue;
			Die winnerDie = initiatorWins ? initiatorDie : defenderDie;
			Sprite2D winnerSprite = initiatorWins ? this.initiatorSprite : this.targetSprites[0];
			
			Die loserDie = initiatorWins ? defenderDie : initiatorDie;
			List<Sprite2D> loserSprites = initiatorWins ? this.targetSprites : new List<Sprite2D>{this.initiatorSprite};
			
			ChangePose(winnerSprite, GetPoseFromDie(winnerDie));
			foreach (Sprite2D sprite in loserSprites){
				if (winnerDie.IsAttackDie) {
					ChangePose(sprite, PoseEnum.DAMAGED);
				} else {
					ChangePose(sprite, GetPoseFromDie(loserDie));
				}
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

	private void ChangePose(Sprite2D sprite, PoseEnum poseToSwapTo){
		GD.Print($"Attempting to change pose for {sprite.Name} to {poseToSwapTo}.");
		_ = SpawnAfterimg(sprite, 0.15f);
		if (!this.spriteToUiDataMap[sprite].Poses.ContainsKey(poseToSwapTo)){
			GD.Print($"Cannot change pose ({poseToSwapTo} does not exist for sprite)");
			return;
		}
		sprite.Texture = this.spriteToUiDataMap[sprite].Poses[poseToSwapTo];		
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
