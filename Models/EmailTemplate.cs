using DevExpress.Xpo;

[Persistent("EmailTemplate")]
public class EmailTemplate : XPObject
{
    public EmailTemplate(Session session) : base(session) { }

    private string _subjectTemplate;
    private string _bodyHtmlTemplate;
    private bool _isActive;
    private bool _isHtml;

    [Association("State-EmailTemplates")]
    public State State { get; set; }

    [Size(-1)]
    public string SubjectTemplate
    {
        get => _subjectTemplate;
        set => SetPropertyValue(nameof(SubjectTemplate), ref _subjectTemplate, value);
    }



    [Size(SizeAttribute.Unlimited)]
    public string BodyHtmlTemplate
    {
        get => _bodyHtmlTemplate;
        set => SetPropertyValue(nameof(BodyHtmlTemplate), ref _bodyHtmlTemplate, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }

    public bool IsHtml
    {
        get => _isHtml;
        set => SetPropertyValue(nameof(IsHtml), ref _isHtml, value);
    }
}
