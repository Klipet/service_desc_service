using DevExpress.Xpo;
[Persistent("NewTiket")]
[DeferredDeletion(false)]

public class NewTiket : XPLiteObject
{
        public NewTiket(Session session) : base(session) { }
        private int _id;
        private string _title;
        private string _description;
        private string _company;
        private Category _category;
        private SubCategory _subCategory;
        private State _state;
        private TiketType _typeTiket;
        private string _author;
        private string _platform;
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
        private User _user;


    [Key(AutoGenerate = true)]
    public int Id
    {
        get => _id;
        set => SetPropertyValue(nameof(Id), ref _id, value);
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
        public string Company
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
        public string Author
        {
            get => _author;
            set => SetPropertyValue(nameof(Author), ref _author, value);
        }
        public string Platform
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
            set => SetPropertyValue(nameof(Preorety), ref _preority, value);
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

    }
