using PkMechScheduler.Frontend.Services;

namespace PkMechScheduler.Frontend.Views;

public partial class StudentFilterView : ContentView
{
    private readonly ScheduleService _scheduleService;
    private readonly Dictionary<string, string> _groups;
    public StudentFilterView()
    {
        _scheduleService = Application.Current?.Handler.MauiContext?.Services.GetService<ScheduleService>();
        _groups = _scheduleService?.GetGroups();
        InitializeComponent();
        GroupsPicker.ItemsSource = _groups!.Select(g => g.Key).ToList();
        GroupsPicker.SelectedIndex = 0;
    }

    public string PickedGroup => GroupsPicker.SelectedItem as string;
}