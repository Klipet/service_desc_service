using DevExpress.Xpo;
[Persistent("EmailQueue")]
public class EmailQueue: XPObject
{
    public EmailQueue(Session session) : base(session) { }

    private string _to;
    private string _subject;
    private string _body;
    private bool _isSent;
    private DateTime _createdAt;
    private DateTime? _sentAt;
    private string _errorMessage;
    public string To
    {
        get => _to;
        set => SetPropertyValue(nameof(To), ref _to, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string Subject
    {
        get => _subject;
        set => SetPropertyValue(nameof(Subject), ref _subject, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string Body
    {
        get => _body;
        set => SetPropertyValue(nameof(Body), ref _body, value);
    }
    
    public bool IsSent
    {
        get => _isSent;
        set => SetPropertyValue(nameof(IsSent), ref _isSent, value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetPropertyValue(nameof(CreatedAt), ref _createdAt, value);
    }

    public DateTime? SentAt
    {
        get => _sentAt;
        set => SetPropertyValue(nameof(SentAt), ref _sentAt, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetPropertyValue(nameof(ErrorMessage), ref _errorMessage, value);
    }
}
