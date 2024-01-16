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
        GD.Print($"{this.mover} and {this.targetToMoveTo}");
        if (this.mover == null || this.targetToMoveTo == null) return;
        bool moveLeft = this.mover.Position > this.targetToMoveTo.Position;

        // Cannot move beyond the target's position.
        int maxFwdDistance = Math.Min(Math.Abs(mover.Position - targetToMoveTo.Position), this.forwardDistance);

        CombatManager.eventManager.BroadcastEvent(
            new CombatEventUnitMoved(this.mover, 
                                     this.mover.Position, 
                                     forwardDistance, 
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