using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using Godot;

public class PushAction : AbstractAction {

    private AbstractCharacter pusher;
    private AbstractCharacter pushee;
    private int pushDistance;

    public PushAction(AbstractCharacter pusher, AbstractCharacter pushee, int pushDistance){
        this.pusher = pusher;
        this.pushee = pushee;
        this.pushDistance = pushDistance;
    }

    public override void Execute(){
        if (this.pusher == null || this.pushee == null) return;
        bool pushLeft = this.pusher.Position > this.pushee.Position;

        CombatManager.eventManager.BroadcastEvent(
            new CombatEventUnitMoved(this.pushee, 
                                     this.pushee.Position, 
                                     pushDistance, 
                                     isMoveLeft: pushLeft,
                                     isForcedMovement: true));

        // If pusher and pushee are in the same lane, push towards the center.
        if (this.pusher.Position == this.pushee.Position){
            if (this.pusher.Position <= 3){
                this.pushee.Position = Math.Min(this.pushee.Position + this.pushDistance, GameVariables.MAX_LANES);
            } else {
                this.pushee.Position = Math.Max(this.pushee.Position - this.pushDistance, GameVariables.MIN_LANES);
            }
            return;
        }

        if (pushLeft) {
            this.pushee.Position = Math.Max(this.pushee.Position - this.pushDistance, GameVariables.MIN_LANES);
        } else {
            this.pushee.Position = Math.Min(this.pushee.Position + this.pushDistance, GameVariables.MAX_LANES);
        }
        return;
    }
}