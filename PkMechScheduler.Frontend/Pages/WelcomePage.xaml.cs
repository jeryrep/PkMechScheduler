﻿using PkMechScheduler.Database.Models;
using PkMechScheduler.Frontend.Enums;
using PkMechScheduler.Frontend.Interfaces;

namespace PkMechScheduler.Frontend.Pages;

public partial class WelcomePage
{
    private readonly IDatabaseService _databaseService;
    private readonly Dictionary<string, string> _groups;
    private readonly List<IView> _views = new();

    public WelcomePage()
    {
        _databaseService = Application.Current?.Handler.MauiContext?.Services.GetService<IDatabaseService>();
        InitializeComponent();
        _groups = _databaseService?.GetGroups().Result;
        GroupsPicker.ItemsSource = _groups!.Select(g => g.Key[..3]).Distinct().ToList();
        GroupsPicker.SelectedItem = Preferences.ContainsKey("Course") ? Preferences.Get("Course", "11A") : "11A";
        LanguagePicker.SelectedIndex = 0;
        switch (Preferences.Get("Mode", string.Empty))
        {
            case "Teacher":
                StudentButton.IsChecked = false;
                DeansOfficeButton.IsChecked = false;
                TeacherButton.IsChecked = true;
                break;
            case "DeansOffice":
                TeacherButton.IsChecked = false;
                StudentButton.IsChecked = false;
                DeansOfficeButton.IsChecked = true;
                break;
            default:
                TeacherButton.IsChecked = false;
                DeansOfficeButton.IsChecked = false;
                StudentButton.IsChecked = true;
                break;
        }
    }

    private async void SavePreferences(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"///{nameof(SchedulePage)}");
    }

    private void OnStudentChecked(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Set("Mode", "Student");
        DeansOfficeConfig.IsVisible = false;
        TeacherConfig.IsVisible = false;
        StudentConfig.IsVisible = true;
    }

    private void OnTeacherChecked(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Set("Mode", "Teacher");
        DeansOfficeConfig.IsVisible = false;
        StudentConfig.IsVisible = false;
        TeacherConfig.IsVisible = true;
    }

    private void OnDeansOfficeChecked(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Set("Mode", "DeansOffice");
        StudentConfig.IsVisible = false;
        TeacherConfig.IsVisible = false;
        DeansOfficeConfig.IsVisible = true;
    }

    private async void UpdateSubjects(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var allInOne = await _databaseService.GetBlocks(picker?.SelectedItem.ToString());
        Preferences.Set("Course", picker?.SelectedItem.ToString());
        _views.ForEach(x => GroupsSelect.Remove(x));
        _views.Clear();
        AddSubjectCheckboxList(SubjectType.Lecture, allInOne);
        AddSubjectCheckboxList(SubjectType.Exercise, allInOne);
        AddSubjectCheckboxList(SubjectType.ComputersLaboratory, allInOne);
        AddSubjectCheckboxList(SubjectType.Laboratory, allInOne);
        AddSubjectCheckboxList(SubjectType.Projects, allInOne);
        AddSubjectCheckboxList(SubjectType.Seminars, allInOne);
    }

    private void AddSubjectCheckboxList(SubjectType type, IEnumerable<BlockModel> blocks)
    {
        var filteredBlocks = blocks.Where(x => x.Group!.StartsWith((char)type)).ToList();
        if (filteredBlocks.Count == 0) return;
        var groupCount = filteredBlocks.Max(x => Convert.ToInt32(x.Group?.Substring(1, 2)));
        var picker = new Picker();
        var list = new List<string>();
        for (var i = 1; i <= groupCount; i++) list.Add($"{(char)type}0{i}");
        picker.ItemsSource = list;
        picker.SelectedIndexChanged += PickerOnSelectedIndexChanged;
        picker.SelectedIndex = Preferences.ContainsKey(((char)type).ToString())
            ? Preferences.Get(((char)type).ToString(), string.Empty).Last() - 49
            : 0;

        var layout = new HorizontalStackLayout
        {
            new Label
            {
                Text = $"Grupa {(char)type}",
                VerticalTextAlignment = TextAlignment.Center
            },
            picker
        };

        _views.Add(layout);
        GroupsSelect.Add(layout);
    }

    private static void PickerOnSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var option = picker?.SelectedItem.ToString();
        Preferences.Set(option?[0].ToString()!, option);
    }
}