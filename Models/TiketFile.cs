using DevExpress.Xpo;

[Persistent("TiketFile")]
public class TiketFile: XPObject
{
    public TiketFile(Session session) : base(session) { }

    private string _fileName;
    private string _fileUrl;
    private bool _isResponse;
    private Tiket _tiket;
    private TiketSolution _tiketSolution;

    [Size(SizeAttribute.Unlimited)]
    public string FileName
    {
        get => _fileName;
        set => SetPropertyValue(nameof(FileName), ref _fileName, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string FileUrl
    {
        get => _fileUrl;
        set => SetPropertyValue(nameof(FileUrl), ref _fileUrl, value);
    }

    public bool IsResponse
    {
        get => _isResponse;
        set => SetPropertyValue(nameof(IsResponse), ref _isResponse, value);
    }

    [Association("Tiket-TiketFiles")]
    public Tiket Tiket
    {
        get => _tiket;
        set => SetPropertyValue(nameof(Tiket), ref _tiket, value);
    }

    [Association("TiketSolution-TiketFiles")]
    public TiketSolution TiketSolution
    {
        get => _tiketSolution;
        set => SetPropertyValue(nameof(TiketSolution), ref _tiketSolution, value);
    }


}