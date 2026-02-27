public class EmailTemplateDto
{
    public int Oid { get; set; }
    public string SubjectTemplate {  get; set; }
    public string BodyHtmlTemplate { get; set; }
    public bool IsActive {  get; set; }
    public bool IsHtml {  get; set; }
    public int StateOid { get; set; }
    public string StateName { get; set; }
}