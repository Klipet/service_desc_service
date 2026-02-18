public class NewTiketDto
    {
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public string Phone { get; set; }
    public string Company { get; set; }
    public string State { get; set; }
    public string TypeTiket { get; set; }
    public string Platform { get; set; }
    public string Preorety { get; set; }
    public DateTime DataPhone { get; set; }
    public bool ResaultPhone { get; set; }
    public DateTime DateSecondPhone { get; set; }
    public string BugNumber { get; set; }
    public bool BugTransfer { get; set; }
    public string Mode { get; set; }
    public DateTime DataCreted { get; set; }

    // User
    public int UserId { get; set; }  // ← для получения от клиента
    public string UserName { get; set; }  // ← для отдачи клиенту

    //workSpace
    public int WorkSpaceId { get; set; } // WorkSpace
    public String WorkSpaceName { get; set; }

}

