public class TiketFileDto
{
    public int Id { get; set; }
    public string FileUrl { get; set; }
    public string FileName { get; set; }
    public bool IsResponse { get; set; }
    public int? TiketOid { get; set; }
    public int? TiketResponseOid { get; set; }


}