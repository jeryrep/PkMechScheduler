using PkMechScheduler.Database.Models;
using PkMechScheduler.Frontend.Enums;
using PkMechScheduler.Frontend.Interfaces;

namespace PkMechScheduler.Frontend.Pages;

public partial class TeacherSchedulePage
{
    private readonly IDatabaseService _databaseService;
    public TeacherSchedulePage()
    {
        _databaseService = Application.Current?.Handler.MauiContext?.Services.GetService<IDatabaseService>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args) => await GenerateSchedule();

    private async Task GenerateSchedule() => ScheduleGridView.GenerateSchedule(
        (await _databaseService.GetTeacherBlocks(Preferences.Get(nameof(Preference.Teacher), string.Empty))).Where(FiltersApply));

    private bool FiltersApply(BaseBlock model) => model.EvenWeek == null || model.EvenWeek == WeekLabel.Text.StartsWith("P");

    private async void RightBtnClicked(object sender, EventArgs e)
    {
        WeekLabel!.Text = WeekLabel.Text switch
        {
            "Parzysty" => "Mieszany",
            "Nieparzysty" => "Parzysty",
            _ => "Nieparzysty"
        };
        await GenerateSchedule();
    }

    private async void LeftBtnClicked(object sender, EventArgs e)
    {
        WeekLabel!.Text = WeekLabel.Text switch
        {
            "Parzysty" => "Nieparzysty",
            "Nieparzysty" => "Mieszany",
            _ => "Parzysty"
        };
        await GenerateSchedule();
    }

    private async void ForceUpdate(object sender, EventArgs e) => ScheduleGridView.GenerateSchedule(
        (await _databaseService.GetTeacherBlocks(Preferences.Get(nameof(Preference.Teacher), string.Empty), true)).Where(FiltersApply));
}