using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI;

public partial class TacticalScene : Node2D, IEventSubscriber, IEventHandler<CombatEventCombatStart>, IEventHandler<CombatEventCombatStateChanged>, IEventHandler<CombatEventCharacterDeath>, IEventHandler<CombatEventAbilityActivated>
{
	public readonly CombatInstance combatData;
	private readonly PackedScene charScene = GD.Load<PackedScene>("res://Tactical/UI/Characters/Character.tscn");
	private readonly PackedScene laneScene = GD.Load<PackedScene>("res://Tactical/UI/Lane.tscn");

	public Dictionary<AbstractCharacter, CharacterUI> characterToNodeMap = new();
	public Dictionary<int, Lane> laneToNodeMap = new();

	private GUIOrchestrator GUIOrchestratorNode;

	private CanvasLayer animationStage;
	private ClashStage clashStage;

	public TacticalScene(){
		// TODO: Remove, this is for debugging. Should be done on game end before loading into the scene.
		CombatManager.combatInstance = new CombatInstance(new TestScenario());
		combatData = CombatManager.combatInstance;
	}

	public override void _Ready(){
		GUIOrchestratorNode = GetNode<GUIOrchestrator>("HUD/GUI");
		animationStage = GetNode<CanvasLayer>("AnimationStage");
		clashStage = GetNode<ClashStage>("AnimationStage/Clash Stage");

		InitSubscriptions();
		CombatManager.ChangeCombatState(CombatState.COMBAT_START);
	}

	public virtual void InitSubscriptions(){
		CombatManager.eventManager.Subscribe(CombatEventType.ON_COMBAT_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_CHARACTER_DEATH, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_ROUND_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_TURN_START, this, CombatEventPriority.UI);
		CombatManager.eventManager.Subscribe(CombatEventType.ON_ABILITY_ACTIVATED, this, CombatEventPriority.UI);
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
			laneNode.Position = new Vector2((i - 1) * 300, 500);
			this.AddChild(laneNode);
		}
		
		HashSet<AbstractCharacter> fighters = CombatManager.combatInstance.fighters;
		foreach (AbstractCharacter fighter in fighters){
			CharacterUI charNode = (CharacterUI) charScene.Instantiate();
			charNode.Name = fighter.CHAR_NAME;
			charNode.Character = fighter;
			characterToNodeMap[fighter] = charNode;

			charNode.CharacterSelected += (charNode) => GUIOrchestratorNode._on_child_character_selection(charNode.Character);
			charNode.Position = new Vector2((fighter.Position - 1) * 325, 500);
			this.AddChild(charNode);
		}
	}

	public void HandleEvent(CombatEventCombatStateChanged data){
		foreach (CharacterUI charUI in characterToNodeMap.Values){
			charUI.Position = new Vector2((charUI.Character.Position - 1) * 325, 500);
		}
	}

	public void HandleEvent(CombatEventCharacterDeath data){
		CharacterUI charUI = characterToNodeMap.GetValueOrDefault(data.deadChar);
		if (charUI == null) return;

		this.RemoveChild(charUI);
		characterToNodeMap.Remove(data.deadChar);
	}

	// TODO: Define a UI event to handle this, since CombatEventAbilityActivated will activate twice during a clash.
	public async void HandleEvent(CombatEventAbilityActivated data){
		if (data.abilityActivated.TYPE == AbilityType.SPECIAL){ return; }
		if (!IsInstanceValid(animationStage) || !IsInstanceValid(clashStage)){ return; }
		animationStage.Visible = true;

		await RemoveStage(1f);
	}

	public async Task<bool> RemoveStage(float duration){
		float currentTime = 0f;
		while (currentTime <= duration){
			await Task.Delay(1);
            currentTime += (float)GetProcessDeltaTime();		// Not using PhysicsProcess since this is graphical effect only.
        }

		animationStage.Visible = false;
		return true;
	}
}
