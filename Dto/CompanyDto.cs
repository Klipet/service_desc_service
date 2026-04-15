public class CompanyDto: BaseDto
{
    public string Idnp { get; set; }
    public int ComapnyStateOid { get; set; }
    public string ComapnyStateName { get; set; }
}

public class CompanyCreateDto : BaseDto
{
    public string Idnp { get; set; }
    public CompanyState ComapnyStateOid { get; set; }
}
