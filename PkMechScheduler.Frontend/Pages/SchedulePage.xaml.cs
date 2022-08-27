using Newtonsoft.Json;
using PkMechScheduler.Frontend.Helpers;
using PkMechScheduler.Frontend.Models;

namespace PkMechScheduler.Frontend.Pages;

public partial class SchedulePage : ContentPage
{
    public SchedulePage()
    {
        InitializeComponent();
        GroupsPicker.ItemsSource = new List<string>
        {
            "o1",
            "o2"
        };
        GroupsPicker.SelectedIndex = 0;
    }
    private async void OnGroupChanged(object sender, EventArgs e)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7293")
        };
        var content = httpClient.GetAsync($"GetRawSchedule?group={GroupsPicker.SelectedItem}").Result.Content;
        var dict = JsonConvert.DeserializeObject<Dictionary<Day, List<BlockModel>>>(await content.ReadAsStringAsync());
        ScheduleGrid.AddRowDefinition(new RowDefinition());
        ScheduleGrid.AddColumnDefinition(new ColumnDefinition(30));
        ScheduleGrid.Add(new Frame
        {
            BackgroundColor = Color.FromArgb("#0F3D3E"),
            Padding = 5,
            Content = new Label { Text = "Nr" }
        });
        ScheduleGrid.AddColumnDefinition(new ColumnDefinition(100));
        ScheduleGrid.Add(new Frame
        {
            BackgroundColor = Color.FromArgb("#0F3D3E"),
            Padding = 5,
            Content = new Label { Text = "Godz" }
        }, 1);
        for (var i = 1; i <= 16; i++)
        {
            ScheduleGrid.RowDefinitions.Add(new RowDefinition());
            ScheduleGrid.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#0F3D3E"),
                Padding = 5,
                Content = new Label { Text = i.ToString() }
            }, 0, i);
            ScheduleGrid.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#0F3D3E"),
                Padding = 5,
                Content = new Label { Text = ConstantHelper.Hours.ElementAt(i - 1) }
            }, 1, i);
        }
        for (var i = 0; i < dict?.Keys.Count; i++)
        {
            ScheduleGrid.AddColumnDefinition(new ColumnDefinition());
            ScheduleGrid.Add(new Frame
            {
                BackgroundColor = Color.FromArgb("#0F3D3E"),
                Padding = 5,
                Content = new Label { Text = Enum.GetName(dict.Keys.ElementAt(i)) }
            }, i + 2);
        }

        foreach (var daySchedule in dict!)
            foreach (var blockModel in daySchedule.Value.Where(x => x.EvenWeek is true or null))
                ScheduleGrid.Add(new Frame
                {
                    BackgroundColor = Color.FromArgb("#E2DCC8"),
                    Padding = 5,
                    Content = new Label { Text =$"{blockModel.Name} [{blockModel.Group}] {blockModel.Place}", TextColor = Color.FromArgb("#100F0F") }
                }, (byte)daySchedule.Key + 2, blockModel.Number + 1);
    }
}