public class PhoneDto
{
    public int Oid { get; set; }
    public string Number { get; set; }
    public string AuthorName { get; set; }
    public int AuthorId { get; set; }
    public DateTime DataCreated { get; set; }
    public DateTime DataModefire { get; set; } = DateTime.Now;
}

public class PhoneDtoAuthor
{
    public int Oid { get; set; }
    public string Number { get; set; }
    public DateTime DataCreated { get; set; }
}


