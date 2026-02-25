using DevExpress.Xpo;

[Persistent("TiketComment")]
public class TiketComment: XPObject
{
    public TiketComment(Session session) : base(session) { }
    private Tiket _tiket;
    private Author _author;
    private string _messageText;
    private string _emailMessageId;
    private DateTime _createdAt;

    [Association("Tiket-TiketMessages")]
    public Tiket Tiket
    {
        get => _tiket;
        set => SetPropertyValue(nameof(Tiket), ref _tiket, value);
    }
    public Author Author
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
