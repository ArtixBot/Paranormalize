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

	public Dictionary<AbstractCharacter, Sprite2D> dataToSpriteMap = new();
	public Dictionary<Sprite2D, List<Texture2D>> spriteQueuedPoses = new();
	public float delayBetweenPoses = 0.35f;
	private float timeSinceDelay;

	private bool isSetup = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		initiator = GetNode<Sprite2D>("Initiator");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
		timeSinceDelay += (float) delta;
		if (timeSinceDelay >= delayBetweenPoses) {
			timeSinceDelay = 0.0f;
			
			// Default to deleting once all animations have played out.
			bool stageCompleted = true; 
			foreach (KeyValuePair<Sprite2D, List<Texture2D>> spritePose in spriteQueuedPoses){
				if (spritePose.Value.Count == 0) continue;
				spritePose.Key.Texture = spritePose.Value[0];
				spritePose.Value.RemoveAt(0);
				stageCompleted = false;		// If an animation played, don't delete the animations yet.
			}
			if (stageCompleted) {
				CanvasLayer animationStage = (CanvasLayer) GetParent();
				animationStage.Visible = false;
				QueueFree();
			}
		}
	}

	public void SetupStage(){
		if (initiatorData == null) return;
		tacticalSceneNode = (TacticalScene) GetParent().GetParent();
		if (!IsInstanceValid(tacticalSceneNode)) return;

		dataToSpriteMap[initiatorData] = initiator;

		foreach(AbstractCharacter target in targetData){
			if (target == initiatorData) continue;
            Sprite2D targetSprite = new(){
                Texture = tacticalSceneNode.characterToPoseMap[target].GetValueOrDefault("preclash", GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"))
            };
            AddChild(targetSprite);
			targets.Add(targetSprite);
			dataToSpriteMap[target] = targetSprite;
		}

		initiator.Texture = tacticalSceneNode.characterToPoseMap[initiatorData].GetValueOrDefault("preclash", GD.Load<Texture2D>("res://Sprites/Characters/no pose found.png"));
		// If the majority of targets are to the right of the initiator, place the initiator on the left side; and vice-versa.
		// If initiator + all targets are in the same lane, place the initiator on the left if the initiator is a player, else on the right for an enemy.
		int initiatorPos = initiatorData.Position;
		double targetAvgPos = targetData.Select(_ => _.Position).Average();

		bool initiatorOnLeftHalf = initiatorPos < targetAvgPos || (initiatorPos == targetAvgPos && initiatorData.CHAR_FACTION == CharacterFaction.PLAYER);
		initiator.Position = initiatorOnLeftHalf ? new Vector2(510, 400) : new Vector2(1510, 400);
		initiator.FlipH = !initiatorOnLeftHalf;
		for (int i = 0; i < targets.Count; i++){
			targets[i].Position = initiatorOnLeftHalf ? new Vector2(1510 + (100 * i), 400) : new Vector2(510 + (100 * i), 400);
			targets[i].FlipH = initiatorOnLeftHalf;
		}

		isSetup = true;
	}

	public void QueueAnimation(AbstractCharacter character, string poseToSwapTo, float afterimgDur = 0.25f){
		Sprite2D charSprite = dataToSpriteMap[character];
		Texture2D oldPose = charSprite.Texture;			// TODO: Create afterimage effect using afterimgDur.
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
}
