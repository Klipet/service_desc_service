public class TiketMessageDto
{
    public int Id { get; set; }
    public int Tiket { get; set; }
    public int Author { get; set; }
    public int User { get; set; }
    public bool IsUser { get; set; }
    public string MailMessageId { get; set; }
    public string MessageText { get; set; }
    public List<string> Email { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime ReadAt { get; set; }
    public bool IsRead { get; set; }

}


public class TiketMessageResponseDto
{
    public int Id { get; set; }
    public int Tiket { get; set; }
    public int AuthorOid { get; set; }
    public string AuthorName { get; set; }
    public bool IsUser {  get; set; }
    public string MailMessageId { get; set; }
    public string MessageText { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ReadAt { get; set; }
    public bool IsRead { get; set; }

}