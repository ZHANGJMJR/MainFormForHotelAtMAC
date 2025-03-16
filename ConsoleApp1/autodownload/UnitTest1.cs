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
                Args = new[] { "--lang=zh-CN" }  } );
        
        var page = await browser.NewPageAsync();
        
        await page.GotoAsync(url);
        await page.ScreenshotAsync(new PageScreenshotOptions{Path = "EAAP.jpg"});
        await page
            .GetByPlaceholder("请输入登录帐号")
            .FillAsync(user);
        Task.Delay(2000).Wait();
        await page.GetByPlaceholder("请输入登录密码").FillAsync(password);
        Task.Delay(2000).Wait();
        //await page.GetByLabel("登 录").ClickAsync();
        await page.ClickAsync("text=Login");
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