using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Microsoft.EntityFrameworkCore;
using PkMechScheduler.Database;
using PkMechScheduler.Database.Models;
using PkMechScheduler.Frontend.Enums;
using PkMechScheduler.Frontend.Interfaces;
using Group = PkMechScheduler.Database.Models.Group;

namespace PkMechScheduler.Frontend.Services;

public class SerializerService : ISerializerService
{
    private readonly SchedulerContext _context;
    public SerializerService(SchedulerContext context) => _context = context;

    public async Task AddScheduleToDb(IDocument document)
    {
        var groupNumber = Regex.Replace(document.QuerySelector("span.tytulnapis")?.InnerHtml!, "<.*?>", string.Empty)
            .Last();
        var table = document.QuerySelector("table.tabela");
        var rows = table?.QuerySelectorAll("tr");
        var list = new List<BlockModel>();
        foreach (var row in rows!.Skip(1).Select((row, i) => (row, i)))
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
                    if (name.Length == 1) name = string.Join(" ", name, textBlocks[1]);
                    var group = string.Empty;
                    bool? evenWeek = null;
                    var initials = string.Empty;

                    foreach (var textBlock in textBlocks)
                    {
                        if (textBlock is "-P" or "P-" && group != string.Empty)
                            initials = textBlock;
                        else if (Regex.IsMatch(textBlock, "^[(][MK][)]") ||
                                 Regex.IsMatch(textBlock, "[WKLĆSP]0?[0-9]?-"))
                        {
                            var groupIndex = textBlock.LastIndexOf("-", StringComparison.Ordinal);
                            group = groupIndex >= 0 ? textBlock[..groupIndex] : textBlock;
                            switch (group[0])
                            {
                                case (char)SubjectType.Lecture:
                                    group = $"{group}01";
                                    break;
                                case (char)SubjectType.Seminars:
                                case (char)SubjectType.Exercise:
                                    group = $"{group}0{groupNumber}";
                                    break;
                            }

                            if (textBlock.Contains("-P") || textBlock.Contains("-(P") || textBlock.Contains("-p"))
                                evenWeek = true;
                            else if (textBlock.Contains("-N") || textBlock.Contains("-(N") || textBlock.Contains("-n"))
                                evenWeek = false;
                        }
                        else if (textBlock.StartsWith("#") || textBlock.Length == 2) initials = textBlock;
                    }

                    var placeIndex = textBlocks.LastOrDefault()!.LastIndexOf("-", StringComparison.Ordinal);
                    var place = placeIndex >= 0
                        ? textBlocks.LastOrDefault()?[..placeIndex]
                        : textBlocks.LastOrDefault();

                    var blockModel = new BlockModel
                    {
                        Number = (byte)row.i,
                        Start = TimeSpan.Parse(timeSpan!.FirstOrDefault()!),
                        End = TimeSpan.Parse(timeSpan!.LastOrDefault()!),
                        DayOfWeek = (DayOfWeek)block.j + 1,
                        Blocks = 1,
                        Name = name,
                        Group = group,
                        EvenWeek = evenWeek,
                        Initials = initials,
                        Place = place
                    };
                    var possibleDuplicate = list.Find(x =>
                        x.DayOfWeek == blockModel.DayOfWeek && x.Name == name && x.Group == group && x.Place == place &&
                        x.EvenWeek == evenWeek);

                    if (possibleDuplicate != null)
                        possibleDuplicate.Blocks++;
                    else
                        list.Add(blockModel);
                }
            }
        }

        foreach (var blockModel in list.Select(blockModel => new
                     {
                         blockModel,
                         possibleDuplicate = _context.Blocks.ToListAsync()
                             .Result.Find(x =>
                                 x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name &&
                                 x.Group == blockModel.Group && x.Place == blockModel.Place &&
                                 x.EvenWeek == blockModel.EvenWeek)
                     })
                     .Where(t => t.possibleDuplicate == null)
                     .Select(t => t?.blockModel)) await _context.Blocks.AddAsync(blockModel);

        await _context.SaveChangesAsync();
    }

    public async Task AddGroupsToDb(IDocument document)
    {
        var paragraphs = document.QuerySelector("div#oddzialy")?.QuerySelectorAll("p");
        foreach (var paragraph in paragraphs!.Select(x => x.InnerHtml))
        {
            var pFrom = paragraph.IndexOf("/", StringComparison.Ordinal);
            var pTo = paragraph[(pFrom + 1)..].IndexOf("\"", StringComparison.Ordinal);
            await _context.Groups.AddAsync(new Group
            {
                Name = Regex.Replace(paragraph, "<.*?>", string.Empty),
                Link = paragraph.Substring(pFrom + 1, pTo)
            });
        }

        await _context.SaveChangesAsync();
    }
}