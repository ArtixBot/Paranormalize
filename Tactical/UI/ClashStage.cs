using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ClashStage : Control {

	public AbstractCharacter initiatorData;
	public List<AbstractCharacter> targetData;

	public TacticalScene tacticalSceneNode;
	public Sprite2D initiator;
	public List<Sprite2D> targets = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		initiator = GetNode<Sprite2D>("Initiator");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetupStage(){
		if (initiatorData == null) return;
		tacticalSceneNode = (TacticalScene) GetParent().GetParent();
		if (!IsInstanceValid(tacticalSceneNode)) return;

		foreach(AbstractCharacter target in targetData){
			if (target == initiatorData) continue;
            Sprite2D targetSprite = new(){
                Texture = tacticalSceneNode.characterToPoseMap[target]["preclash"]
            };
            AddChild(targetSprite);
			targets.Add(targetSprite);
		}

		initiator.Texture = tacticalSceneNode.characterToPoseMap[initiatorData]["preclash"];
		// If the majority of targets are to the right of the initiator, place the initiator on the left side; and vice-versa.
		// If initiator + all targets are in the same lane, place the initiator on the left if the initiator is a player, else on the right for an enemy.
		int initiatorPos = initiatorData.Position;
		double targetAvgPos = targetData.Select(_ => _.Position).Average();

		if (initiatorPos < targetAvgPos || (initiatorPos == targetAvgPos && initiatorData.CHAR_FACTION == CharacterFaction.PLAYER)){
			initiator.Position = new Vector2(510, 400);
			initiator.FlipH = false;

			for (int i = 0; i < targets.Count; i++){
				targets[i].Position = new Vector2(1510 + (100 * i), 400);
				targets[i].FlipH = true;
			}
		} else {
			initiator.Position = new Vector2(1510, 400);
			initiator.FlipH = true;

			for (int i = 0; i < targets.Count; i++){
				targets[i].Position = new Vector2(510 + (100 * i), 400);
				targets[i].FlipH = false;
			}
		}
	}
}
