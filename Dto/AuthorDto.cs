public class AuthorDto: BaseDto
{
    public string phone { get; set; }
    public string email { get; set; }

    //Platform
    public string PlatformName { get; set; }
    public int PlatformId { get; set; }
}