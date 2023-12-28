using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PullAction : AbstractAction {

    private AbstractCharacter puller;
    private AbstractCharacter pullee;
    private int pullDistance;

    public PullAction(AbstractCharacter puller, AbstractCharacter pullee, int pullDistance){
        this.puller = puller;
        this.pullee = pullee;
        this.pullDistance = pullDistance;
    }

    public override void Execute(){
        if (this.puller == null || this.pullee == null) return;

        // Cannot pull beyond the puller's position.
        int maxPullDistance = Math.Min(Math.Abs(puller.Position - pullee.Position), this.pullDistance);
        bool pullLeft = this.puller.Position < this.pullee.Position;
        
        CombatManager.eventManager.BroadcastEvent(
            new CombatEventUnitMoved(this.pullee, 
                                     this.pullee.Position, 
                                     maxPullDistance, 
                                     isMoveLeft: pullLeft, 
                                     isForcedMovement: true));

        if (pullLeft) {
            this.pullee.Position -= maxPullDistance;
        } else {
            this.pullee.Position += maxPullDistance;
        }
        return;
    }
}