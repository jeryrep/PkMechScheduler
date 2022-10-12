using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.Shapes;
using PkMechScheduler.Database.Models;
using PkMechScheduler.Frontend.Helpers;
using PkMechScheduler.Frontend.Services;
using PkMechScheduler.Frontend.Views;

namespace PkMechScheduler.Frontend.Pages;

public partial class ConfiguratorPage
{
    private readonly ScrapService _scheduleService;
    private List<Frame> _frames = new();
    private readonly Dictionary<string, string> _groups;
    private List<IView> _filters = new();

    public ConfiguratorPage()
    {
        _scheduleService = Application.Current?.Handler.MauiContext?.Services.GetService<ScrapService>();
        InitializeComponent();
        //_groups = _scheduleService?.GetGroups();
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
        PrepareScheduleGrid();
    }

    private void PrepareScheduleGrid()
    {
        _frames.ForEach(x => ScheduleGrid.Remove(x));
        _frames = new List<Frame>();
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
    }

    private void GenerateSchedule(object sender, EventArgs e)
    {
        PrepareScheduleGrid();
        /*var schedule = _scheduleService.GetRawSchedule(((StudentFilterView)_filters[0]).PickedGroup);
        for (var i = 0; i < schedule.Keys.Count; i++)
            ScheduleGrid.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#0F3D3E"),
                Padding = 5,
                Content = new Label { Text = Enum.GetName(schedule.Keys.ElementAt(i)) }
            }, i + 2);

        foreach (var daySchedule in schedule)
            foreach (var blockModel in daySchedule.Value.Where(x => FiltersApply(x, (StudentFilterView)_filters[0])))
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
                var gesture = new TapGestureRecognizer();
                gesture.Tapped += async (_, _) => {
                    await DisplayAlert(blockModel.Name, $"Grupa: {blockModel.Group}\n" +
                                                        $"Sala: {blockModel.Place}\n" +
                                                        $"Tydzień: {blockModel.EvenWeek switch { true => "Parzysty", false => "Nieparzysty", _ => "Brak informacji" }}\n" +
                                                        $"Liczba godzin: {blockModel.StudentBlocks}",
                        "OK");
                };
                block.GestureRecognizers.Add(gesture);
                _frames.Add(block);
                ScheduleGrid.Add(block, (byte)daySchedule.Key + 2, blockModel.Number + 1);
                ScheduleGrid.SetRowSpan(block, blockModel.StudentBlocks);
            }*/
    }

    private static bool FiltersApply(StudentBlock model, StudentFilterView filter)
    {
        return (model.EvenWeek == filter.EvenWeek || model.EvenWeek == null) && (Regex.IsMatch(model.Group, $"^L0{filter.LaboratoryGroup}") ||
                                                   Regex.IsMatch(model.Group, "^[ĆWS]") ||
                                                   Regex.IsMatch(model.Group, $"^P0{filter.ProjectGroup}") ||
                                                   Regex.IsMatch(model.Group, $"^K0{filter.ComputersLaboratoryGroup}"));
    }

    private void AddGroup(object sender, EventArgs e)
    {
        var filterView = new StudentFilterView();
        _filters.Add(filterView);
        Filters.Add(filterView);
    }
}