using System.Collections.Generic;
using System.Linq;
using Godot;

public class Break : AbstractAbility, IEventHandler<CombatEventDieHit>, IEventHandler<CombatEventDieClash>{
    public static string id = "BREAK";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 2;
    private static int min_range = 0;
    private static int max_range = 1;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Die bluntDie = new Die(DieType.BLUNT, 9, 15, "ANTI_BLOCK");

    public Break(): base(
        id,
        strings,
        AbilityType.ATTACK,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.ENEMIES_ONLY}
    ){
        this.BASE_DICE = new List<Die>{bluntDie};
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_CLASH, this, CombatEventPriority.STANDARD);
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_DIE_HIT, this, CombatEventPriority.STANDARD);
    }

    public void HandleEvent(CombatEventDieClash data){
        if (data.attackerDie == bluntDie && data.reactDie.DieType == DieType.BLOCK){
            data.attackerRoll += 4;
            Logging.Log($"Break roll increased by +4 from clashing against Block die (from {data.attackerRoll - 4} => {data.attackerRoll})", Logging.LogLevel.ESSENTIAL);
        }
        if (data.reactDie == bluntDie && data.attackerDie.DieType == DieType.BLOCK){
            data.reactRoll += 4;
            Logging.Log($"Break roll increased by +4 from clashing against Block die (from {data.reactRoll - 4} => {data.reactRoll})", Logging.LogLevel.ESSENTIAL);
        }
    }

    public void HandleEvent(CombatEventDieHit data){
        if (data.die == bluntDie){
            CombatManager.ExecuteAction(new ApplyStatusAction(data.hitUnit, new DebuffBroken(), 2));
            CombatManager.ExecuteAction(new ApplyStatusAction(data.hitUnit, new ConditionNextRoundStatusGain(new DebuffBroken()), 2));
        }
    }
}