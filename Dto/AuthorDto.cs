public class AuthorDto: BaseDto
{
    public List<PhoneDtoAuthor> Phone { get; set; }
    public string Email { get; set; }

    //Platform
    public string PlatformName { get; set; }
    public int PlatformId { get; set; }
}

public class AuthorPostDto : BaseDto
{
    public string PhoneOid { get; set; }
    public string Email { get; set; }
    public int PlatformId { get; set; }
}