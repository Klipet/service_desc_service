using DevExpress.Xpo;

[Persistent("TiketLog")]
public class TiketLog: XPObject
{
    public TiketLog(Session session) : base(session) { }

    public int TiketId { get; set; }
    public string Action { get; set; }
    public DateTime ChangedAt { get; set; }
    public User User { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public Company Company { get; set; }
    public Category Category { get; set; }
    public SubCategory SubCategory { get; set; }
    public State State { get; set; }
    public TiketType TypeTiket { get; set; }
    public Author Author { get; set; }
    public Platform Platform { get; set; }
    public WorkSpace WorkSpace { get; set; }
    public Preority Preority { get; set; }
    public string Phone { get; set; }
    public bool ResaultPhone { get; set; }
    public DateTime DataPhone { get; set; }
    public DateTime DateSecondPhone { get; set; }
    public bool BugTransfer { get; set; }
    public string BugNumber { get; set; }
    public Mode Mode { get; set; }
    public DateTime DataCreated { get; set; }
    public DateTime DataModefire { get; set; }
}

