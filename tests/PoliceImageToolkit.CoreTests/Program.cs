using PoliceImageToolkit.Services;

var tests = new (string Name, Action Execute)[]
{
    ("長截圖依比例與重疊計算分頁", LongScreenshot_CalculatesExpectedPages),
    ("長截圖空白尺寸不產生分頁", LongScreenshot_ReturnsEmptyForEmptySource),
    ("長截圖拒絕無效圖框設定", LongScreenshot_RejectsInvalidOptions),
    ("版本比較可辨識新舊版本", UpdateService_ComparesSemanticVersions)
};

int failed = 0;
foreach (var test in tests)
{
    try
    {
        test.Execute();
        Console.WriteLine($"PASS: {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL: {test.Name}\n{ex.Message}");
    }
}

Console.WriteLine($"完成：{tests.Length - failed}/{tests.Length} 項測試通過。");
return failed == 0 ? 0 : 1;

static void LongScreenshot_CalculatesExpectedPages()
{
    var service = new LongScreenshotService();
    var options = new LongScreenshotSplitOptions(8, 17.5, 5, "unused");
    var pages = service.CalculatePages(800, 3500, options);

    AssertEqual(3, pages.Count, "頁數");
    AssertPage(pages[0], 1, 0, 1750, 0, 50);
    AssertPage(pages[1], 2, 1700, 1750, 50, 50);
    AssertPage(pages[2], 3, 3400, 100, 50, 0);
}

static void LongScreenshot_ReturnsEmptyForEmptySource()
{
    var service = new LongScreenshotService();
    var options = new LongScreenshotSplitOptions(8, 17.5, 5, "unused");

    AssertEqual(0, service.CalculatePages(0, 500, options).Count, "寬度為零");
    AssertEqual(0, service.CalculatePages(500, 0, options).Count, "高度為零");
}

static void LongScreenshot_RejectsInvalidOptions()
{
    var service = new LongScreenshotService();
    AssertThrows<ArgumentOutOfRangeException>(() =>
        service.CalculatePages(500, 1000, new LongScreenshotSplitOptions(0, 17.5, 5, "unused")));
    AssertThrows<ArgumentOutOfRangeException>(() =>
        service.CalculatePages(500, 1000, new LongScreenshotSplitOptions(8, 17.5, -1, "unused")));
}

static void UpdateService_ComparesSemanticVersions()
{
    AssertTrue(UpdateService.CompareVersions("v11.3.0", "v11.2.0") > 0, "新版應大於舊版");
    AssertTrue(UpdateService.CompareVersions("v11.2.0-beta", "11.2.0") == 0, "預發布後綴應採相同核心版本");
    AssertTrue(UpdateService.CompareVersions("v11.1.9", "v11.2.0") < 0, "舊版應小於新版");
}

static void AssertPage(PoliceImageToolkit.Models.LongScreenshotPage page, int number, int sourceY, int height, int overlapTop, int overlapBottom)
{
    AssertEqual(number, page.PageNumber, "頁碼");
    AssertEqual(sourceY, page.SourceY, "起始 Y");
    AssertEqual(height, page.PixelHeight, "頁高");
    AssertEqual(overlapTop, page.OverlapTopPixels, "上方重疊");
    AssertEqual(overlapBottom, page.OverlapBottomPixels, "下方重疊");
}

static void AssertEqual<T>(T expected, T actual, string name) where T : IEquatable<T>
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{name}預期為 {expected}，實際為 {actual}。");
    }
}

static void AssertTrue(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void AssertThrows<TException>(Action action) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"預期應拋出 {typeof(TException).Name}。");
}
