public class TicketPhoneDto
{
    public int? Id { get; set; }

    public int TicketId { get; set; }

    public int? UserId { get; set; }

    public string Phone { get; set; }

    public bool ResultPhone { get; set; }
    public int ResultPhoneOid { get; set; }
    public string ResultPhoneName { get; set; }

    public string Comment { get; set; }
}


public class TicketPhoneResponseDto
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int? UserId { get; set; }

    public string UserName { get; set; }

    public string Phone { get; set; }

    public int ResultPhoneOid { get; set; }
    public string ResultPhoneName { get; set; }

    public string Comment { get; set; }

    public DateTime DataCreated { get; set; }

    public bool IsActive { get; set; }
}

public class TicketPhonePostDto
{

    public int? TicketId { get; set; }

    public int? UserId { get; set; }

    public string? Phone { get; set; }

    public int? ResultPhoneOid { get; set; }
    public string? Comment { get; set; }
    public bool? IsActive { get; set; }
}
