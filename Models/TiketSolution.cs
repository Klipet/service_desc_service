using DevExpress.Xpo;
using System.ComponentModel.DataAnnotations.Schema;
[Persistent("TiketSolution")]
public class TiketSolution: XPObject
{
    public TiketSolution(Session session) : base(session) { }
    private Tiket _tiket;
    private Author _author;
    private User _user;
    private string _messageText;
    private DateTime _createdAt;
    private string _emailList;


    [Association("TiketSolution-TiketFiles")]
    public XPCollection<TiketFile> Files => GetCollection<TiketFile>(nameof(Files));

    [Association("Tiket-Solutions")]
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
    public User User
    {
        get => _user;
        set => SetPropertyValue(nameof(User), ref _user, value);
    }



    [Size(SizeAttribute.Unlimited)]
    public string MessageText
    {
        get => _messageText;
        set => SetPropertyValue(nameof(MessageText), ref _messageText, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string EmailList
    {
        get => _emailList;
        set => SetPropertyValue(nameof(EmailList), ref _emailList, value);
    }

    [NotMapped]
    public List<string> EmailListParsed
    {
        get => string.IsNullOrEmpty(_emailList) ? new List<string>() : _emailList.Split(';').ToList();
        set => EmailList = value == null ? null : string.Join(';', value);
    }

    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetPropertyValue(nameof(CreatedAt), ref _createdAt, value);
    }

}