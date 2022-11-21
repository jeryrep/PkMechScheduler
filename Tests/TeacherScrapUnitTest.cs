using MechScraper.Enums;
using MechScraper.Models;

namespace MechScraper.Tests;

[TestClass]
public class TeacherScrapUnitTest
{
    [TestMethod]
    public void CheckJPietraszekOddWeekSchedule()
    {
        var list = Parser.ConvertDocumentToBlockList(Scraper.ScrapSchedule("n351.html").Result, Mode.Teacher);
        Assert.IsNotNull(list);
        Assert.AreEqual(8, list.Count);
    }
    [TestMethod]
    public void CheckJPietraszekEvenWeekSchedule()
    {
        var list = Parser.ConvertDocumentToBlockList(Scraper.ScrapSchedule("n352.html").Result, Mode.Teacher);
        Assert.IsNotNull(list);
        Assert.AreEqual(7, list.Count);
    }

    [TestMethod]
    public void CheckJPietraszekBothWeekSchedule()
    {
        var documents = Scraper.ScrapSchedules(new[] { "n351.html", "n352.html" });
        var list = Parser.ConvertDocumentsToBlockList(documents, Mode.Teacher);
        Assert.IsNotNull(list);
        Assert.AreEqual(13, list.Count);
    }

    [TestMethod]
    public void CheckMAntkowiczBothWeekSchedule()
    {
        var documents = Scraper.ScrapSchedules(new[] { "n19.html", "n20.html" });
        var list = Parser.ConvertDocumentsToBlockList(documents, Mode.Teacher);
        Assert.IsNotNull(list);
        Assert.AreEqual(8, list.Count);
    }
}