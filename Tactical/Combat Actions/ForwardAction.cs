using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Godot;

public class ForwardAction : AbstractAction {

    private AbstractCharacter mover;
    private AbstractCharacter targetToMoveTo;
    private int forwardDistance;

    public ForwardAction(AbstractCharacter mover, AbstractCharacter targetToMoveTo, int forwardDistance){
        this.mover = mover;
        this.targetToMoveTo = targetToMoveTo;
        this.forwardDistance = forwardDistance;
    }

    public override void Execute(){
        // If targetToMoveTo is null, ForwardAction always moves the unit right X lanes.
        if (this.targetToMoveTo == null){
            CombatManager.eventManager.BroadcastEvent(
            new CombatEventUnitMoved(this.mover, 
                                     this.mover.Position, 
                                     forwardDistance, 
                                     isMoveLeft: false,
                                     isForcedMovement: false));
            this.mover.Position = Math.Min(this.mover.Position + this.forwardDistance, GameVariables.MAX_LANES);
            return;
        }
        if (this.mover == null) return;
        bool moveLeft = this.mover.Position > this.targetToMoveTo.Position;

        // Cannot move beyond the target's position.
        int maxFwdDistance = Math.Min(Math.Abs(mover.Position - targetToMoveTo.Position), this.forwardDistance);

        CombatManager.eventManager.BroadcastEvent(
            new CombatEventUnitMoved(this.mover, 
                                     this.mover.Position, 
                                     maxFwdDistance, 
                                     isMoveLeft: moveLeft,
                                     isForcedMovement: false));

        if (moveLeft) {
            this.mover.Position -= maxFwdDistance;
        } else {
            this.mover.Position += maxFwdDistance;
        }
        return;
    }
}