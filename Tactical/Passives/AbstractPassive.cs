namespace CharacterPassives;

public abstract class AbstractPassive : IEventSubscriber {
    public string ID;
    public string NAME;
    public string DESC;
    public int COST;
    public AbstractCharacter OWNER;

    public AbstractPassive(string ID, Localization.PassiveStrings PASSIVE_STRINGS, int COST){
        this.ID = ID;
        this.NAME = PASSIVE_STRINGS.NAME;
        this.DESC = PASSIVE_STRINGS.DESC;
        this.COST = COST;
    }

    public virtual void InitSubscriptions() {
    }
}