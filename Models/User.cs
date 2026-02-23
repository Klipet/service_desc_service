using DevExpress.Xpo;

[Persistent("User")]
public class User: XPObject
{
    public User(Session session) : base(session){}
    private int _id;
    private string _name;
    private string _firstName;
    private string _email;
    private string _passwordHash;
    private string _phone;
    private string _loghin;
    private WorkSpace _workSpace;
    private DateTime _dateCreated;


    [Association("User-Tikets")]
    public XPCollection<Tiket> Tikets => GetCollection<Tiket>(nameof(Tikets));

    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
    public string FirstName
    {
        get => _firstName;
        set => SetPropertyValue(nameof(FirstName), ref _firstName, value);
    }
    public string Email
    {
        get => _email;
        set => SetPropertyValue(nameof(Email), ref _email, value);
    }
    public string PasswordHash
    {
        get => _passwordHash;
        set => SetPropertyValue(nameof(PasswordHash), ref _passwordHash, value);
    }
    public string Phone
    {
        get => _phone;
        set => SetPropertyValue(nameof(Phone), ref _phone, value);
    }
    public string Loghin
    {
        get => _loghin;
        set => SetPropertyValue(nameof(Loghin), ref _loghin, value);
    }
    [Association("WorkSpace-User")]
    public WorkSpace WorkSpace
    {
        get => _workSpace;
        set => SetPropertyValue(nameof(WorkSpace), ref _workSpace, value);
    }
    public DateTime DateCreated
    {
        get => _dateCreated;
        set => SetPropertyValue(nameof(DateCreated), ref _dateCreated, value);
    }
}

