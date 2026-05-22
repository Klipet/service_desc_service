using DevExpress.Xpo;

[Persistent("TiketMessage")]
public class TiketMessage : XPObject
{
    public TiketMessage(Session session) : base(session) { }
    private Tiket _tiket;
    private Author? _author;
    private User? _user;
    private string _messageText;
    private string _emailMessageId;
    private DateTime _createdAt;
    private bool _isRead;
    private DateTime? _readAt;
    private bool _isUser;


    public User? User
    {
        get => _user;
        set => SetPropertyValue(nameof(User), ref _user, value);
    }
    public bool IsUser
    {
        get => _isUser;
        set => SetPropertyValue(nameof(IsUser), ref _isUser, value);
    }

    public DateTime? ReadAt
    {
        get => _readAt;
        set => SetPropertyValue(nameof(ReadAt), ref _readAt, value);
    }
    public bool IsRead
    {
        get => _isRead;
        set => SetPropertyValue(nameof(IsRead), ref _isRead, value);
    }

    [Association("Tiket-TiketMessages")]
    public Tiket Tiket
    {
        get => _tiket;
        set => SetPropertyValue(nameof(Tiket), ref _tiket, value);
    }
    public Author? Author
    {
        get => _author;
        set => SetPropertyValue(nameof(Author), ref _author, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string MessageText
    {
        get => _messageText;
        set => SetPropertyValue(nameof(MessageText), ref _messageText, value);
    }
    public string EmailMessageId
    {
        get => _emailMessageId;
        set => SetPropertyValue(nameof(EmailMessageId), ref _emailMessageId, value);
    }
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetPropertyValue(nameof(CreatedAt), ref _createdAt, value);
    }

}
