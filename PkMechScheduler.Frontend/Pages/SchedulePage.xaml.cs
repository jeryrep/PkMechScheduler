using Microsoft.Maui.Controls.Shapes;
using PkMechScheduler.Database.Models;
using PkMechScheduler.Frontend.Enums;
using PkMechScheduler.Frontend.Helpers;
using PkMechScheduler.Frontend.Interfaces;

namespace PkMechScheduler.Frontend.Pages;

public partial class SchedulePage
{
    private readonly IDatabaseService _databaseService;
    private readonly List<Frame> _frames = new();
    private List<BlockModel> _schedule;
    private Line _timeLine = new();
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        _schedule = _databaseService.GetBlocks(Preferences.Get("Course", string.Empty)).Result.Where(FiltersApply).ToList();
        LectureLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Lecture).ToString()));
        ExerciseLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Exercise).ToString()));
        LaboratoryLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Laboratory).ToString()));
        ComputersLaboratoryLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.ComputersLaboratory).ToString()));
        ProjectsLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Projects).ToString()));
        SeminarsLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Seminars).ToString()));
        WfLayout.IsVisible = _schedule.Any(x => x.Group is "K" or "M");
        EnglishLayout.IsVisible = _schedule.Any(x => x.Group == ((char)SubjectType.Exercise).ToString());
        GenerateSchedule();
    }

    public SchedulePage()
    {
        _databaseService = Application.Current?.Handler.MauiContext?.Services.GetService<IDatabaseService>();
        InitializeComponent();
    }

    private void PrepareScheduleGrid()
    {
        _frames.ForEach(x => ScheduleGrid.Remove(x));
        _frames.Clear();
        ScheduleGrid.Remove(_timeLine);
    }

    private void GenerateSchedule()
    {
        PrepareScheduleGrid();
        var schedule = _databaseService.GetBlocks(Preferences.Get("Course", string.Empty)).Result.Where(FiltersApply);

        foreach (var blockModel in schedule)
        {
            var block = new Frame
            {
                Background = Color.FromArgb("#6E6E6E"),
                Padding = 5,
                Content = new Label
                {
                    FormattedText = new FormattedString
                    {
                        Spans =
                        {
                            new Span
                            {
                                Text = $"{blockModel.Name} "
                            },
                            new Span
                            {
                                Text = $"{blockModel.Group}\n",
                                FontAttributes = FontAttributes.Italic
                            },
                            new Span
                            {
                                Text = blockModel.Place,
                                FontAttributes = FontAttributes.Bold
                            }
                        }
                    },
                    TextColor = Color.FromArgb("#100F0F"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                }
            };
            var gesture = new TapGestureRecognizer();
            gesture.Tapped += async (_, _) => {
                await DisplayAlert(blockModel.Name, $"Grupa: {blockModel.Group}\n" +
                                                    $"Sala: {blockModel.Place}\n" +
                                                    $"Tydzień: {blockModel.EvenWeek switch { true => "Parzysty", false => "Nieparzysty", _ => "Brak informacji" }}\n" +
                                                    $"Liczba godzin: {blockModel.Blocks}", "OK");
            };
            block.GestureRecognizers.Add(gesture);
            _frames.Add(block);
            ScheduleGrid.Add(block, (int)blockModel.DayOfWeek + 1, blockModel.Number + 1);
            ScheduleGrid.SetRowSpan(block, blockModel.Blocks);
            _timeLine = new Line
            {
                X2 = DeviceDisplay.MainDisplayInfo.Width,
                Stroke = Brush.OrangeRed,
                StrokeDashArray = new DoubleCollection(new[] { 1.0, 1.0 }),
                StrokeDashOffset = 5
            };
            var (rowNum, breakTime) = GetCurrentHourRow();
            if (!breakTime) _timeLine.VerticalOptions = LayoutOptions.Center;
            ScheduleGrid.Add(_timeLine, 2, rowNum + 1);
            ScheduleGrid.SetColumnSpan(_timeLine, ScheduleGrid.ColumnDefinitions.Count);
        }
    }

    private static (int, bool) GetCurrentHourRow()
    {
        if (DateTime.Now < DateTime.Parse(ConstantHelper.Hours.First().Split("-").First()))
            return (0, true);
        if (DateTime.Now > DateTime.Parse(ConstantHelper.Hours.Last().Split("-").Last()))
            return (ConstantHelper.Hours.Count - 1, true);
        foreach (var (h, i) in ConstantHelper.Hours.Select((h, i) => (h, i)))
        {
            var timespan = h.Split("-");
            if (DateTime.Now > DateTime.Parse(timespan[0]) && DateTime.Now < DateTime.Parse(timespan[1]))
                return (i, false);
            if (DateTime.Now.Subtract(new TimeSpan(0, 15, 0)) < DateTime.Parse(timespan[1]))
                return (i + 1, true);
        }
        return (0, false);
    }

    private bool FiltersApply(BlockModel model) =>
        (model.EvenWeek == EvenWeekRadio.IsChecked || model.EvenWeek == null) &&
        ((model.Group == Preferences.Get(((char)SubjectType.ComputersLaboratory).ToString(), string.Empty) && ComputersLaboratoryCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Exercise).ToString(), string.Empty) && ExerciseCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Laboratory).ToString(), string.Empty) && LaboratoryCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Projects).ToString(), string.Empty) && ProjectCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Seminars).ToString(), string.Empty) && SeminarsCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Lecture).ToString(), string.Empty) && LectureCheckbox.IsChecked) ||
         (model.Name == "WF" && Preferences.Get("WF", string.Empty) == model.Group && WfCheckbox.IsChecked) || 
         (model.Name == "J angielski" && EnglishCheckbox.IsChecked));

    private void UpdateSchedule(object sender, EventArgs e) => GenerateSchedule();
}