namespace CharacterPassives;

public abstract class AbstractPassive : IEventSubscriber {
    public string ID;
    public string NAME;
    public string DESC;
    public int COST;
    public AbstractCharacter OWNER;

    public virtual void InitSubscriptions() {
    }
}