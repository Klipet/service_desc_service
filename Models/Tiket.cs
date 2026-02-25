using DevExpress.Xpo;
[Persistent("Tiket")]
[DeferredDeletion(false)]

public class Tiket : XPObject
{
        public Tiket(Session session) : base(session) { }
        private int _id;
        private string _title;
        private string _description;
        private Company _company;
        private Category _category;
        private SubCategory _subCategory;
        private State _state;
        private TiketType _typeTiket;
        private Author _author;
        private Platform _platform;
        private WorkSpace _workSpace;
        private Preority _preority;
        private string _phone;
        private bool _resaultPhone;
        private DateTime _dataPhone;
        private DateTime _dateSecondPhone;
        private bool _bugTransfer;
        private string _bugNumber;
        private Mode _mode;
        private DateTime _dataCreated;
        private DateTime _dataModefire;
        private User _user;
        private DateTime? _dueDate;
    private string _email;
    private string _messageId;

    [Association("Tiket-TiketMessages")]
    public XPCollection<TiketComment> Messages
    {
        get { return GetCollection<TiketComment>(nameof(Messages)); }
    }
    public string Title
        {
            get => _title;
            set => SetPropertyValue(nameof(Title),ref _title, value);
        }
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get => _description;
            set => SetPropertyValue(nameof(Description), ref _description, value);
        }
    [Association("Company-Tikets")]
    public Company Company
        {
            get => _company;
            set => SetPropertyValue(nameof(Company), ref _company, value);
        }
    [Association("Category-Tikets")]
    public Category Category
        {
            get => _category;
            set => SetPropertyValue(nameof(Category), ref _category, value);
        }
    [Association("SubCategory-Tikets")]
    public SubCategory SubCategory
        {
            get => _subCategory;
            set => SetPropertyValue(nameof(SubCategory), ref _subCategory, value);
        }
    [Association("State-Tikets")]
    public State State
        {
            get => _state;
            set => SetPropertyValue(nameof(State), ref _state, value);
        }
    [Association("TiketType-Tikets")]
    public TiketType TypeTiket
        {
            get => _typeTiket;
            set => SetPropertyValue(nameof(TypeTiket), ref _typeTiket, value);
        }
    [Association("Author-Tikets")]
    public Author Author
        {
            get => _author;
            set => SetPropertyValue(nameof(Author), ref _author, value);
        }
    [Association("Platform-Tikets")]
    public Platform Platform
        {
            get => _platform;
            set => SetPropertyValue(nameof(Platform), ref _platform, value);
        }
        [Association("WorkSpace-Tikets")]
        public WorkSpace WorkSpace
        {
            get => _workSpace;
            set => SetPropertyValue(nameof(WorkSpace), ref _workSpace, value);
        }

        [Association("User-Tikets")]
        public User User
        {
            get => _user;
            set => SetPropertyValue(nameof(User), ref _user, value);
        }
    [Association("Preority-Tikets")]
    public Preority Preorety
    {
        get => _preority;
        set 
        {
         SetPropertyValue(nameof(Preorety), ref _preority, value);

            // При выборе приоритета — автоматически считаем DueDate
            if (_preority != null)
            {
                var calculator = new DeadlineCalculator(Session);
                DueDate = calculator.Calculate( _preority.DeadlineHours);
            } 
        }
        
    }
        public string Phone
        {
            get => _phone;
            set => SetPropertyValue(nameof(Phone), ref _phone, value);
        }
        public bool ResaultPhone
        {
            get => _resaultPhone;
            set => SetPropertyValue(nameof(ResaultPhone), ref _resaultPhone, value);
        }
        public DateTime DataPhone
        {
            get => _dataPhone;
            set => SetPropertyValue(nameof(DataPhone), ref _dataPhone, value);
        }
    public DateTime DataModefire
    {
        get => _dataModefire;
        set => SetPropertyValue(nameof(DataPhone), ref _dataModefire, value);
    }
    public DateTime DateSecondPhone
        {
            get => _dateSecondPhone;
            set => SetPropertyValue(nameof(DateSecondPhone), ref _dateSecondPhone, value);
        }
        public bool BugTransfer
        {
            get => _bugTransfer;
            set => SetPropertyValue(nameof(BugTransfer), ref _bugTransfer, value);
        }
        public String BugNumber
        {
            get => _bugNumber;
            set => SetPropertyValue(nameof(BugNumber), ref _bugNumber, value);
        }
    [Association("Mode-Tikets")]
    public Mode Mode
        {
            get => _mode;
            set => SetPropertyValue(nameof(Mode), ref _mode, value);
        }
        public DateTime DataCreted
        {
            get => _dataCreated;
            set => SetPropertyValue(nameof(DataCreted), ref _dataCreated, value);
        }
    public DateTime? DueDate
    {
        get => _dueDate;
        set => SetPropertyValue(nameof(DueDate), ref _dueDate, value);
    }
    public String Email
    {
        get => _email;
        set => SetPropertyValue(nameof(Email), ref _email, value);
    }
    public String MessageId
    {
        get => _messageId;
        set => SetPropertyValue(nameof(MessageId), ref _messageId, value);
    }


    private TiketLog CreateLog(string action)
    {
        return new TiketLog(Session)
        {
            TiketId = this.Oid,
            Action = action,
            ChangedAt = DateTime.UtcNow,
            User = _user,

            Title = _title,
            Description = _description,
            Company = _company,
            Category = _category,
            SubCategory = _subCategory,
            State = _state,
            TypeTiket = _typeTiket,
            Author = _author,
            Platform = _platform,
            WorkSpace = _workSpace,
            Preority = _preority,
            Phone = _phone,
            ResaultPhone = _resaultPhone,
            DataPhone = _dataPhone,
            DateSecondPhone = _dateSecondPhone,
            BugTransfer = _bugTransfer,
            BugNumber = _bugNumber,
            Mode = _mode,
            DataCreated = _dataCreated,
            DataModefire = _dataModefire
        };
    }
    protected override void OnSaving()
    {
        base.OnSaving();
        CreateLog(Session.IsNewObject(this) ? "INSERT" : "UPDATE");
    }

    protected override void OnDeleting()
    {
        base.OnDeleting();
        CreateLog("DELETE");
    }

}
