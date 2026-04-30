public class TiketResponseDto
    {
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Phone { get; set; } = string.Empty;
    public DateTime DataPhone { get; set; } = DateTime.Now;
    public bool ResaultPhone { get; set; }
    public DateTime DateSecondPhone { get; set; }
    public string BugNumber { get; set; }
    public bool BugTransfer { get; set; }
    public DateTime DataCreted { get; set; }
    public DateTime DataModefire { get; set; }

    // User
    public int UserId { get; set; }  // ← для получения от клиента
    public string UserName { get; set; } = string.Empty;  // ← для отдачи клиенту

    //workSpace
    public int WorkSpaceId { get; set; } 
    public String WorkSpaceName { get; set; } = String.Empty;

    //state
    public string StateName { get; set; } = string.Empty;
    public int StateId { get; set; }

    //typeTiket
    public string TypeTiketName { get; set; } = string.Empty;
    public int TypeTiketId { get; set; }


    //preority
    public string PreorityName { get; set; } = string.Empty;
    public int PreorityId { get; set; }

    //preority
    public string ModeName { get; set; } = string.Empty;
    public int ModeId { get; set; }

    //SubCategory
    public string SubCategoryName { get; set; } = string.Empty;
    public int SubCategoryId { get; set; }

    //Category
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }

    //Autor
    public string AuthorName { get; set; } = string.Empty;
    public int AuthorId { get; set; }

    //Platform
    public string PlatformName { get; set; } = string.Empty;
    public int PlatformId { get; set; }
    //Company
    public string CompanyName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public DateTime DueDate { get; set; }

    public List<TiketFileDto> Files { get; set; } = new();
    public List<TiketSolutionDto> Solution { get; set; } = new();
    public List<TiketCommentDto> Comment { get; set; } = new();
}

public class TiketPostDto
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
    //workSpace
    public int WorkSpaceId { get; set; } // WorkSpace
    //state
    public int StateId { get; set; }

    //typeTiket
    public int TypeTiketId { get; set; }
    public int PreorityId { get; set; }
    public int ModeId { get; set; }
    public int SubCategoryId { get; set; }
    public int CategoryId { get; set; }
    public int AuthorId { get; set; }
    public int PlatformId { get; set; }
    public int CompanyId { get; set; }
}



