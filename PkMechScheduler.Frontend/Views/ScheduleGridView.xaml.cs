using Microsoft.Maui.Controls.Shapes;
using PkMechScheduler.Database.Models;
using PkMechScheduler.Frontend.Helpers;

namespace PkMechScheduler.Frontend.Views;

public partial class ScheduleGridView
{
    private readonly List<Frame> _frames = new();
    private Line _timeLine = new();
    private Rectangle _currentDay = new();
    public ScheduleGridView() => InitializeComponent();

    private void PrepareScheduleGrid()
    {
        _frames.ForEach(x => ScheduleGrid.Remove(x));
        _frames.Clear();
        ScheduleGrid.Remove(_timeLine);
        ScheduleGrid.Remove(_currentDay);
        if (DateTime.Now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return;
        _currentDay = new Rectangle
        {
            Opacity = 0.05,
            StrokeThickness = 2
        };
        _currentDay.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb("#ACACAC"), Color.FromArgb("#ACACAC"));
        _currentDay.SetAppThemeColor(Shape.StrokeProperty, Color.FromArgb("#FFFFFF"), Color.FromArgb("#FFFFFF"));
        ScheduleGrid.Add(_currentDay, (int)DateTime.Now.DayOfWeek, 1);
        ScheduleGrid.SetRowSpan(_currentDay, 16);
    }

    public void GenerateSchedule(IEnumerable<BlockModel> blocks)
    {
        PrepareScheduleGrid();

        foreach (var blockModel in blocks)
        {
            var block = new Frame
            {
                Background = Color.FromArgb("#6E6E6E"),
                Padding = 5,
                CornerRadius = 4,
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
            block.SetAppThemeColor(Microsoft.Maui.Controls.Frame.BorderColorProperty, Color.FromArgb("#C8C8C8"), Color.FromArgb("#404040"));
            var gesture = new TapGestureRecognizer();
            gesture.Tapped += async (_, _) => {
                await Application.Current?.MainPage?.DisplayAlert(blockModel.Name, $"Grupa: {blockModel.Group}\n" +
                    $"Sala: {blockModel.Place}\n" +
                    $"Tydzień: {blockModel.EvenWeek switch { true => "Parzysty", false => "Nieparzysty", _ => "Oba" }}\n" +
                    $"Liczba godzin: {blockModel.Blocks}", "OK")!;
            };
            block.GestureRecognizers.Add(gesture);
            _frames.Add(block);
            ScheduleGrid.Add(block, (int)blockModel.DayOfWeek, blockModel.Number + 1);
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
            ScheduleGrid.Add(_timeLine, 1, rowNum + 1);
            ScheduleGrid.SetColumnSpan(_timeLine, ScheduleGrid.ColumnDefinitions.Count);
        }
    }

    private static (int, bool) GetCurrentHourRow()
    {
        if (DateTime.Now < DateTime.Parse(ConstantHelper.Hours.First().Split("-").First()))
            return (0, true);
        if (DateTime.Now > DateTime.Parse(ConstantHelper.Hours.Last().Split("-").Last()))
            return (ConstantHelper.Hours.Count, true);
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
}