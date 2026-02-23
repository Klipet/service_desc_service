public class UserDto
{
 //   public int Oid { get; set; }
    public string Name { get; set; }
    public string FirstName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string Loghin { get; set; }
    public DateTime DateCreated { get; set; }
    //workSpace
    public int WorkSpaceId { get; set; }
//    public String WorkSpaceName { get; set; }
}

public class UserResponseDto
{
    public int Oid { get; set; }
    public string Name { get; set; }
    public string FirstName { get; set; }
    public string Email { get; set; }

    public string Loghin { get; set; }
    public string Phone { get; set; }
    public string WorkSpaceName { get; set; }
    public int WorkSpaceId { get; set; }
    public DateTime DateCreated { get; set; }
}
