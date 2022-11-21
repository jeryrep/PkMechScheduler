using AngleSharp.Dom;
using MechScraper.Enums;
using MechScraper.Models;
using System.Text.RegularExpressions;
using System.Text;
using Group = MechScraper.Models.Group;

namespace MechScraper;

public static class Parser
{
    private const string GenderPattern = "^[(][MK][)]";
    private const string GroupPattern = "[WKLĆSP]0?[0-9]?-";
    private const string LinkPattern = "[nos]\\d{1,3}\\.html";
    private const string HtmlTagPattern = "<.*?>";
    private const string OrganizationalUnitPattern = "^M-?[1-9]";

    public static List<BaseBlock> ConvertDocumentToBlockList(IDocument document, Mode mode)
    {
        var list = new List<BaseBlock>();
        HandleSingleDocument(document, mode, list);
        return list;
    }

    public static List<BaseBlock> ConvertDocumentsToBlockList(IEnumerable<IDocument> documents, Mode mode)
    {
        var list = new List<BaseBlock>();
        foreach (var document in documents) HandleSingleDocument(document, mode, list);
        return list;
    }

    private static void HandleSingleDocument(IParentNode document, Mode mode, List<BaseBlock> returnList)
    {
        var groupNumber = Regex
            .Replace(document.QuerySelector("span.tytulnapis")?.InnerHtml!, HtmlTagPattern, string.Empty).Last();
        var table = document.QuerySelector("table.tabela");
        var rows = table?.QuerySelectorAll("tr");
        var list = new List<BaseBlock>();
        foreach (var row in rows!.Skip(1).Select((row, i) => (row, i)))
            HandleRow(row.row, row.i, groupNumber, list, mode);
        if (mode == Mode.Student)
        {
            UnifyBothWeekSubjects(list, mode);
        }
        SaveUniqueBlocks(list, mode, returnList);
        if (mode != Mode.Student)
        {
            UnifyBothWeekSubjects(returnList, mode);
        }
    }

    private static void SaveUniqueBlocks(IEnumerable<BaseBlock> list, Mode mode, List<BaseBlock> returnList)
    {
        switch (mode)
        {
            case Mode.Student:
                foreach (var blockModel in list.Select(block => new
                             {
                                 blockModel = (StudentBlock)block,
                                 possibleDuplicate = returnList.Find(x =>
                                     x.DayOfWeek == block.DayOfWeek && x.Name == block.Name &&
                                     x.Group == block.Group && ((StudentBlock)x).Place == ((StudentBlock)block).Place &&
                                     x.EvenWeek == block.EvenWeek)
                             })
                             .Where(t => t.possibleDuplicate == null)
                             .Select(t => t.blockModel)) returnList.Add(blockModel);
                break;
            case Mode.Teacher:
                foreach (var blockModel in list.Select(block => new
                             {
                                 blockModel = (TeacherBlock)block,
                                 possibleDuplicate = returnList.OfType<TeacherBlock>().ToList().Find(x =>
                                     x.DayOfWeek == block.DayOfWeek && x.Name == block.Name &&
                                     x.Group == block.Group && x.Place == ((TeacherBlock)block).Place &&
                                     x.EvenWeek == block.EvenWeek && x.Courses == ((TeacherBlock)block).Courses &&
                                     x.Number == block.Number)
                             })
                             .Where(t => t.possibleDuplicate == null)
                             .Select(t => t.blockModel)) returnList.Add(blockModel);
                break;
            default:
                foreach (var blockModel in list.Select(block => new
                             {
                                 blockModel = (RoomBlock)block,
                                 possibleDuplicate = returnList.OfType<RoomBlock>().ToList().Find(x =>
                                     x.DayOfWeek == block.DayOfWeek && x.Name == block.Name &&
                                     x.Group == block.Group && x.Initials == ((RoomBlock)block).Initials &&
                                     x.EvenWeek == block.EvenWeek && x.Courses == ((RoomBlock)block).Courses)
                             })
                             .Where(t => t.possibleDuplicate == null)
                             .Select(t => t.blockModel)) returnList.Add(blockModel);
                break;
        }
    }

    private static void UnifyBothWeekSubjects(List<BaseBlock> list, Mode mode)
    {
        switch (mode)
        {
            case Mode.Student:
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var blockModel = list[i];
                    if (blockModel.Name is "J angielski" or "WF") continue;
                    var doubleWeek = list.Find(x =>
                        x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name &&
                        x.Group == blockModel.Group && ((StudentBlock)x).Place == ((StudentBlock)blockModel).Place &&
                        x.EvenWeek != blockModel.EvenWeek);
                    if (doubleWeek == null) continue;
                    doubleWeek.EvenWeek = null;
                    list.Remove(blockModel);
                }

                break;
            case Mode.Teacher:
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var blockModel = list[i];
                    if (blockModel.Name is "WF") continue;
                    var doubleWeek = list.Find(x =>
                        x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name &&
                        x.Group == blockModel.Group && ((TeacherBlock)x).Place == ((TeacherBlock)blockModel).Place &&
                        x.EvenWeek != blockModel.EvenWeek && x.Number == blockModel.Number);
                    if (doubleWeek == null) continue;
                    doubleWeek.EvenWeek = null;
                    list.Remove(blockModel);
                }

                break;
            default:
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var blockModel = list[i];
                    if (blockModel.Name is "J angielski" or "WF") continue;
                    var doubleWeek = list.Find(x =>
                        x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name &&
                        x.Group == blockModel.Group &&
                        x.EvenWeek != blockModel.EvenWeek);
                    if (doubleWeek == null) continue;
                    doubleWeek.EvenWeek = null;
                    list.Remove(blockModel);
                }

                break;
        }
    }

    private static void HandleRow(IParentNode row, int i, char groupNumber, List<BaseBlock> list, Mode mode)
    {
        var timeSpan = row.QuerySelector("td.g")?.InnerHtml.Split("-");
        foreach (var block in row.QuerySelectorAll("td.l").Select((cell, j) => (cell, j)))
        {
            if (block.cell.InnerHtml.Contains("&nbsp;"))
                continue;
            switch (mode)
            {
                case Mode.Student:
                    HandleBlock(i, groupNumber, list, block.cell, block.j, timeSpan!);
                    break;
                case Mode.Teacher:
                    HandleTeacherBlock(i, list, block.cell, block.j, timeSpan!);
                    break;
                default:
                    HandleRoomBlock(i, list, block.cell, block.j, timeSpan!);
                    break;
            }
        }
    }

    private static void HandleRoomBlock(int i, List<BaseBlock> list, IElement blockCell, int j, string[] timeSpan)
    {
        var textBlocks = Regex.Replace(blockCell.InnerHtml, HtmlTagPattern, string.Empty).Split(" ");
        if (textBlocks.FirstOrDefault()!.Length != 2)
        {
            var simpleBlockModel = new RoomBlock
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
                x.Description == simpleBlockModel.Description);
            if (duplicate != null) duplicate.Blocks++;
            else list.Add(simpleBlockModel);
            return;
        }

        var name = textBlocks[2];
        var group = textBlocks.LastOrDefault();
        var linkMatches = Regex.Matches(blockCell.InnerHtml, LinkPattern);
        var blockModel = new RoomBlock
        {
            Number = (byte)i,
            Start = TimeSpan.Parse(timeSpan.FirstOrDefault()!),
            End = TimeSpan.Parse(timeSpan.LastOrDefault()!),
            DayOfWeek = (DayOfWeek)j + 1,
            Blocks = 1,
            Name = name,
            Group = group,
            EvenWeek = textBlocks[1]
                .Split(",")
                .FirstOrDefault()!.Split('-')
                .LastOrDefault()!.Contains('P',
                    StringComparison.OrdinalIgnoreCase),
            Courses = ParseStringWithDash(textBlocks[1].Split(',')),
            CourseLinks = string.Join('|', linkMatches.Where(x => x.Value.StartsWith('o'))),
            Initials = textBlocks.FirstOrDefault(),
            InitialsLink = linkMatches.FirstOrDefault(x => x.Value.StartsWith('n'))?.Value
        };
        var possibleDuplicate = list.OfType<RoomBlock>().ToList().Find(x =>
            x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name && x.Group == blockModel.Group &&
            x.EvenWeek == blockModel.EvenWeek &&
            x.Courses!.Equals(blockModel.Courses, StringComparison.OrdinalIgnoreCase) &&
            x.Number == blockModel.Number - x.Blocks &&
            x.Initials!.Equals(blockModel.Initials, StringComparison.OrdinalIgnoreCase));

        if (possibleDuplicate != null)
        {
            possibleDuplicate.Blocks++;
            possibleDuplicate.End = blockModel.End;
        }
        else list.Add(blockModel);
    }

    private static void HandleTeacherBlock(int i, List<BaseBlock> list, IElement blockCell, int j, string[] timeSpan)
    {
        var textBlocks = Regex.Replace(blockCell.InnerHtml, HtmlTagPattern, string.Empty).Split(" ");
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

        var linkMatches = Regex.Matches(blockCell.InnerHtml, LinkPattern);
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
            CourseLinks = string.Join("|", linkMatches.Where(x => x.Value.StartsWith('o'))),
            Place = ParseStringWithDash(textBlocks.LastOrDefault()!),
            PlaceLink = linkMatches.FirstOrDefault(x => x.Value.StartsWith('s'))?.Value,
        };
        var possibleDuplicate = list.OfType<TeacherBlock>().ToList().Find(x =>
            x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name && x.Group == blockModel.Group &&
            x.Place == blockModel.Place &&
            x.EvenWeek == blockModel.EvenWeek &&
            x.Courses!.Equals(blockModel.Courses, StringComparison.OrdinalIgnoreCase) &&
            x.Number == blockModel.Number - x.Blocks);

        if (possibleDuplicate != null)
        {
            possibleDuplicate.Blocks++;
            possibleDuplicate.End = blockModel.End;
        }
        else list.Add(blockModel);
    }

    private static void HandleBlock(int i, char groupNumber, List<BaseBlock> list, IElement cell, int j,
        string[] timeSpan)
    {
        foreach (var subject in cell.InnerHtml.Split("<br>"))
        {
            var textBlocks = Regex.Replace(subject, HtmlTagPattern, string.Empty).Split(" ");
            if (!subject.Contains("<span class=\"p\">"))
            {
                var simpleBlockModel = new StudentBlock
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
                    x.Description == simpleBlockModel.Description);
                if (duplicate != null) duplicate.Blocks++;
                else list.Add(simpleBlockModel);
                return;
            }

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
                        else if (Regex.IsMatch(textBlock, GenderPattern) || Regex.IsMatch(textBlock, GroupPattern))
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

            var linkMatches = Regex.Matches(subject, LinkPattern);
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
                InitialsLink = linkMatches.FirstOrDefault(x => x.Value.StartsWith('n'))?.Value,
                Place = ParseStringWithDash(textBlocks.LastOrDefault()!),
                PlaceLink = linkMatches.FirstOrDefault(x => x.Value.StartsWith('s'))?.Value,
            };
            var possibleDuplicate = list.OfType<StudentBlock>().ToList().Find(x =>
                x.DayOfWeek == blockModel.DayOfWeek && x.Name == blockModel.Name && x.Group == blockModel.Group &&
                x.Place == blockModel.Place &&
                x.EvenWeek == blockModel.EvenWeek &&
                x.Initials!.Equals(blockModel.Initials, StringComparison.OrdinalIgnoreCase));

            if (possibleDuplicate != null)
            {
                possibleDuplicate.Blocks++;
                possibleDuplicate.End = blockModel.End;
            }
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

    public static IEnumerable<Group> GetGroupListFromDocument(IDocument document)
    {
        var paragraphs = document.QuerySelector("div#oddzialy")?.QuerySelectorAll("p");
        return paragraphs!.Select(x => x.InnerHtml)
            .Select(paragraph => new { paragraph, pFrom = paragraph.IndexOf("/", StringComparison.Ordinal) })
            .Select(t => new { t, pTo = t.paragraph[(t.pFrom + 1)..].IndexOf("\"", StringComparison.Ordinal) })
            .Select(t => new Group
            {
                Name = Regex.Replace(t.t.paragraph, HtmlTagPattern, string.Empty),
                Link = t.t.paragraph.Substring(t.t.pFrom + 1, t.pTo)
            });
    }

    public static IEnumerable<Teacher> GetTeacherListFromDocument(IDocument document)
    {
        var teachers = document.QuerySelector("div#nauczyciele")?.QuerySelectorAll("p");
        return teachers!.Select(x => x.InnerHtml)
            .Select(teacher => new { teacher, pFrom = teacher.IndexOf("/", StringComparison.Ordinal) })
            .Select(t => new { t, pTo = t.teacher[(t.pFrom + 1)..].IndexOf("\"", StringComparison.Ordinal) })
            .Select(t => new { t, innerTexts = Regex.Replace(t.t.teacher, HtmlTagPattern, string.Empty).Split(" ") })
            .Select(t => new { t, name = ParseStringWithDash(t.innerTexts.FirstOrDefault()!) })
            .Select(t => new Teacher
            {
                Name = t.name,
                Link = t.t.t.t.teacher.Substring(t.t.t.t.pFrom + 1, t.t.t.pTo),
                EvenWeek = t.t.innerTexts.FirstOrDefault()!.LastOrDefault() == 'p'
            });
    }

    public static IEnumerable<Room> GetRoomListFromDocument(IDocument document)
    {
        var rooms = document.QuerySelector("div#sale")?.QuerySelectorAll("p");
        var list = new List<Room>();
        foreach (var room in rooms!.Select(x => x.InnerHtml).Skip(1))
        {
            var innerText = Regex.Replace(room, HtmlTagPattern, string.Empty);
            var innerTexts = innerText.Split(" ").ToList();
            if ((innerTexts.Count == 1 && (innerTexts.FirstOrDefault()!.LastOrDefault() is not 'p' and not 'n' ||
                                           innerTexts.FirstOrDefault()!.Contains("--------"))) ||
                (innerTexts.Count == 2 && Regex.IsMatch(innerTexts.LastOrDefault()!, OrganizationalUnitPattern) &&
                 !Regex.IsMatch(innerTexts.FirstOrDefault()!, OrganizationalUnitPattern))) continue;
            var pFrom = room.IndexOf("/", StringComparison.Ordinal);
            var pTo = room[(pFrom + 1)..].IndexOf("\"", StringComparison.Ordinal);
            var name = ParseStringWithDash(innerTexts.FirstOrDefault()!);
            var organizationalUnit = "WM";
            if (innerTexts.Count == 1)
                organizationalUnit = name;
            else if (Regex.IsMatch(innerTexts[1], OrganizationalUnitPattern))
                organizationalUnit = innerTexts[1];
            var description = string.Empty;
            if (innerText.Contains("w tyg."))
            {
                innerTexts.RemoveRange(innerTexts.Count - 4, 3);
                var stringBuilder = new StringBuilder();
                foreach (var text in innerTexts.Skip(1))
                {
                    if (Regex.IsMatch(text, OrganizationalUnitPattern)) continue;
                    stringBuilder.Append($"{text} ");
                }

                description = stringBuilder.ToString();
            }

            list.Add(new Room
            {
                Name = name,
                Description = description,
                Link = room.Substring(pFrom + 1, pTo),
                OrganizationalUnit = organizationalUnit,
                EvenWeek = innerTexts.FirstOrDefault()!.LastOrDefault() == 'p'
            });
        }

        return list;
    }
}