public class AuthorDto: BaseDto
{
    public string Phone { get; set; }
    public string Email { get; set; }

    //Platform
    public string PlatformName { get; set; }
    public int PlatformId { get; set; }
}