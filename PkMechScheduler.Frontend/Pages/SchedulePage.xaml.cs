using MechScraper.Enums;
using MechScraper.Models;
using PkMechScheduler.Database.Enums;
using PkMechScheduler.Frontend.Interfaces;

namespace PkMechScheduler.Frontend.Pages;

public partial class SchedulePage
{
    private readonly IDatabaseService _databaseService;

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        var fullSchedule = (await _databaseService.GetBlocks(Preferences.Get(nameof(Preference.Course), string.Empty),
            Preferences.Get(nameof(Preference.Course), string.Empty))).ToList();
        LectureLayout.IsVisible = fullSchedule.Any(x => x.Description == null && x.Group!.StartsWith(((char)SubjectType.Lecture).ToString()));
        ExerciseLayout.IsVisible = fullSchedule.Any(x => x.Description == null && x.Group!.StartsWith(((char)SubjectType.Exercise).ToString()));
        LaboratoryLayout.IsVisible =
            fullSchedule.Any(x => x.Description == null && x.Group!.StartsWith(((char)SubjectType.Laboratory).ToString()));
        ComputersLaboratoryLayout.IsVisible =
            fullSchedule.Any(x => x.Description == null && x.Group!.StartsWith(((char)SubjectType.ComputersLaboratory).ToString()));
        ProjectsLayout.IsVisible = fullSchedule.Any(x => x.Description == null && x.Group!.StartsWith(((char)SubjectType.Projects).ToString()));
        SeminarsLayout.IsVisible = fullSchedule.Any(x => x.Description == null && x.Group!.StartsWith(((char)SubjectType.Seminars).ToString()));
        WfLayout.IsVisible = fullSchedule.Any(x => x.Description == null && x.Group is "K" or "M");
        EnglishLayout.IsVisible = fullSchedule.Any(x => x.Description == null && x.Group == ((char)SubjectType.Exercise).ToString());
        await GenerateSchedule();
        Title = Preferences.Get(nameof(Preference.Course), "Rozkład zajęć");
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        LectureCheckbox.IsChecked = true;
        ExerciseCheckbox.IsChecked = true;
        LaboratoryCheckbox.IsChecked = true;
        ComputersLaboratoryCheckbox.IsChecked = true;
        ProjectCheckbox.IsChecked = true;
        SeminarsCheckbox.IsChecked = true;
        WfCheckbox.IsChecked = true;
        EnglishCheckbox.IsChecked = true;
    }

    public SchedulePage()
    {
        _databaseService = Application.Current?.Handler.MauiContext?.Services.GetService<IDatabaseService>();
        InitializeComponent();
    }

    private async Task GenerateSchedule() => ScheduleGridView.GenerateSchedule(
        (await _databaseService.GetBlocks(Preferences.Get(nameof(Preference.Course), string.Empty),
            Preferences.Get(nameof(Preference.Course), string.Empty))).Where(FiltersApply));

    private bool FiltersApply(StudentBlock model) =>
        (model.EvenWeek == null || model.EvenWeek == WeekLabel.Text.StartsWith("P")) &&
        ((model.Group == Preferences.Get(((char)SubjectType.ComputersLaboratory).ToString(), string.Empty) &&
          ComputersLaboratoryCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Exercise).ToString(), string.Empty) &&
          ExerciseCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Laboratory).ToString(), string.Empty) &&
          LaboratoryCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Projects).ToString(), string.Empty) &&
          ProjectCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Seminars).ToString(), string.Empty) &&
          SeminarsCheckbox.IsChecked) ||
         (model.Group == Preferences.Get(((char)SubjectType.Lecture).ToString(), string.Empty) &&
          LectureCheckbox.IsChecked) ||
         (model.Name == "WF" && Preferences.Get("WF", string.Empty) == model.Group && WfCheckbox.IsChecked) ||
         (model.Name == "J angielski" && EnglishCheckbox.IsChecked));

    private async void UpdateSchedule(object sender, EventArgs e) => await GenerateSchedule();

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
        (await _databaseService.GetBlocks(Preferences.Get(nameof(Preference.Course), string.Empty),
            Preferences.Get(nameof(Preference.Course), string.Empty), true)).Where(FiltersApply));
}