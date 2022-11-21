using MechScraper.Models;
using Microsoft.Maui.Controls.Shapes;
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
#if WINDOWS
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
#endif
    }

    public void GenerateSchedule(IEnumerable<BaseBlock> blocks)
    {
        PrepareScheduleGrid();

        foreach (var blockModel in blocks)
        {
            var block = new Frame
            {
                Padding = 5,
                CornerRadius = 4,
            };
            block.SetAppThemeColor(Microsoft.Maui.Controls.Frame.BorderColorProperty, Color.FromArgb("#C8C8C8"), Color.FromArgb("#404040"));
            block.SetAppThemeColor(BackgroundColorProperty, Color.FromArgb("#ffeeff"), Color.FromArgb("#c48b9f"));
            Label label;
            if (blockModel is TeacherBlock { Description: { } } tBlock)
                label = new Label
                {
                    Text = tBlock.Description,
                    TextColor = Color.FromArgb("#100F0F"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
            else
            {
                var formattedString = blockModel switch
                {
                    TeacherBlock teacherBlock => new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = $"{teacherBlock.Courses}\n"},
                            new Span { Text = $"{blockModel.Name} " },
                            new Span { Text = $"{blockModel.Group}\n", FontAttributes = FontAttributes.Italic },
                            new Span { Text = teacherBlock.Place, FontAttributes = FontAttributes.Bold }
                        }
                    },
                    StudentBlock studentBlock => new FormattedString
                    {
                        Spans =
                        {
                            new Span { Text = $"{blockModel.Name} " },
                            new Span { Text = $"{blockModel.Group}\n", FontAttributes = FontAttributes.Italic },
                            new Span { Text = studentBlock.Place, FontAttributes = FontAttributes.Bold }
                        }
                    },
                    _ => new FormattedString()
                };
                label = new Label
                {
                    FormattedText = formattedString,
                    TextColor = Color.FromArgb("#100F0F"),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
            }
            var gesture = new TapGestureRecognizer();
            switch (blockModel)
            {
                case TeacherBlock { Description: null } teacherBlock:
                    gesture.Tapped += async (_, _) => {
                        await Application.Current?.MainPage?.DisplayAlert(blockModel.Name, $"Grupy: {teacherBlock.Courses}\n" +
                            $"Sala: {teacherBlock.Place}\n" +
                            $"Tydzień: {blockModel.EvenWeek switch { true => "Parzysty", false => "Nieparzysty", _ => "Oba" }}\n" +
                            $"Liczba godzin: {blockModel.Blocks}", "OK")!;
                    };
                    break;
                case StudentBlock studentBlock:
                    gesture.Tapped += async (_, _) => {
                        await Application.Current?.MainPage?.DisplayAlert(blockModel.Name, $"Grupy: {blockModel.Group}\n" +
                            $"Sala: {studentBlock.Place}\n" +
                            $"Tydzień: {blockModel.EvenWeek switch { true => "Parzysty", false => "Nieparzysty", _ => "Oba" }}\n" +
                            $"Liczba godzin: {blockModel.Blocks}", "OK")!;
                    };
                    break;
            }
            block.GestureRecognizers.Add(gesture);
            block.Content = label;

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