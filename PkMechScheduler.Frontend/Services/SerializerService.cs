using System.Text.RegularExpressions;
using PkMechScheduler.Frontend.Models;
using IElement = AngleSharp.Dom.IElement;

namespace PkMechScheduler.Frontend.Services;

public class SerializerService
{
    public Dictionary<Day, List<BlockModel>> SerializeScheduleToJson(IElement element)
    {
        var rows = element.QuerySelectorAll("tr");
        var schedule = new Dictionary<Day, List<BlockModel>>
        {
            { Day.Monday, new List<BlockModel>() },
            { Day.Tuesday, new List<BlockModel>() },
            { Day.Wednesday, new List<BlockModel>() },
            { Day.Thursday, new List<BlockModel>() },
            { Day.Friday, new List<BlockModel>() }
        };
        foreach (var row in rows.Skip(1).Select((row, i) => (row, i)))
        {
            var timeSpan = row.row.QuerySelector("td.g")?.InnerHtml.Split("-");
            foreach (var block in row.row.QuerySelectorAll("td.l").Select((cell, j) => (cell, j)))
            {
                if (block.cell.InnerHtml.Contains("&nbsp;"))
                    continue;
                foreach (var subject in block.cell.InnerHtml.Split("<br>"))
                {
                    var textBlocks = Regex.Replace(subject, "<.*?>", string.Empty).Split(" ");

                    var index = textBlocks.FirstOrDefault()!.LastIndexOf("-", StringComparison.Ordinal);
                    var name = index >= 0 ? textBlocks.FirstOrDefault()?[..index] : textBlocks.FirstOrDefault();
                    var group = string.Empty;
                    bool? evenWeek = null;
                    var initials = string.Empty;


                    foreach (var textBlock in textBlocks)
                    {
                        if (Regex.IsMatch(textBlock, "^[(][MK][)]") || Regex.IsMatch(textBlock, "[WKLĆSP]0?[0-9]?-"))
                        {
                            var groupIndex = textBlock.LastIndexOf("-", StringComparison.Ordinal);
                            group = groupIndex >= 0 ? textBlock[..groupIndex] : textBlock;
                            if (textBlock.Contains("-P") || textBlock.Contains("-(P"))
                                evenWeek = true;
                            else if (textBlock.Contains("-N") || textBlock.Contains("-(N"))
                                evenWeek = false;
                        }

                        if (textBlock.StartsWith("#") || textBlock.Length == 2) initials = textBlock;
                    }

                    var placeIndex = textBlocks.LastOrDefault()!.LastIndexOf("-", StringComparison.Ordinal);
                    var place = placeIndex >= 0 ? textBlocks.LastOrDefault()?[..placeIndex] : textBlocks.LastOrDefault();

                    var blockModel = new BlockModel
                    {
                        Number = (byte)row.i,
                        Start = TimeSpan.Parse(timeSpan!.FirstOrDefault()!),
                        End = TimeSpan.Parse(timeSpan!.LastOrDefault()!),
                        Blocks = 1,
                        Name = name,
                        Group = group,
                        EvenWeek = evenWeek,
                        Initials = initials,
                        Place = place
                    };

                    var possibleDuplicate = schedule.First(x => x.Key == (Day)block.j).Value
                        .Find(x => x.Name == name && x.Group == group && x.Place == place && x.EvenWeek == evenWeek);

                    /*if (possibleDuplicate != null)
                    {
                        possibleDuplicate.Blocks++;
                    }
                    else
                    {*/
                    schedule.First(x => x.Key == (Day)block.j).Value.Add(blockModel);
                    //} 
                }
            }
        }
        return schedule;
    }

    public Dictionary<string, string> SerializeGroups(IElement table)
    {
        var groups = new Dictionary<string, string>();
        var paragraphs = table.QuerySelectorAll("p");
        foreach (var paragraph in paragraphs.Select(x => x.InnerHtml))
        {
            var pFrom = paragraph.IndexOf("/", StringComparison.Ordinal);
            var pTo = paragraph[(pFrom + 1)..].IndexOf("\"", StringComparison.Ordinal);
            groups.Add(Regex.Replace(paragraph, "<.*?>", string.Empty), paragraph.Substring(pFrom + 1, pTo));
        }
        return groups;
    }
}