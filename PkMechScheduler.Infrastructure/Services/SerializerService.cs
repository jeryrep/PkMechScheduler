using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Microsoft.EntityFrameworkCore;
using PkMechScheduler.Database;
using PkMechScheduler.Database.Enums;
using PkMechScheduler.Database.Models;
using PkMechScheduler.Infrastructure.Interfaces;
using Group = PkMechScheduler.Database.Models.Group;

namespace PkMechScheduler.Infrastructure.Services;

public class SerializerService : ISerializerService
{
    private readonly SchedulerContext _context;
    public SerializerService(SchedulerContext context) => _context = context;

    public async Task ConvertDocumentToBlockList(IDocument document, Preference mode)
    {
        var groupNumber = Regex.Replace(document.QuerySelector("span.tytulnapis")?.InnerHtml!, "<.*?>", string.Empty).Last();
        var table = document.QuerySelector("table.tabela");
        var rows = table?.QuerySelectorAll("tr");
        var list = new List<BaseBlock>();
        foreach (var row in rows!.Skip(1).Select((row, i) => (row, i)))
            HandleRow(row.row, row.i, groupNumber, list, mode);

        UnifyBothWeekSubjects(list);

        await SaveUniqueBlocks(list, mode);
    }

    public async Task ConvertDocumentsToBlockList(IEnumerable<IDocument> documents, Preference mode)
    {
        foreach (var document in documents) await ConvertDocumentToBlockList(document, mode);
    }

    private async Task SaveUniqueBlocks(IEnumerable<BaseBlock> list, Preference mode)
    {
        switch (mode)
        {
            case Preference.Student:
                foreach (var blockModel in list.Select(block => new
                             {
                                 blockModel = (StudentBlock)block,
                                 possibleDuplicate = _context.StudentBlocks.ToListAsync()
                                     .Result.Find(x =>
                                         x.DayOfWeek == block.DayOfWeek && x.Name == block.Name &&
                                         x.Group == block.Group && x.Place == block.Place &&
                                         x.EvenWeek == block.EvenWeek)
                             })
                             .Where(t => t.possibleDuplicate == null)
                             .Select(t => t?.blockModel)) await _context.StudentBlocks.AddAsync(blockModel!);
                break;
            case Preference.Teacher:
                foreach (var blockModel in list.Select(block => new
                             {
                                 blockModel = (TeacherBlock)block,
                                 possibleDuplicate = _context.TeacherBlocks.ToListAsync()
                                     .Result.Find(x =>
                                         x.DayOfWeek == block.DayOfWeek && x.Name == block.Name &&
                                         x.Group == block.Group && x.Place == block.Place &&
                                         x.EvenWeek == block.EvenWeek && x.Courses == ((TeacherBlock)block).Courses)
                             })
                             .Where(t => t.possibleDuplicate == null)
                             .Select(t => t.blockModel)) await _context.TeacherBlocks.AddAsync(blockModel);
                break;
        }

        await _context.SaveChangesAsync();
    }

    private static void UnifyBothWeekSubjects(List<BaseBlock> list)
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

    private static void HandleRow(IParentNode row, int i, char groupNumber, List<BaseBlock> list, Preference mode)
    {
        var timeSpan = row.QuerySelector("td.g")?.InnerHtml.Split("-");
        foreach (var block in row.QuerySelectorAll("td.l").Select((cell, j) => (cell, j)))
        {
            if (block.cell.InnerHtml.Contains("&nbsp;"))
                continue;
            switch (mode)
            {
                case Preference.Student:
                    HandleBlock(i, groupNumber, list, block.cell, block.j, timeSpan!);
                    break;
                case Preference.Teacher:
                    HandleTeacherBlock(i, list, block.cell, block.j, timeSpan!);
                    break;
            }
        }
    }

    private static void HandleTeacherBlock(int i, List<BaseBlock> list, IElement blockCell, int j, string[] timeSpan)
    {
        var textBlocks = Regex.Replace(blockCell.InnerHtml, "<.*?>", string.Empty).Split(" ");
        if (!Regex.IsMatch(textBlocks.FirstOrDefault()!, "^[1-9]{2}"))
        {
            var simpleBlockModel = new TeacherBlock
            {
                Number = (byte)i,
                Start = TimeSpan.Parse(timeSpan.FirstOrDefault()!),
                End = TimeSpan.Parse(timeSpan.LastOrDefault()!),
                DayOfWeek = (DayOfWeek)j + 1,
                Blocks = 1,
                Description = string.Join(" ", textBlocks)
            };
            var duplicate = list.Find(x =>
                x.DayOfWeek == simpleBlockModel.DayOfWeek &&
                ((TeacherBlock)x).Description == simpleBlockModel.Description);
            if (duplicate != null) duplicate.Blocks++;
            else list.Add(simpleBlockModel);
            return;
        }

        var name = textBlocks[1];
        var group = textBlocks[^2];
        switch (name)
        {
            case "J":
                name = "J angielski";
                group = "Ć";
                break;
            case "Met":
                name = "Met dośw";
                break;
        }

        var blockModel = new TeacherBlock
        {
            Number = (byte)i,
            Start = TimeSpan.Parse(timeSpan.FirstOrDefault()!),
            End = TimeSpan.Parse(timeSpan.LastOrDefault()!),
            DayOfWeek = (DayOfWeek)j + 1,
            Blocks = 1,
            Name = name,
            Group = group,
            EvenWeek = textBlocks.LastOrDefault()?.Last() == 'p',
            Courses = ParseStringWithDash(textBlocks.FirstOrDefault()?.Split(",")!),
            Place = ParseStringWithDash(textBlocks.LastOrDefault()!)
        };
        var possibleDuplicate = list.Find(x =>
            x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name && x.Group == blockModel.Group &&
            x.Place == blockModel.Place &&
            x.EvenWeek == blockModel.EvenWeek &&
            ((TeacherBlock)x).Courses!.Equals(blockModel.Courses, StringComparison.OrdinalIgnoreCase) &&
            x.Number == blockModel.Number - x.Blocks);

        if (possibleDuplicate != null) possibleDuplicate.Blocks++;
        else list.Add(blockModel);
    }

    private static void HandleBlock(int i, char groupNumber, List<BaseBlock> list, IElement cell, int j, string[] timeSpan)
    {
        foreach (var subject in cell.InnerHtml.Split("<br>"))
        {
            var textBlocks = Regex.Replace(subject, "<.*?>", string.Empty).Split(" ");

            var name = ParseStringWithDash(textBlocks.FirstOrDefault()!);
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
                        else if (Regex.IsMatch(textBlock, "^[(][MK][)]") || Regex.IsMatch(textBlock, "[WKLĆSP]0?[0-9]?-"))
                        {
                            group = ParseStringWithDash(textBlock);
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

            var blockModel = new StudentBlock
            {
                Number = (byte)i,
                Start = TimeSpan.Parse(timeSpan.FirstOrDefault()!),
                End = TimeSpan.Parse(timeSpan.LastOrDefault()!),
                DayOfWeek = (DayOfWeek)j + 1,
                Blocks = 1,
                Name = name,
                Group = group,
                EvenWeek = evenWeek,
                Initials = initials,
                Place = ParseStringWithDash(textBlocks.LastOrDefault()!)
            };
            var possibleDuplicate = list.Find(x =>
                x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name && x.Group == blockModel.Group &&
                x.Place == blockModel.Place &&
                x.EvenWeek == blockModel.EvenWeek &&
                ((StudentBlock)x).Initials!.Equals(blockModel.Initials, StringComparison.OrdinalIgnoreCase));

            if (possibleDuplicate != null)
                possibleDuplicate.Blocks++;
            else
                list.Add(blockModel);
        }
    }

    private static string ParseStringWithDash(string s)
    {
        var index = s.LastIndexOf("-", StringComparison.Ordinal);
        return index >= 0 ? s[..index] : s;
    }

    private static string ParseStringWithDash(IEnumerable<string> strings)
    {
        var sb = new StringBuilder();
        foreach (var s in strings)
            sb.Append($"{ParseStringWithDash(s)},");
        return sb.ToString()[..^1];
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

        var teachers = document.QuerySelector("div#nauczyciele")?.QuerySelectorAll("p");
        foreach (var teacher in teachers!.Select(x => x.InnerHtml))
        {
            var pFrom = teacher.IndexOf("/", StringComparison.Ordinal);
            var pTo = teacher[(pFrom + 1)..].IndexOf("\"", StringComparison.Ordinal);
            var innerTexts = Regex.Replace(teacher, "<.*?>", string.Empty).Split(" ");
            var name = ParseStringWithDash(innerTexts.FirstOrDefault()!);
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
            var innerText = Regex.Replace(room, "<.*?>", string.Empty);
            var innerTexts = innerText.Split(" ").ToList();
            if ((innerTexts.Count == 1 && (innerTexts.FirstOrDefault()!.LastOrDefault() is not 'p' and not 'n' ||
                                           innerTexts.FirstOrDefault()!.Contains("--------"))) ||
                (innerTexts.Count == 2 && Regex.IsMatch(innerTexts.LastOrDefault()!, "^M-?[1-9]") &&
                 !Regex.IsMatch(innerTexts.FirstOrDefault()!, "^M-?[1-9]"))) continue;
            var pFrom = room.IndexOf("/", StringComparison.Ordinal);
            var pTo = room[(pFrom + 1)..].IndexOf("\"", StringComparison.Ordinal);
            var name = ParseStringWithDash(innerTexts.FirstOrDefault()!);
            var organizationalUnit = "WM";
            if (innerTexts.Count == 1)
                organizationalUnit = name;
            else if (Regex.IsMatch(innerTexts[1], "^M-?[1-9]"))
                organizationalUnit = innerTexts[1];
            var description = string.Empty;
            if (innerText.Contains("w tyg."))
            {
                innerTexts.RemoveRange(innerTexts.Count - 4, 3);
                var stringBuilder = new StringBuilder();
                foreach (var text in innerTexts.Skip(1))
                {
                    if (Regex.IsMatch(text, "^M-?[1-9]")) continue;
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
}