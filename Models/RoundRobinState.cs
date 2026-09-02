using DevExpress.Xpo;

public class RoundRobinState : XPObject
{
    public RoundRobinState(Session session)
        : base(session) { }

    private WorkSpace _workSpace;
    private User _lastAssignedUser;

    public WorkSpace WorkSpace
    {
        get => _workSpace;
        set => SetPropertyValue( nameof(WorkSpace),ref _workSpace, value);
    }

    public User LastAssignedUser
    {
        get => _lastAssignedUser;
        set => SetPropertyValue(nameof(LastAssignedUser),ref _lastAssignedUser, value);
    }
}