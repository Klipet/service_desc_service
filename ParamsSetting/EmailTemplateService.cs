using DevExpress.Xpo;
using System.Text.RegularExpressions;

public class EmailTemplateService
{
    private readonly UnitOfWork _uow;

    public EmailTemplateService(UnitOfWork uow)
    {
        _uow = uow;
    }

    public (string subject, string body, bool isHtml) Render(
    Tiket tiket,
    Dictionary<string, string> values)
    {
        var template = _uow.Query<EmailTemplate>()
            .FirstOrDefault(t =>
                t.IsActive &&
                (t.State == null || t.State.Oid == tiket.State.Oid));

        if (template == null)
            throw new Exception("Template not found");

        var subject = Replace(template.SubjectTemplate, values);
        var body = Replace(template.BodyHtmlTemplate, values);

        return (subject, body, template.IsHtml);
    }

    private string Replace(string text, Dictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return Regex.Replace(text, @"\{(.*?)\}", match =>
        {
            var key = match.Groups[1].Value;

            return values.ContainsKey(key)
                ? values[key] ?? ""
                : match.Value;
        });
    }
}