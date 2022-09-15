using System.Globalization;
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
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        _schedule = _databaseService.GetBlocks(Preferences.Get("Course", string.Empty)).Result.Where(FiltersApply).ToList();
        LectureLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Lecture).ToString()));
        ExerciseLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Exercise).ToString()));
        LaboratoryLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Laboratory).ToString()));
        ComputersLaboratoryLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.ComputersLaboratory).ToString()));
        ProjectsLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Projects).ToString()));
        SeminarsLayout.IsVisible = _schedule.Any(x => x.Group!.StartsWith(((char)SubjectType.Seminars).ToString()));
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
        for (var i = 1; i <= 16; i++)
        {
            ScheduleGrid.AddRowDefinition(new RowDefinition());
            var rect = new Rectangle
            {
                Fill = new SolidColorBrush(i % 2 == 0 ? Color.FromArgb("#A9A9A9") : Colors.Transparent)
            };
            ScheduleGrid.Add(rect, 2, i);
            ScheduleGrid.SetColumnSpan(rect, 5);
        }

        ScheduleGrid.Add(new Frame
        {
            BackgroundColor = Color.FromArgb("#0F3D3E"),
            Padding = 5,
            Content = new Label { Text = "Nr" , HorizontalTextAlignment = TextAlignment.Center }
        });
        ScheduleGrid.Add(new Frame
        {
            BackgroundColor = Color.FromArgb("#0F3D3E"),
            Padding = 5,
            Content = new Label { Text = "Godz", HorizontalTextAlignment = TextAlignment.Center }
        }, 1);
        for (var i = 1; i <= 16; i++)
        {
            ScheduleGrid.AddRowDefinition(new RowDefinition());
            ScheduleGrid.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#0F3D3E"),
                Padding = 5,
                Content = new Label { Text = i.ToString(), HorizontalTextAlignment = TextAlignment.Center }
            }, 0, i);
            ScheduleGrid.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#0F3D3E"),
                Padding = 5,
                Content = new Label { Text = ConstantHelper.Hours.ElementAt(i - 1), HorizontalTextAlignment = TextAlignment.Center }
            }, 1, i);
        }

        for (var i = 1; i <= 5; i++)
            ScheduleGrid.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#0F3D3E"),
                Padding = 5,
                Content = new Label
                {
                    Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(new CultureInfo("pl-PL").DateTimeFormat.GetDayName((DayOfWeek)i)),
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }, i + 1);
    }

    private void GenerateSchedule()
    {
        PrepareScheduleGrid();
        var schedule = _databaseService.GetBlocks(Preferences.Get("Course", string.Empty)).Result.Where(FiltersApply);

        foreach (var blockModel in schedule)
        {
            var block = new Frame
            {
                Background = blockModel.EvenWeek switch
                {
                    true => Color.FromArgb("#A5F1E9"),
                    false => Color.FromArgb("#FA9494"),
                    _ => new LinearGradientBrush(new GradientStopCollection
                    {
                        new(Color.FromArgb("#A5F1E9"), 0.1f),
                        new(Color.FromArgb("#FA9494"), 1.0f)
                    })
                },
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
            var line = new Line
            {
                X2 = DeviceDisplay.MainDisplayInfo.Width,
                Stroke = Brush.OrangeRed,
                StrokeDashArray = new DoubleCollection(new[] { 1.0, 1.0 }),
                StrokeDashOffset = 5
            };
            var (rowNum, breakTime) = GetCurrentHourRow();
            if (!breakTime) line.VerticalOptions = LayoutOptions.Center;
            ScheduleGrid.Add(line, 2, rowNum + 1);
            ScheduleGrid.SetColumnSpan(line, ScheduleGrid.ColumnDefinitions.Count);
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
                return (i, true);
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
         (model.Group == Preferences.Get(((char)SubjectType.Lecture).ToString(), string.Empty) && LectureCheckbox.IsChecked));

    private void UpdateSchedule(object sender, EventArgs e) => GenerateSchedule();
}