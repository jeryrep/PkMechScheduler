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
    public SchedulePage()
    {
        _databaseService = Application.Current?.Handler.MauiContext?.Services.GetService<IDatabaseService>();
        InitializeComponent();
        GenerateSchedule();
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
                Fill = new SolidColorBrush(i % 2 == 0 ? new Color(15, 52, 96) : new Color(83, 52, 131))
            };
            ScheduleGrid.Add(rect, 2, i);
            ScheduleGrid.SetColumnSpan(rect, 5);
        }
        ScheduleGrid.Add(new Frame
        {
            BackgroundColor = Color.FromArgb("#0F3D3E"),
            Padding = 5,
            Content = new Label { Text = "Nr" }
        });
        ScheduleGrid.Add(new Frame
        {
            BackgroundColor = Color.FromArgb("#0F3D3E"),
            Padding = 5,
            Content = new Label { Text = "Godz" }
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
                Content = new Label
                    { Text = ConstantHelper.Hours.ElementAt(i - 1), HorizontalTextAlignment = TextAlignment.Center }
            }, 1, i);
        }
        for (var i = 1; i <= 5; i++)
            ScheduleGrid.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#0F3D3E"),
                Padding = 5,
                Content = new Label { Text = Enum.GetName((DayOfWeek)i) }
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
                    Text = $"{blockModel.Name} [{blockModel.Group}] {blockModel.Place}",
                    TextColor = Color.FromArgb("#100F0F")
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
        }
    }

    private static bool FiltersApply(BlockModel model)
    {
        return /*(model.EvenWeek == filter.EvenWeek || model.EvenWeek == null) &&*/ 
            model.Group == Preferences.Get(((char)SubjectType.ComputersLaboratory).ToString(), string.Empty) ||
            model.Group == Preferences.Get(((char)SubjectType.Exercise).ToString(), string.Empty) ||
            model.Group == Preferences.Get(((char)SubjectType.Laboratory).ToString(), string.Empty) ||
            model.Group == Preferences.Get(((char)SubjectType.Projects).ToString(), string.Empty) ||
            model.Group == Preferences.Get(((char)SubjectType.Seminars).ToString(), string.Empty) || 
            model.Group == Preferences.Get(((char)SubjectType.Lecture).ToString(), string.Empty);
    }

    private void UpdateSchedule(object sender, EventArgs e)
    {
        GenerateSchedule();
    }
}