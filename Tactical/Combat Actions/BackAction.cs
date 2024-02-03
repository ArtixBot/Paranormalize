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
        GD.Print($"{this.mover} and {this.targetToMoveFrom}");
        if (this.mover == null || this.targetToMoveFrom == null) return;
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