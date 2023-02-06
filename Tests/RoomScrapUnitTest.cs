using MechScraper.Enums;
using MechScraper.Models;

namespace MechScraper.Tests;

[TestClass]
public class RoomScrapUnitTest
{
    [TestMethod]
    public void CheckK123EvenWeekRoomBlockCount()
    {
        var list = Parser.ConvertDocumentToBlockList(Scraper.ScrapSchedule("s48.html").Result, Mode.DeansOffice);
        Assert.IsNotNull(list);
        Assert.AreEqual(3, list.Count);
    }

    [TestMethod]
    public void CheckK123OddWeeksRoomBlockCount()
    {
        var list = Parser.ConvertDocumentToBlockList(Scraper.ScrapSchedule("s47.html").Result, Mode.DeansOffice);
        Assert.IsNotNull(list);
        Assert.AreEqual(5, list.Count);
    }

    [TestMethod]
    public void CheckK123BothWeekRoomBlockCount()
    {
        var documents = Scraper.ScrapSchedules(new[] { "s47.html", "s48.html" });
        var list = Parser.ConvertDocumentsToBlockList(documents, Mode.DeansOffice).OfType<RoomBlock>().ToList();
        Assert.IsNotNull(list);
        Assert.AreEqual(8, list.Count);
    }

    [TestMethod]
    public void CheckG120BothWeekRoomBlockCount()
    {
        var documents = Scraper.ScrapSchedules(new[] { "s214.html", "s215.html" });
        var list = Parser.ConvertDocumentsToBlockList(documents, Mode.DeansOffice);
        Assert.IsNotNull(list);
        Assert.AreEqual(35, list.Count);
    }
}