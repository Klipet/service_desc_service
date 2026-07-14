using DevExpress.Xpo;

[Persistent("Phone")]
public class Phone: XPObject
{

    public Phone(Session session) : base(session) { }

    private string _number;
    private Author _author;
    private DateTime dataCreated;
    private DateTime dataModefire;

    public string Number
    {
        get => _number;
        set => SetPropertyValue(nameof(Number), ref _number, value);
    }
    [Association("Author-Phones")]
    public Author Author
    {
        get => _author;
        set => SetPropertyValue(nameof(Author), ref _author, value);
    }
    public DateTime DataCreated
    {
        get => dataCreated;
        set => SetPropertyValue(nameof(DataCreated), ref dataCreated, value);
    }
    public DateTime DataModefire
    {
        get => dataModefire;
        set => SetPropertyValue(nameof(DataModefire), ref dataModefire, value);
    }
}
