// =============================================================================
// Bogus DataSet 使用範例
// 展示各種內建 DataSet 的完整用法
// =============================================================================

using Bogus;
using FluentAssertions;
using Xunit;

namespace BogusDataSets.Templates;

// =============================================================================
// DataSet 完整範例展示
// =============================================================================

public class DataSetExamples
{
    private readonly Faker _faker = new();

    #region Person DataSet

    /// <summary>
    /// Person DataSet - 個人資訊
    /// </summary>
    [Fact]
    public void PersonDataSet_個人資訊()
    {
        // Person 是預先產生的完整個人資料
        var person = _faker.Person;

        // 基本資訊
        var fullName = person.FullName;        // 完整姓名
        var firstName = person.FirstName;      // 名字
        var lastName = person.LastName;        // 姓氏
        var userName = person.UserName;        // 使用者名稱

        // 聯絡資訊
        var email = person.Email;              // 電子郵件
        var phone = person.Phone;              // 電話號碼
        var website = person.Website;          // 網站

        // 個人屬性
        var gender = person.Gender;            // 性別 (Male/Female)
        var dateOfBirth = person.DateOfBirth;  // 生日

        // 公司資訊
        var company = person.Company;          // 公司資訊物件

        // 地址資訊
        var address = person.Address;          // 地址物件

        // 驗證
        fullName.Should().NotBeNullOrEmpty();
        email.Should().Contain("@");
        dateOfBirth.Should().BeBefore(DateTime.Now);
    }

    /// <summary>
    /// Name DataSet - 姓名相關
    /// </summary>
    [Fact]
    public void NameDataSet_姓名相關()
    {
        var firstName = _faker.Name.FirstName();          // 名字
        var lastName = _faker.Name.LastName();            // 姓氏
        var fullName = _faker.Name.FullName();            // 完整姓名
        var prefix = _faker.Name.Prefix();                // 稱謂 (Mr., Ms., Dr.)
        var suffix = _faker.Name.Suffix();                // 後綴 (Jr., Sr., III)
        var jobTitle = _faker.Name.JobTitle();            // 職稱
        var jobDescriptor = _faker.Name.JobDescriptor();  // 職位描述
        var jobArea = _faker.Name.JobArea();              // 工作領域

        firstName.Should().NotBeNullOrEmpty();
        jobTitle.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Address DataSet

    /// <summary>
    /// Address DataSet - 地址資訊
    /// </summary>
    [Fact]
    public void AddressDataSet_地址資訊()
    {
        // 完整地址
        var fullAddress = _faker.Address.FullAddress();
        
        // 地址組成部分
        var streetAddress = _faker.Address.StreetAddress();    // 街道地址
        var secondaryAddress = _faker.Address.SecondaryAddress(); // 次要地址 (公寓號)
        var city = _faker.Address.City();                       // 城市
        var cityPrefix = _faker.Address.CityPrefix();           // 城市前綴
        var citySuffix = _faker.Address.CitySuffix();           // 城市後綴
        var state = _faker.Address.State();                     // 州/省
        var stateAbbr = _faker.Address.StateAbbr();             // 州/省縮寫
        var zipCode = _faker.Address.ZipCode();                 // 郵遞區號
        var buildingNumber = _faker.Address.BuildingNumber();   // 建築號碼
        var streetName = _faker.Address.StreetName();           // 街道名稱
        var streetSuffix = _faker.Address.StreetSuffix();       // 街道後綴

        // 國家相關
        var country = _faker.Address.Country();                 // 國家
        var countryCode = _faker.Address.CountryCode();         // 國家代碼

        // 地理座標
        var latitude = _faker.Address.Latitude();               // 緯度
        var longitude = _faker.Address.Longitude();             // 經度
        var direction = _faker.Address.Direction();             // 方向 (North, South)
        var cardinalDirection = _faker.Address.CardinalDirection(); // 基本方向

        fullAddress.Should().NotBeNullOrEmpty();
        latitude.Should().BeInRange(-90, 90);
        longitude.Should().BeInRange(-180, 180);
    }

    #endregion

    #region Company DataSet

    /// <summary>
    /// Company DataSet - 公司資訊
    /// </summary>
    [Fact]
    public void CompanyDataSet_公司資訊()
    {
        var companyName = _faker.Company.CompanyName();        // 公司名稱
        var companySuffix = _faker.Company.CompanySuffix();    // 公司後綴 (Inc., LLC)
        var catchPhrase = _faker.Company.CatchPhrase();        // 標語
        var bs = _faker.Company.Bs();                          // 商業術語
        
        companyName.Should().NotBeNullOrEmpty();
        catchPhrase.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Commerce DataSet

    /// <summary>
    /// Commerce DataSet - 商業資訊
    /// </summary>
    [Fact]
    public void CommerceDataSet_商業資訊()
    {
        // 產品資訊
        var productName = _faker.Commerce.ProductName();           // 產品名稱
        var productAdjective = _faker.Commerce.ProductAdjective(); // 產品形容詞
        var productMaterial = _faker.Commerce.ProductMaterial();   // 產品材質
        var product = _faker.Commerce.Product();                   // 產品類型

        // 部門與類別
        var department = _faker.Commerce.Department();             // 部門
        var categories = _faker.Commerce.Categories(3);            // 多個類別

        // 價格（回傳字串格式）
        var price = _faker.Commerce.Price(1, 1000, 2);             // 價格字串
        var priceDecimal = _faker.Commerce.Price(1, 1000, 2, "$"); // 帶符號

        // 條碼
        var ean8 = _faker.Commerce.Ean8();                         // EAN-8 條碼
        var ean13 = _faker.Commerce.Ean13();                       // EAN-13 條碼

        // 顏色
        var color = _faker.Commerce.Color();                       // 顏色名稱

        productName.Should().NotBeNullOrEmpty();
        department.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Internet DataSet

    /// <summary>
    /// Internet DataSet - 網路資訊
    /// </summary>
    [Fact]
    public void InternetDataSet_網路資訊()
    {
        // 電子郵件
        var email = _faker.Internet.Email();                          // 隨機郵件
        var emailWithName = _faker.Internet.Email("john", "doe");     // 指定名稱
        var exampleEmail = _faker.Internet.ExampleEmail();            // example.com 郵件

        // 使用者名稱與密碼
        var userName = _faker.Internet.UserName();                    // 使用者名稱
        var userNameWithName = _faker.Internet.UserName("john", "doe"); // 指定名稱
        var password = _faker.Internet.Password();                    // 密碼
        var passwordLength = _faker.Internet.Password(16, false, "", "!@#"); // 自訂密碼

        // 網址
        var url = _faker.Internet.Url();                              // URL
        var urlWithProtocol = _faker.Internet.UrlWithPath();          // 帶路徑的 URL
        var domainName = _faker.Internet.DomainName();                // 網域名稱
        var domainWord = _faker.Internet.DomainWord();                // 網域單字
        var domainSuffix = _faker.Internet.DomainSuffix();            // 網域後綴

        // IP 地址
        var ip = _faker.Internet.Ip();                                // IPv4
        var ipv6 = _faker.Internet.Ipv6();                            // IPv6
        var mac = _faker.Internet.Mac();                              // MAC 地址

        // 其他
        var userAgent = _faker.Internet.UserAgent();                  // User Agent
        var protocol = _faker.Internet.Protocol();                    // 協定 (http/https)
        var port = _faker.Internet.Port();                            // 連接埠

        // Avatar
        var avatar = _faker.Internet.Avatar();                        // 頭像 URL

        email.Should().Contain("@");
        ip.Should().MatchRegex(@"^\d+\.\d+\.\d+\.\d+$");
    }

    #endregion

    #region Finance DataSet

    /// <summary>
    /// Finance DataSet - 金融資訊
    /// </summary>
    [Fact]
    public void FinanceDataSet_金融資訊()
    {
        // 信用卡
        var creditCardNumber = _faker.Finance.CreditCardNumber();     // 信用卡號
        var creditCardCvv = _faker.Finance.CreditCardCvv();           // CVV

        // 帳戶
        var account = _faker.Finance.Account();                       // 帳戶號碼
        var accountName = _faker.Finance.AccountName();               // 帳戶名稱
        var routingNumber = _faker.Finance.RoutingNumber();           // 路由號碼

        // 金額
        var amount = _faker.Finance.Amount(100, 10000, 2);            // 金額

        // 貨幣
        var currency = _faker.Finance.Currency();                     // 貨幣物件

        // 國際銀行
        var iban = _faker.Finance.Iban();                             // IBAN
        var bic = _faker.Finance.Bic();                               // BIC/SWIFT

        // 加密貨幣
        var bitcoinAddress = _faker.Finance.BitcoinAddress();         // 比特幣地址
        var ethereumAddress = _faker.Finance.EthereumAddress();       // 以太坊地址

        creditCardNumber.Should().NotBeNullOrEmpty();
        amount.Should().BeGreaterThan(0);
    }

    #endregion

    #region Date DataSet

    /// <summary>
    /// Date DataSet - 日期時間
    /// </summary>
    [Fact]
    public void DateDataSet_日期時間()
    {
        // 過去與未來
        var past = _faker.Date.Past();                               // 過去一年內
        var pastYears = _faker.Date.Past(5);                         // 過去 5 年內
        var future = _faker.Date.Future();                           // 未來一年內
        var futureYears = _faker.Date.Future(3);                     // 未來 3 年內

        // 最近與即將
        var recent = _faker.Date.Recent();                           // 最近幾天
        var recentDays = _faker.Date.Recent(7);                      // 最近 7 天
        var soon = _faker.Date.Soon();                               // 即將到來
        var soonDays = _faker.Date.Soon(14);                         // 14 天內

        // 範圍
        var between = _faker.Date.Between(
            DateTime.Now.AddYears(-1), 
            DateTime.Now);                                           // 範圍內日期

        // 生日（特定年齡範圍）
        var birthday = _faker.Date.Past(50, DateTime.Now.AddYears(-18)); // 18-68 歲

        // 時間相關
        var timespan = _faker.Date.Timespan();                       // TimeSpan
        var weekday = _faker.Date.Weekday();                         // 星期幾
        var month = _faker.Date.Month();                             // 月份名稱

        // DateTimeOffset
        var pastOffset = _faker.Date.PastOffset();                   // DateTimeOffset
        var futureOffset = _faker.Date.FutureOffset();

        past.Should().BeBefore(DateTime.Now);
        future.Should().BeAfter(DateTime.Now);
    }

    #endregion

    #region Lorem DataSet

    /// <summary>
    /// Lorem DataSet - 文字內容
    /// </summary>
    [Fact]
    public void LoremDataSet_文字內容()
    {
        // 單字
        var word = _faker.Lorem.Word();                              // 單字
        var words = _faker.Lorem.Words(5);                           // 多個單字

        // 句子
        var sentence = _faker.Lorem.Sentence();                      // 句子
        var sentenceWords = _faker.Lorem.Sentence(10);               // 10 個單字的句子
        var sentences = _faker.Lorem.Sentences(3);                   // 多個句子

        // 段落
        var paragraph = _faker.Lorem.Paragraph();                    // 段落
        var paragraphSentences = _faker.Lorem.Paragraph(5);          // 5 個句子的段落
        var paragraphs = _faker.Lorem.Paragraphs(2);                 // 多個段落

        // 文字區塊
        var text = _faker.Lorem.Text();                              // 文字區塊
        var lines = _faker.Lorem.Lines();                            // 多行文字

        // Slug
        var slug = _faker.Lorem.Slug();                              // URL-friendly 字串

        // 字母與字元
        var letter = _faker.Lorem.Letter();                          // 單一字母
        var letters = _faker.Lorem.Letter(10);                       // 多個字母

        word.Should().NotBeNullOrEmpty();
        sentence.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Phone DataSet

    /// <summary>
    /// Phone DataSet - 電話號碼
    /// </summary>
    [Fact]
    public void PhoneDataSet_電話號碼()
    {
        var phoneNumber = _faker.Phone.PhoneNumber();                // 電話號碼
        var phoneNumberFormat = _faker.Phone.PhoneNumberFormat();    // 格式化電話

        phoneNumber.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region System DataSet

    /// <summary>
    /// System DataSet - 系統相關
    /// </summary>
    [Fact]
    public void SystemDataSet_系統相關()
    {
        // 檔案相關
        var fileName = _faker.System.FileName();                     // 檔案名稱
        var commonFileName = _faker.System.CommonFileName();         // 常見檔案名稱
        var fileExt = _faker.System.FileExt();                       // 副檔名
        var commonFileExt = _faker.System.CommonFileExt();           // 常見副檔名

        // MIME 類型
        var mimeType = _faker.System.MimeType();                     // MIME 類型
        var commonFileType = _faker.System.CommonFileType();         // 常見檔案類型

        // 路徑
        var filePath = _faker.System.FilePath();                     // 檔案路徑
        var directoryPath = _faker.System.DirectoryPath();           // 目錄路徑

        // 版本
        var version = _faker.System.Version();                       // 版本號
        var semver = _faker.System.Semver();                         // 語意版本

        // Android
        var androidId = _faker.System.AndroidId();                   // Android ID

        // Apple
        var applePushToken = _faker.System.ApplePushToken();         // Apple Push Token

        fileName.Should().NotBeNullOrEmpty();
        mimeType.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Random DataSet

    /// <summary>
    /// Random DataSet - 隨機資料
    /// </summary>
    [Fact]
    public void RandomDataSet_隨機資料()
    {
        // 數值
        var randomInt = _faker.Random.Int(1, 100);                   // 整數
        var randomLong = _faker.Random.Long(1, 1000000);             // 長整數
        var randomDecimal = _faker.Random.Decimal(0, 1000);          // 小數
        var randomDouble = _faker.Random.Double(0, 100);             // 雙精度
        var randomFloat = _faker.Random.Float(0, 10);                // 單精度
        var randomByte = _faker.Random.Byte();                       // 位元組
        var randomShort = _faker.Random.Short(0, 1000);              // 短整數

        // 字元與字串
        var randomChar = _faker.Random.Char('a', 'z');               // 字元
        var randomString = _faker.Random.String(10);                 // 字串
        var randomString2 = _faker.Random.String2(10, "abc123");     // 指定字元集
        var alphanumeric = _faker.Random.AlphaNumeric(8);            // 英數字

        // 布林
        var randomBool = _faker.Random.Bool();                       // 布林
        var weightedBool = _faker.Random.Bool(0.8f);                 // 80% 為 true

        // GUID 與 Hash
        var randomGuid = _faker.Random.Guid();                       // GUID
        var randomUuid = _faker.Random.Uuid();                       // UUID
        var randomHash = _faker.Random.Hash();                       // Hash 值
        var randomHexadecimal = _faker.Random.Hexadecimal(16);       // 十六進位

        // 列舉
        var randomEnum = _faker.Random.Enum<DayOfWeek>();            // 隨機列舉

        // 集合操作
        var array = new[] { "A", "B", "C", "D", "E" };
        var randomElement = _faker.Random.ArrayElement(array);       // 陣列隨機元素
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var randomListItem = _faker.Random.ListItem(list);           // 清單隨機元素
        var shuffled = _faker.Random.Shuffle(array);                 // 洗牌

        // 取多個
        var randomElements = _faker.Random.ArrayElements(array, 3);  // 取 3 個

        randomInt.Should().BeInRange(1, 100);
        randomGuid.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region Vehicle DataSet

    /// <summary>
    /// Vehicle DataSet - 車輛資訊
    /// </summary>
    [Fact]
    public void VehicleDataSet_車輛資訊()
    {
        var manufacturer = _faker.Vehicle.Manufacturer();            // 製造商
        var model = _faker.Vehicle.Model();                          // 型號
        var type = _faker.Vehicle.Type();                            // 類型
        var fuel = _faker.Vehicle.Fuel();                            // 燃料類型
        var vin = _faker.Vehicle.Vin();                              // 車輛識別號

        manufacturer.Should().NotBeNullOrEmpty();
        vin.Should().HaveLength(17);
    }

    #endregion

    #region Image DataSet

    /// <summary>
    /// Image DataSet - 圖片 URL
    /// </summary>
    [Fact]
    public void ImageDataSet_圖片URL()
    {
        var imageUrl = _faker.Image.PicsumUrl();                     // Picsum 圖片
        var loremFlickr = _faker.Image.LoremFlickrUrl();             // LoremFlickr
        var placeholder = _faker.Image.PlaceholderUrl();             // 佔位圖

        // 特定類別圖片
        var abstract_ = _faker.Image.Abstract();
        var animals = _faker.Image.Animals();
        var business = _faker.Image.Business();
        var cats = _faker.Image.Cats();
        var city = _faker.Image.City();
        var food = _faker.Image.Food();
        var nightlife = _faker.Image.Nightlife();
        var fashion = _faker.Image.Fashion();
        var people = _faker.Image.People();
        var nature = _faker.Image.Nature();
        var sports = _faker.Image.Sports();
        var technics = _faker.Image.Technics();
        var transport = _faker.Image.Transport();

        imageUrl.Should().StartWith("http");
    }

    #endregion

    #region Rant DataSet

    /// <summary>
    /// Rant DataSet - 評論與抱怨
    /// </summary>
    [Fact]
    public void RantDataSet_評論()
    {
        var review = _faker.Rant.Review();                           // 產品評論
        var reviews = _faker.Rant.Reviews(3);                        // 多個評論

        review.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Hacker DataSet

    /// <summary>
    /// Hacker DataSet - 技術術語
    /// </summary>
    [Fact]
    public void HackerDataSet_技術術語()
    {
        var abbreviation = _faker.Hacker.Abbreviation();             // 縮寫 (TCP, HTTP)
        var adjective = _faker.Hacker.Adjective();                   // 形容詞
        var noun = _faker.Hacker.Noun();                             // 名詞
        var verb = _faker.Hacker.Verb();                             // 動詞
        var ingverb = _faker.Hacker.IngVerb();                       // ing 動詞
        var phrase = _faker.Hacker.Phrase();                         // 技術短語

        phrase.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Database DataSet

    /// <summary>
    /// Database DataSet - 資料庫相關
    /// </summary>
    [Fact]
    public void DatabaseDataSet_資料庫()
    {
        var column = _faker.Database.Column();                       // 欄位名稱
        var type = _faker.Database.Type();                           // 資料類型
        var collation = _faker.Database.Collation();                 // 排序規則
        var engine = _faker.Database.Engine();                       // 資料庫引擎

        column.Should().NotBeNullOrEmpty();
    }

    #endregion
}
