using System.Collections.Generic;

public class Again : AbstractAbility, IEventHandler<CombatEventAbilityActivated>, IEventHandler<CombatEventCombatEnd> {
    public static string id = "AGAIN";
    private static Localization.AbilityStrings strings = Localization.LocalizationLibrary.Instance.GetAbilityStrings(id);

    private static int cd = 5;
    private static int min_range = 0;
    private static int max_range = 2;
    private static bool targetsLane = false;
    private static bool needsUnit = true;

    private Dictionary<AbstractCharacter, AbstractAbility> mostRecentAbilities = new();

    public Again(): base(
        id,
        strings,
        AbilityType.UTILITY,
        cd,
        min_range,
        max_range,
        targetsLane,
        needsUnit,
        new HashSet<TargetingModifiers>{TargetingModifiers.ALLIES_ONLY, TargetingModifiers.NOT_SELF},
        FIXED_COOLDOWN: true
    ){
    }

    public override void InitSubscriptions(){
        base.InitSubscriptions();
        CombatEventManager.instance?.Subscribe(CombatEventType.ON_COMBAT_END, this, CombatEventPriority.STANDARD);
    }

    public override void HandleEvent(CombatEventAbilityActivated data){
        base.HandleEvent(data);
        if (data.caster.CHAR_FACTION == this.OWNER.CHAR_FACTION){
            mostRecentAbilities[data.caster] = data.abilityActivated;
        }
        if (data.abilityActivated.Equals(this)){
            if (!mostRecentAbilities.ContainsKey(data.target)) return;
            // TODO: Change this from InvigorateAction to RevitalizeAction.
            CombatManager.ExecuteAction(new InvigorateAction(mostRecentAbilities[data.target], 99));
        }
    }

    public virtual void HandleEvent(CombatEventCombatEnd data){
        this.mostRecentAbilities.Clear();
    }
}