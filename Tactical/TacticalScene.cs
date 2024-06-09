using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI;

public partial class TacticalScene : Node2D,
									 IEventSubscriber,
									 IEventHandler<CombatEventAbilityActivated>,
									 IEventHandler<CombatEventDamageTaken>,
									 IEventHandler<CombatEventUnitMoved>,
									 IEventHandler<CombatEventCombatStart>,
									 IEventHandler<CombatEventCombatStateChanged>,
									 IEventHandler<CombatEventCharacterDeath>,
									 IEventHandler<CombatUiEventPostDieRolled>,
									 IEventHandler<CombatEventDieHit>,
									 IEventHandler<CombatEventDieBlocked>,
									 IEventHandler<CombatEventDieEvaded>,
									 IEventHandler<CombatEventClashTie>
{
	public readonly CombatInstance combatData;
	private readonly PackedScene charScene = GD.Load<PackedScene>("res://Tactical/UI/Characters/Character.tscn");
	private readonly PackedScene laneScene = GD.Load<PackedScene>("res://Tactical/UI/Lane.tscn");
	private readonly PackedScene clashScene = GD.Load<PackedScene>("res://Tactical/UI/ClashStage.tscn");

	public Dictionary<AbstractCharacter, CharacterUI> characterToNodeMap = new();
	public Dictionary<AbstractCharacter, Dictionary<string, Texture2D>> characterToPoseMap = new();
	public Dictionary<int, Lane> laneToNodeMap = new();

	private GUIOrchestrator GUIOrchestratorNode;

	// Render combat stages in sequence. This queue only has one scene in it when it's the player's turn,
	// but it can have multiple in a row if enemy actions are consecutive (e.g. an opponent activates multiple Utility abilities in a row;
	// or an opponent attacks a character multiple times with non-clashable attacks.)
	private Queue<ClashStage> queuedClashes = new();
	private ClashStage currentClash = null;
	private ClashStage clashToAnimate {
		get {
			return queuedClashes.LastOrDefault();
		}
	}

	private CanvasLayer animationStage;

	public TacticalScene(){
		// TODO: Remove, this is for debugging. Should be done on game end before loading into the scene.
		CombatManager.combatInstance = new CombatInstance(new TestScenario());
		combatData = CombatManager.combatInstance;
	}

	public override void _Ready(){
		GUIOrchestratorNode = GetNode<GUIOrchestrator>("HUD/GUI");
		animationStage = GetNode<CanvasLayer>("AnimationStage");

		InitSubscriptions();
		CombatManager.ChangeCombatState(CombatState.COMBAT_START);
	}

	private float timeSinceDelay;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		// Pick up next clash from queue if one exists.
		if (!IsInstanceValid(currentClash) && queuedClashes.Count() > 0){
			currentClash = queuedClashes.Dequeue();
			GD.Print($"New clash character: {currentClash.initiator.CHAR_NAME}");
			GD.Print($"New clash target: {currentClash.targetData.First().CHAR_NAME}");

			animationStage.AddChild(currentClash);
		}

		// if (IsInstanceValid(currentClash)){
		// 	timeSinceDelay += (float) delta;
		// 	if (timeSinceDelay >= 1.0f){
		// 		timeSinceDelay = 0.0f;
		// 		currentClash?.QueueFree();
		// 	}
		// }
	}

    public virtual void InitSubscriptions(){
		// Spawn lanes.
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_START, this, CombatEventPriority.UI);
		// Move units.
		CombatManager.eventManager.Subscribe(CombatEventType.ON_UNIT_MOVED, this, CombatEventPriority.UI);
		// Clash scene-related event handling.
		CombatManager.eventManager.Subscribe(CombatEventType.ON_DIE_CLASH, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_CHARACTER_DEATH, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_TAKE_DAMAGE, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_DIE_BLOCKED, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_DIE_EVADED, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_CLASH_TIE, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_STATE_CHANGE, this, CombatEventPriority.UI);

		CombatManager.eventManager.Subscribe(CombatEventType.UI_EVENT_POST_DIE_ROLLED, this, CombatEventPriority.UI);
    }


	public void HandleEvent(CombatEventCombatStart data){
		for (int i = GameVariables.MIN_LANES; i <= GameVariables.MAX_LANES; i++){
			Lane laneNode = (Lane) laneScene.Instantiate();
			laneNode.Name = $"Lane {i}";
			laneNode.laneNum = i;
			laneToNodeMap[i] = laneNode;
			if (i > GameVariables.MAX_LANES / 2){
				laneNode.GetNode<Sprite2D>("Sprite2D").FlipH = true;
			}

			laneNode.LaneSelected += (laneNode) => GUIOrchestratorNode._on_child_lane_selection(laneNode.laneNum);
			laneNode.Position = new Vector2(100 + (i - 1) * 300, 500);
			this.AddChild(laneNode);
		}
		
		HashSet<AbstractCharacter> fighters = CombatManager.combatInstance.fighters;
		foreach (AbstractCharacter fighter in fighters){
			CharacterUI charNode = (CharacterUI) charScene.Instantiate();
			charNode.Name = fighter.CHAR_NAME;
			charNode.Character = fighter;
			characterToNodeMap[fighter] = charNode;
			characterToPoseMap[fighter] = charNode.Poses;

			charNode.CharacterSelected += (charNode) => GUIOrchestratorNode._on_child_character_selection(charNode.Character);
			charNode.Position = new Vector2(150 + (charNode.Character.Position - 1) * 300, 500);
			this.AddChild(charNode);
		}
	}

	public async void HandleEvent(CombatEventUnitMoved data){
		CharacterUI charNode = characterToNodeMap.GetValueOrDefault(data.movedUnit);
		if (!IsInstanceValid(charNode)) return;

		int newLane = data.isMoveLeft ? data.originalLane - data.moveMagnitude : data.originalLane + data.moveMagnitude;

		Vector2 oldPos = charNode.Position;
		Vector2 newPos = new(150 + (newLane - 1) * 300, 500);

		float currentTime = 0f;
		// TODO: 0.5f is a static value used because Clash Stage currently has anims set to 0.5 seconds. Define this someplace better.
		// float? delay = CombatManager.combatInstance?.abilityItrCount * 0.5f;
		// await Task.Delay(TimeSpan.FromSeconds((double)delay));
		while (currentTime <= 0.25f){
            float normalized = Math.Min((float)(currentTime / 0.25f), 1.0f);
			charNode.Position = oldPos.Lerp(newPos, Lerpables.EaseOut(normalized, 5));

			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
        }
	}

	public void HandleEvent(CombatEventCombatStateChanged data){
		if (data.newState == CombatState.RESOLVE_ABILITIES){
			if (CombatManager.combatInstance == null) { return; }
			if (CombatManager.combatInstance?.activeAbility.TYPE == AbilityType.SPECIAL){ return; }
			if (!IsInstanceValid(animationStage)){ return; }

			ClashStage clashStage = (ClashStage) clashScene.Instantiate();

			clashStage.initiator = CombatManager.combatInstance.activeChar;
			queuedClashes.Enqueue(clashStage);
		}
	}

	public void HandleEvent(CombatEventAbilityActivated data){
		if (!IsInstanceValid(this.clashToAnimate)) {return;}		// Pass and Move don't queue a clash to animate.
		if (data.caster == this.clashToAnimate.initiator){
			this.clashToAnimate.initiatorAbility = data.abilityActivated;
			this.clashToAnimate.targetData = data.targets;
			if (data.abilityDice.Count != 0){
				this.clashToAnimate.initiatorQueuedDice.Enqueue(CombatManager.combatInstance.activeAbilityDice.ToArray());
			}
		} else if (data.caster != this.clashToAnimate.initiator){
			this.clashToAnimate.targetAbility = data.abilityActivated;
			this.clashToAnimate.targetQueuedDice.Enqueue(CombatManager.combatInstance.reactAbilityDice?.ToArray());
		}
	}

	public async void HandleEvent(CombatEventDamageTaken data){
		// TODO: 0.5f is a static value used because Clash Stage currently has anims set to 0.5 seconds. Define this someplace better.
		// float? delay = CombatManager.combatInstance?.abilityItrCount * 0.5f;
		// await Task.Delay(TimeSpan.FromSeconds((double)delay));

		// string colorPrefix = data.isPoiseDamage ? "[color=#ffba44]" : "[color=#ff4444]";

        // RichTextLabel damageNumber = new(){
		// 	BbcodeEnabled = true,
		// 	Size = new Vector2(100, 100),
		// 	Position = data.isPoiseDamage ? new Vector2(50, 50) : new Vector2(0, 0),
		// 	MouseFilter = Control.MouseFilterEnum.Ignore,
		// 	// Int-cast; DamageAction does an int cast before dealing final damage.
        //     Text = colorPrefix + $"[font n=Assets/RobotoSlab-VariableFont_wght.ttf s=64][outline_size=12][outline_color=black]{(int)data.damageTaken}[/outline_color][/outline_size][/font][/color]"
        // };
		// float lifetime = 0.5f;

		// ClashStage clashStage = (ClashStage) animationStage.GetNodeOrNull("Clash Stage");
        // if (IsInstanceValid(clashStage)){
		// 	Sprite2D hitUnit = clashStage.dataToSpriteMap[data.target.CHAR_NAME];
		// 	damageNumber.Position -= new Vector2(300, 300);		// Move damage number off the sprite itself.
		// 	hitUnit.AddChild(damageNumber);
		// } else {
		// 	AbstractCharacter damagedCharacter = data.target;
		// 	CharacterUI charNode = characterToNodeMap.GetValueOrDefault(damagedCharacter);
		// 	if (!IsInstanceValid(charNode)) return;
        // 	// A character can take damage outside the clash stage (e.g. status effects). If the clash stage isn't active, damage numbers go above the sprite instead.
		// 	charNode.AddChild(damageNumber);
		// }
		// await Task.Delay(TimeSpan.FromSeconds((double)lifetime));
		// damageNumber.QueueFree();
	}

	public void HandleEvent(CombatEventDieHit data){
		ClashStage clashStage = (ClashStage) animationStage.GetNodeOrNull("Clash Stage");
		if (!IsInstanceValid(clashStage)) return;

		AbstractCharacter hitter = data.hitter;
		AbstractCharacter hitUnit = data.hitUnit;

		// TODO: Randomize type of pierce/blunt/slash animation each time?
		switch (data.die.DieType){
			case DieType.PIERCE:
				clashStage.QueueAnimation(hitter, "pierce");
				break;
			case DieType.BLUNT:
				clashStage.QueueAnimation(hitter, "blunt");
				break;
			case DieType.SLASH:
				clashStage.QueueAnimation(hitter, "slash");
				break;
			default:
				clashStage.QueueAnimation(hitter, "slash");
				break;
		}
		clashStage.QueueAnimation(hitUnit, "damaged");
	}

	public void HandleEvent(CombatEventDieBlocked data){
		ClashStage clashStage = (ClashStage) animationStage.GetNodeOrNull("Clash Stage");
		if (!IsInstanceValid(clashStage)) return;

		AbstractCharacter hitter = data.hitter;
		AbstractCharacter hitUnit = data.hitUnit;

		clashStage.QueueAnimation(hitUnit, "stagger");
		clashStage.QueueAnimation(hitter, "block_anim");
	}

	public void HandleEvent(CombatEventDieEvaded data){
		ClashStage clashStage = (ClashStage) animationStage.GetNodeOrNull("Clash Stage");
		if (!IsInstanceValid(clashStage)) return;

		AbstractCharacter hitter = data.hitter;
		AbstractCharacter hitUnit = data.hitUnit;

		clashStage.QueueAnimation(hitUnit, "whiff");
		clashStage.QueueAnimation(hitter, "evade_anim");
	}

	public void HandleEvent(CombatEventClashTie data){
		ClashStage clashStage = (ClashStage) animationStage.GetNodeOrNull("Clash Stage");
		if (!IsInstanceValid(clashStage)) return;

		AbstractCharacter hitter = CombatManager.combatInstance?.activeChar;
		AbstractCharacter defender = CombatManager.combatInstance?.activeAbilityTargets.FirstOrDefault();
		if (hitter == null || defender == null) return;

		// TODO: Queue animation based on die type *other* than only preclash?
		clashStage.QueueAnimation(hitter, "preclash");
		clashStage.QueueAnimation(defender, "preclash");
	}

	public void HandleEvent(CombatUiEventPostDieRolled data){
		if (!IsInstanceValid(this.clashToAnimate)) {return;}
		// Add to render queues for clashStage. Apparently casting ToArray() prevents the items from becoming null when activeAbilityDice is null??
		if (CombatManager.combatInstance.activeAbilityDice != null){
			this.clashToAnimate.initiatorQueuedDice.Enqueue(CombatManager.combatInstance.activeAbilityDice.ToArray());
		}
		if (CombatManager.combatInstance.reactAbilityDice != null){
			this.clashToAnimate.targetQueuedDice.Enqueue(CombatManager.combatInstance.reactAbilityDice.ToArray());
		}
	}

	public void HandleEvent(CombatEventCharacterDeath data){
		CharacterUI charUI = characterToNodeMap.GetValueOrDefault(data.deadChar);
		if (charUI == null) return;

		this.RemoveChild(charUI);
		characterToNodeMap.Remove(data.deadChar);
	}
}
