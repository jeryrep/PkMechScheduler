using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Microsoft.EntityFrameworkCore;
using PkMechScheduler.Database;
using PkMechScheduler.Database.Models;
using PkMechScheduler.Frontend.Enums;
using PkMechScheduler.Frontend.Interfaces;
using Group = PkMechScheduler.Database.Models.Group;
using IElement = AngleSharp.Dom.IElement;

namespace PkMechScheduler.Frontend.Services;

public partial class SerializerService : ISerializerService
{
    private readonly SchedulerContext _context;
    public SerializerService(SchedulerContext context) => _context = context;

    public async Task AddScheduleToDb(IDocument document)
    {
        var groupNumber = ClearTagRegex().Replace(document.QuerySelector("span.tytulnapis")?.InnerHtml!, string.Empty)
            .Last();
        var table = document.QuerySelector("table.tabela");
        var rows = table?.QuerySelectorAll("tr");
        var list = new List<BlockModel>();
        foreach (var row in rows!.Skip(1).Select((row, i) => (row, i))) HandleRow(row, groupNumber, list);

        UnifyBothWeekSubjects(list);

        await SaveUniqueBlocks(list);
    }

    private async Task SaveUniqueBlocks(IEnumerable<BlockModel> list)
    {
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
                     .Select(t => t?.blockModel)) await _context.Blocks.AddAsync(blockModel!);
        await _context.SaveChangesAsync();
    }

    private static void UnifyBothWeekSubjects(List<BlockModel> list)
    {
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var blockModel = list[i];
            if (blockModel.Name is "J angielski" or "WF") continue;
            var doubleWeek = list.Find(x =>
                x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name &&
                x.Group == blockModel.Group && x.Place == blockModel.Place &&
                x.EvenWeek != blockModel.EvenWeek);
            if (doubleWeek == null) continue;
            doubleWeek.EvenWeek = null;
            list.Remove(blockModel);
        }
    }

    private static void HandleRow((IElement row, int i) row, char groupNumber, List<BlockModel> list)
    {
        var timeSpan = row.row.QuerySelector("td.g")?.InnerHtml.Split("-");
        foreach (var block in row.row.QuerySelectorAll("td.l").Select((cell, j) => (cell, j)))
            HandleBlock(row, groupNumber, list, block, timeSpan);
    }

    private static void HandleBlock((IElement row, int i) row, char groupNumber, List<BlockModel> list,
        (IElement cell, int j) block, string[] timeSpan)
    {
        if (block.cell.InnerHtml.Contains("&nbsp;"))
            return;
        foreach (var subject in block.cell.InnerHtml.Split("<br>"))
        {
            var textBlocks = ClearTagRegex().Replace(subject, string.Empty).Split(" ");

            var index = textBlocks.FirstOrDefault()!.LastIndexOf("-", StringComparison.Ordinal);
            var name = index >= 0 ? textBlocks.FirstOrDefault()?[..index] : textBlocks.FirstOrDefault();
            var group = string.Empty;
            bool? evenWeek = null;
            var initials = string.Empty;
            switch (name)
            {
                case "J":
                    var nameSplit = textBlocks[1].Split("-");
                    name = string.Join(" ", name, nameSplit.First());
                    group = ((char)SubjectType.Exercise).ToString();
                    evenWeek = nameSplit.Last().Contains('P');
                    initials = textBlocks[2];
                    break;
                case "WF":
                    group = textBlocks[1][1].ToString();
                    initials = textBlocks[3];
                    break;
                default:
                {
                    foreach (var textBlock in textBlocks)
                    {
                        if (textBlock is "-P" or "P-" && group != string.Empty)
                            initials = textBlock;
                        else if (PeGenderRegex().IsMatch(textBlock) ||
                                 GroupRegex().IsMatch(textBlock))
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

                    break;
                }
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
                x.EvenWeek == evenWeek && x.Initials!.Equals(initials, StringComparison.OrdinalIgnoreCase));

            if (possibleDuplicate != null)
                possibleDuplicate.Blocks++;
            else
                list.Add(blockModel);
        }
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
                Name = ClearTagRegex().Replace(paragraph, string.Empty),
                Link = paragraph.Substring(pFrom + 1, pTo)
            });
        }

        var teachers = document.QuerySelector("div#nauczyciele")?.QuerySelectorAll("p");
        foreach (var teacher in teachers!.Select(x => x.InnerHtml))
        {
            var pFrom = teacher.IndexOf("/", StringComparison.Ordinal);
            var pTo = teacher[(pFrom + 1)..].IndexOf("\"", StringComparison.Ordinal);
            var innerTexts = ClearTagRegex().Replace(teacher, string.Empty).Split(" ");
            var index = innerTexts.FirstOrDefault()!.LastIndexOf("-", StringComparison.Ordinal);
            var name = index >= 0 ? innerTexts.FirstOrDefault()?[..index] : innerTexts.FirstOrDefault();
            await _context.Teachers.AddAsync(new Teacher
            {
                Name = name,
                Link = teacher.Substring(pFrom + 1, pTo),
                EvenWeek = innerTexts.FirstOrDefault()!.LastOrDefault() == 'p'
            });
        }

        var rooms = document.QuerySelector("div#sale")?.QuerySelectorAll("p");
        foreach (var room in rooms!.Select(x => x.InnerHtml).Skip(1))
        {
            var innerText = ClearTagRegex().Replace(room, string.Empty);
            var innerTexts = innerText.Split(" ").ToList();
            if ((innerTexts.Count == 1 && (innerTexts.FirstOrDefault()!.LastOrDefault() is not 'p' and not 'n' ||
                                            innerTexts.FirstOrDefault()!.Contains("--------"))) ||
                (innerTexts.Count == 2 && OrganizationalUnitRegex().IsMatch(innerTexts.LastOrDefault()!) && 
                 !OrganizationalUnitRegex().IsMatch(innerTexts.FirstOrDefault()!))) continue;
            var pFrom = room.IndexOf("/", StringComparison.Ordinal);
            var pTo = room[(pFrom + 1)..].IndexOf("\"", StringComparison.Ordinal);

            var index = innerTexts.FirstOrDefault()!.LastIndexOf("-", StringComparison.Ordinal);
            var name = index >= 0 ? innerTexts.FirstOrDefault()?[..index] : innerTexts.FirstOrDefault();
            var organizationalUnit = "WM";
            if (innerTexts.Count == 1) 
                organizationalUnit = name;
            else if (OrganizationalUnitRegex().IsMatch(innerTexts[1])) 
                organizationalUnit = innerTexts[1];
            var description = string.Empty;
            if (innerText.Contains("w tyg."))
            {
                innerTexts.RemoveRange(innerTexts.Count - 4, 3);
                var stringBuilder = new StringBuilder();
                foreach (var text in innerTexts.Skip(1))
                {
                    if (OrganizationalUnitRegex().IsMatch(text)) continue;
                    stringBuilder.Append($"{text} ");
                }
                description = stringBuilder.ToString();
            }
            await _context.Rooms.AddAsync(new Room
            {
                Name = name,
                Description = description,
                Link = room.Substring(pFrom + 1, pTo),
                OrganizationalUnit = organizationalUnit,
                EvenWeek = innerTexts.FirstOrDefault()!.LastOrDefault() == 'p'
            });
        }

        await _context.SaveChangesAsync();
    }

    [GeneratedRegex("<.*?>")]
    private static partial Regex ClearTagRegex();

    [GeneratedRegex("^M-?[1-9]")]
    private static partial Regex OrganizationalUnitRegex();

    [GeneratedRegex("^[(][MK][)]")]
    private static partial Regex PeGenderRegex();

    [GeneratedRegex("[WKLĆSP]0?[0-9]?-")]
    private static partial Regex GroupRegex();
}