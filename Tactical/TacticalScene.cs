using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI;

public partial class TacticalScene : Node2D, IEventSubscriber, IEventHandler<CombatEventCombatStart>, IEventHandler<CombatEventCombatStateChanged>, IEventHandler<CombatEventCharacterDeath>, IEventHandler<CombatEventDieHit>, IEventHandler<CombatEventClashTie>
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
			this.AddChild(charNode);
		}
	}

	public void HandleEvent(CombatEventCombatStateChanged data){
		foreach (CharacterUI charUI in characterToNodeMap.Values){
			charUI.Position = new Vector2(150 + (charUI.Character.Position - 1) * 300, 500);
		}

		if (data.newState == CombatState.RESOLVE_ABILITIES){
			if (CombatManager.combatInstance?.activeAbility.TYPE == AbilityType.SPECIAL){ return; }
			if (!IsInstanceValid(animationStage)){ return; }

			ClashStage clashStage = (ClashStage) clashScene.Instantiate();
			clashStage.Name = "Clash Stage";
			animationStage.AddChild(clashStage);

			clashStage.initiatorData = CombatManager.combatInstance?.activeChar;
			clashStage.targetData = CombatManager.combatInstance?.activeAbilityTargets;
			clashStage.SetupStage();
			animationStage.Visible = true;
		}
	}

	public void HandleEvent(CombatEventDieHit data){
		if (!IsInstanceValid(animationStage.GetNode("Clash Stage"))) return;
		AbstractCharacter hitter = data.hitter;
		ClashStage clashStage = (ClashStage) animationStage.GetNode("Clash Stage");

		clashStage.QueueAnimation(hitter, "slash");
	}

	public void HandleEvent(CombatEventClashTie data){
		if (!IsInstanceValid(animationStage.GetNode("Clash Stage"))) return;
		AbstractCharacter hitter = CombatManager.combatInstance?.activeChar;
		if (hitter == null) return;
		ClashStage clashStage = (ClashStage) animationStage.GetNode("Clash Stage");

		clashStage.QueueAnimation(hitter, "preclash");
	}

	public void HandleEvent(CombatEventCharacterDeath data){
		CharacterUI charUI = characterToNodeMap.GetValueOrDefault(data.deadChar);
		if (charUI == null) return;

		this.RemoveChild(charUI);
		characterToNodeMap.Remove(data.deadChar);
	}
}
