using MechScraper.Models;
using PkMechScheduler.Database.Enums;
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

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        Title = Preferences.Get(nameof(Preference.Teacher), "Rozkład zajęć nauczyciela");
        await GenerateSchedule();
    }

    private async Task GenerateSchedule() => ScheduleGridView.GenerateSchedule(
        (await _databaseService.GetTeacherBlocks()).Where(FiltersApply));

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

    private async void ForceUpdate(object sender, EventArgs e) => await GenerateSchedule();
}