using PkMechScheduler.Frontend.Services;

namespace PkMechScheduler.Frontend.Views;

public partial class StudentFilterView
{
    private readonly Dictionary<string, string> _groups;
    public StudentFilterView()
    {
        var scheduleService = Application.Current?.Handler.MauiContext?.Services.GetService<ScrapService>();
        //_groups = scheduleService?.GetGroups();
        InitializeComponent();
        GroupsPicker.ItemsSource = _groups!.Select(g => g.Key).ToList();
        GroupsPicker.SelectedIndex = 0;
        WeekPicker.SelectedIndex = 0;
    }

    public string PickedGroup => _groups[GroupsPicker.SelectedItem as string ?? string.Empty];

    public string LaboratoryGroup => LaboratoryGroupNumber.Text;
    public string ProjectGroup => ProjectGroupNumber.Text;
    public string ComputersLaboratoryGroup => ComputersLabGroupNumber.Text;
    public bool EvenWeek => (WeekPicker.SelectedIndex + 1) % 2 == 0;
}