public class TiketSolutionDto
{
    public int Tiket {  get; set; }
    public int Author { get; set; }   
    public int User { get; set; }
    public string MessageText { get; set; }
    private DateTime CreatedAt { get; set; }
    public List<string> EmailList { get; set; } = new List<string>();
}
public class TiketSolutionDtoResponse
{
    public Tiket Tiket { get; set; }
    public Author Author { get; set; }
    public string MessageText { get; set; }
    private List<string> EmailList { get; set; }
}