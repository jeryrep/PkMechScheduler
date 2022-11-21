using MechScraper.Enums;

namespace MechScraper.Tests;

[TestClass]
public class CourseScrapUnitTest
{
    [TestMethod]
    public void Check14BSchedule()
    {
        var list = Parser.ConvertDocumentToBlockList(Scraper.ScrapSchedule("o64.html").Result, Mode.Student);
        Assert.IsNotNull(list);
        Assert.AreEqual(14, list.Count);
    }
}