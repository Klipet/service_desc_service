
public class WorkSpaceDto
{
    public int Oid { get; set; } 
    public string Name { get; set; }
    public bool Active  { get; set; }
    public  DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateModifire { get; set; } = DateTime.UtcNow;
}
