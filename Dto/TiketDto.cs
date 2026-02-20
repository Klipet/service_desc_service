public class TiketDto
    {
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Phone { get; set; }
    public DateTime DataPhone { get; set; }
    public bool ResaultPhone { get; set; }
    public DateTime DateSecondPhone { get; set; }
    public string BugNumber { get; set; }
    public bool BugTransfer { get; set; }
    public DateTime DataCreted { get; set; }
    public DateTime DataModefire { get; set; }

    // User
    public int UserId { get; set; }  // ← для получения от клиента
    public string UserName { get; set; }  // ← для отдачи клиенту

    //workSpace
    public int WorkSpaceId { get; set; } // WorkSpace
    public String WorkSpaceName { get; set; }

    //state
    public string StateName { get; set; }
    public int StateId { get; set; }

    //typeTiket
    public string TypeTiketName { get; set; }
    public int TypeTiketId { get; set; }


    //preority
    public string PreorityName { get; set; }
    public int PreorityId { get; set; }

    //preority
    public string ModeName { get; set; }
    public int ModeId { get; set; }

    //SubCategory
    public string SubCategoryName { get; set; }
    public int SubCategoryId { get; set; }

    //Category
    public string CategoryName { get; set; }
    public int CategoryId { get; set; }

    //Autor
    public string AuthorName { get; set; }
    public int AuthorId { get; set; }

    //Platform
    public string PlatformName { get; set; }
    public int PlatformId { get; set; }

    //Company
    public string CompanyName { get; set; }
    public int CompanyId { get; set; }

}

