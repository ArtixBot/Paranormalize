using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI;

public partial class TacticalScene : Node2D, IEventSubscriber, IEventHandler<CombatEventUnitMoved>, IEventHandler<CombatEventCombatStart>, IEventHandler<CombatEventCombatStateChanged>, IEventHandler<CombatEventCharacterDeath>, IEventHandler<CombatEventDieHit>, IEventHandler<CombatEventClashTie>
{
	public readonly CombatInstance combatData;
	private readonly PackedScene charScene = GD.Load<PackedScene>("res://Tactical/UI/Characters/Character.tscn");
	private readonly PackedScene laneScene = GD.Load<PackedScene>("res://Tactical/UI/Lane.tscn");
	private readonly PackedScene clashScene = GD.Load<PackedScene>("res://Tactical/UI/ClashStage.tscn");

	public Dictionary<AbstractCharacter, CharacterUI> characterToNodeMap = new();
	public Dictionary<AbstractCharacter, Dictionary<string, Texture2D>> characterToPoseMap = new();
	public Dictionary<int, Lane> laneToNodeMap = new();

	private GUIOrchestrator GUIOrchestratorNode;

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

	public virtual void InitSubscriptions(){
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_CHARACTER_DEATH, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_UNIT_MOVED, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_CLASH_TIE, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_STATE_CHANGE, this, CombatEventPriority.UI);
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

	// TODO: Since game effects are calculated immediately, this effect will render immediately even if a push/pull/forward/back is a later die.
	// In short, it renders faster than the dice roll and playout render effect will take place. Figure out a way to delay rendering.
	public async void HandleEvent(CombatEventUnitMoved data){
		CharacterUI charNode = characterToNodeMap[data.movedUnit];
		if (!IsInstanceValid(charNode)) return;

		int newLane = data.isMoveLeft ? data.originalLane - data.moveMagnitude : data.originalLane + data.moveMagnitude;

		Vector2 oldPos = charNode.Position;
		Vector2 newPos = new(150 + (newLane - 1) * 300, 500);

		float currentTime = 0f;
		while (currentTime <= 0.25f){
            float normalized = Math.Min((float)(currentTime / 0.25f), 1.0f);
			charNode.Position = oldPos.Lerp(newPos, Lerpables.EaseOut(normalized, 5));

			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
        }
	}

	public void HandleEvent(CombatEventCombatStateChanged data){
		// foreach (CharacterUI charUI in characterToNodeMap.Values){
		// 	charUI.Position = new Vector2(150 + (charUI.Character.Position - 1) * 300, 500);
		// }

		if (data.newState == CombatState.RESOLVE_ABILITIES){
			if (CombatManager.combatInstance == null) { return; }
			if (CombatManager.combatInstance?.activeAbility.TYPE == AbilityType.SPECIAL){ return; }
			if (!IsInstanceValid(animationStage)){ return; }

			ClashStage clashStage = (ClashStage) clashScene.Instantiate();
			clashStage.Name = "Clash Stage";
			animationStage.AddChild(clashStage);

			clashStage.initiatorData = CombatManager.combatInstance.activeChar;
			clashStage.initiatorDiceData = CombatManager.combatInstance.activeAbilityDice;
			clashStage.targetData = CombatManager.combatInstance.activeAbilityTargets;
			clashStage.defenderDiceData = CombatManager.combatInstance.reactAbilityDice;
			clashStage.SetupStage();
			animationStage.Visible = true;
		}
	}

	public void HandleEvent(CombatEventDieHit data){
		if (!IsInstanceValid(animationStage.GetNode("Clash Stage"))) return;
		ClashStage clashStage = (ClashStage) animationStage.GetNode("Clash Stage");
		AbstractCharacter hitter = data.hitter;
		AbstractCharacter hitUnit = data.hitUnit;

		// TODO: Queue animation based on die type *other* than only slash.
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

	public void HandleEvent(CombatEventClashTie data){
		if (!IsInstanceValid(animationStage.GetNode("Clash Stage"))) return;
		AbstractCharacter hitter = CombatManager.combatInstance?.activeChar;
		AbstractCharacter defender = CombatManager.combatInstance?.activeAbilityTargets.FirstOrDefault();
		if (hitter == null || defender == null) return;
		ClashStage clashStage = (ClashStage) animationStage.GetNode("Clash Stage");

		// TODO: Queue animation based on die type *other* than only preclash?
		clashStage.QueueAnimation(hitter, "preclash");
		clashStage.QueueAnimation(defender, "preclash");
	}

	public void HandleEvent(CombatEventCharacterDeath data){
		CharacterUI charUI = characterToNodeMap.GetValueOrDefault(data.deadChar);
		if (charUI == null) return;

		this.RemoveChild(charUI);
		characterToNodeMap.Remove(data.deadChar);
	}
}
