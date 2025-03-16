using System.Runtime.InteropServices.JavaScript;
using Microsoft.Playwright;

namespace autodownload;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Test1()
    {
     string url = "https://sp.fuioupay.com/login";
     string detailsurl = "https://sp.fuioupay.com/count/commodity/sales/details/details";
     string filedownloadurl = "https://sp.fuioupay.com/download/doc";
     string user = "MX4351344";
     string password = "719720";
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
            { Headless = false,
                Args = new List<string> { "--start-maximized","--lang=zh-CN" } } );
        
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize(){Width = 1440, Height = 900},
            Permissions = new[] { "geolocation" },
            Locale = "zh-CN",
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Accept-Language"] = "zh-CN,cn;q=0.9"
            },
            Geolocation = new Geolocation
            {
                Longitude = 116.4f,
                Latitude = 39.9024f,
                Accuracy = 100,
            },
            RecordVideoDir ="videos/",
            RecordVideoSize = new RecordVideoSize()  { Width = 1440, Height = 900 },
        });

        var page = await context.NewPageAsync();
        var path = await page.Video.PathAsync();
        await page.GotoAsync(url);
        await page.ScreenshotAsync(new PageScreenshotOptions{Path = "EAAP.jpg"});
        await page
            .GetByPlaceholder("请输入登录帐号")
            .FillAsync(user);
        Task.Delay(2000).Wait();
        await page.GetByPlaceholder("请输入登录密码").FillAsync(password);
        Task.Delay(2000).Wait();
        //await page.GetByLabel("登 录").ClickAsync();
        await page.ClickAsync("text=登 录");
        // await page.FillAsync("#UserName","admin");
        // await page.FillAsync("#Password","password");
        Task.Delay(1000).Wait();
        await page.GotoAsync(detailsurl);
        Task.Delay(5000).Wait();
        await page.ScreenshotAsync(new PageScreenshotOptions{Path = "EAAP2.jpg"});
        await page.GotoAsync(filedownloadurl);
        Task.Delay(5000).Wait();
        await page.ScreenshotAsync(new PageScreenshotOptions{Path = "EAAP3.jpg"});
        
    }
}