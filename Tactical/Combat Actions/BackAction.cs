using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Godot;

public class BackAction : AbstractAction {

    private AbstractCharacter mover;
    private AbstractCharacter targetToMoveFrom;
    private int backDistance;

    public BackAction(AbstractCharacter mover, AbstractCharacter targetToMoveFrom, int backDistance){
        this.mover = mover;
        this.targetToMoveFrom = targetToMoveFrom;
        this.backDistance = backDistance;
    }

    public override void Execute(){
        // If targetToMoveFrom is null, BackAction always moves the unit left X lanes.
        if (this.targetToMoveFrom == null){
            CombatManager.eventManager.BroadcastEvent(
            new CombatEventUnitMoved(this.mover, 
                                     this.mover.Position, 
                                     backDistance, 
                                     isMoveLeft: true,
                                     isForcedMovement: false));
            this.mover.Position = Math.Max(this.mover.Position - this.backDistance, GameVariables.MIN_LANES);
            return;
        }
        if (this.mover == null) return;
        bool moveLeft = this.mover.Position < this.targetToMoveFrom.Position;

        CombatManager.eventManager.BroadcastEvent(
            new CombatEventUnitMoved(this.mover, 
                                     this.mover.Position, 
                                     backDistance, 
                                     isMoveLeft: moveLeft,
                                     isForcedMovement: false));

        // If mover and target are in the same lane, move towards the center.
        if (this.mover.Position == this.targetToMoveFrom.Position){
            if (this.targetToMoveFrom.Position <= 3){
                this.mover.Position = Math.Min(this.mover.Position + this.backDistance, GameVariables.MAX_LANES);
            } else {
                this.mover.Position = Math.Max(this.mover.Position - this.backDistance, GameVariables.MIN_LANES);
            }
            return;
        }

        if (moveLeft) {
            this.mover.Position = Math.Max(this.mover.Position - this.backDistance, GameVariables.MIN_LANES);
        } else {
            this.mover.Position = Math.Min(this.mover.Position + this.backDistance, GameVariables.MAX_LANES);
        }
        return;
    }
}