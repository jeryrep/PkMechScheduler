using PkMechScheduler.Frontend.Helpers;
using PkMechScheduler.Frontend.Models;
using PkMechScheduler.Frontend.Services;

namespace PkMechScheduler.Frontend.Pages;

public partial class SchedulePage
{
    private readonly ScheduleService _scheduleService;
    private List<Frame> _frames = new();
    private readonly Dictionary<string, string> _groups;
    public SchedulePage()
    {
        _scheduleService = Application.Current?.Handler.MauiContext?.Services.GetService<ScheduleService>();
        InitializeComponent();
        _groups = _scheduleService?.GetGroups();
        GroupsPicker.ItemsSource = _groups!.Select(g => g.Key).ToList();
        GroupsPicker.SelectedIndex = 0;
    }

    private void GenerateSchedule(object sender, EventArgs e)
    {
        foreach (var frame in _frames)
            ScheduleGrid.Remove(frame);
        _frames = new List<Frame>();
        var schedule = _scheduleService.GetRawSchedule(_groups.ElementAt(GroupsPicker.SelectedIndex).Value);
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
                Content = new Label { Text = ConstantHelper.Hours.ElementAt(i - 1), HorizontalTextAlignment = TextAlignment.Center }
            }, 1, i);
        }
        for (var i = 0; i < schedule.Keys.Count; i++)
            ScheduleGrid.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#0F3D3E"),
                Padding = 5,
                Content = new Label { Text = Enum.GetName(schedule.Keys.ElementAt(i)) }
            }, i + 2);

        foreach (var daySchedule in schedule)
            foreach (var blockModel in daySchedule.Value.Where(FiltersApply))
            {
                var block = new Frame
                {
                    BackgroundColor = Color.FromArgb("#E2DCC8"),
                    Padding = 5,
                    Content = new Label
                    {
                        Text = $"{blockModel.Name} [{blockModel.Group}] {blockModel.Place}",
                        TextColor = Color.FromArgb("#100F0F")
                    }
                };
                _frames.Add(block);
                ScheduleGrid.Add(block, (byte)daySchedule.Key + 2, blockModel.Number + 1);
            }
    }

    private static bool FiltersApply(BlockModel model)
    {
        return model.EvenWeek is true or null;
    }
}