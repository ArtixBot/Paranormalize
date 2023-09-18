using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// When initiating a combat encounter, the ScenarioInfo provides all of the information necessary to the CombatManager.
/// </summary>
public partial class ScenarioInfo {
    public List<(CharacterInfo character, int startingPosition)> fighters;

    public ScenarioInfo(List<(CharacterInfo character, int startingPosition)> fighters){
        this.fighters = fighters;
    }
}
