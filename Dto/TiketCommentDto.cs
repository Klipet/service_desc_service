public class TiketCommentDto
{
    public int Id { get; set; }
    public int Tiket { get; set; }
    public int Author { get; set; }
    public string MailMessageId { get; set; }
    public string MessageText { get; set; }
    public DateTime CreatedAt { get; set; }

}