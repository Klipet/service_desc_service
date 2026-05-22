public class TiketEmailServiceDto
{
    public bool IsNewTicket { get; set; }
    public TiketResponseDto? Ticket { get; set; }
    public TiketMessageDto? Comment { get; set; }
}