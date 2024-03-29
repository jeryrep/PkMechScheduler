﻿using MechScraper.Enums;
using MechScraper.Models;
using PkMechScheduler.Database.Enums;
using PkMechScheduler.Frontend.Interfaces;

namespace PkMechScheduler.Frontend.Pages;

public partial class WelcomePage
{
    private readonly IDatabaseService _databaseService;
    private readonly List<IView> _views = new();

    public WelcomePage()
    {
        _databaseService = Application.Current?.Handler.MauiContext?.Services.GetService<IDatabaseService>();
        InitializeComponent();
        var groups = _databaseService?.GetGroups().Result;
        GroupsPicker.ItemsSource = groups!.Select(g => g.Key[..3]).Distinct().ToList();
        GroupsPicker.SelectedItem = Preferences.Get(nameof(Preference.Course), "11A");
        var teachers = _databaseService?.GetTeachers().Result;
        TeacherPicker.ItemsSource = teachers!.Select(g => g.Key).Distinct().ToList();
        if (Preferences.ContainsKey(nameof(Preference.Teacher)))
            TeacherPicker.SelectedItem = Preferences.Get(nameof(Preference.Teacher), string.Empty);
        else
            TeacherPicker.SelectedIndex = 0;
        LanguagePicker.SelectedIndex = 0;
        switch (Preferences.Get(nameof(Preference.Mode), "Student"))
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
        if (StudentButton.IsChecked)
            await Shell.Current.GoToAsync($"///{nameof(SchedulePage)}");
        else if (TeacherButton.IsChecked)
            await Shell.Current.GoToAsync($"///{nameof(TeacherSchedulePage)}");
    }

    private void OnStudentChecked(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Set(nameof(Preference.Mode), "Student");
        TeacherConfig.IsVisible = false;
        StudentConfig.IsVisible = true;
    }

    private void OnTeacherChecked(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Set(nameof(Preference.Mode), "Teacher");
        StudentConfig.IsVisible = false;
        TeacherConfig.IsVisible = true;
    }

    private void OnDeansOfficeChecked(object sender, CheckedChangedEventArgs e)
    {
        Preferences.Set(nameof(Preference.Mode), "DeansOffice");
        StudentConfig.IsVisible = false;
        TeacherConfig.IsVisible = false;
    }

    private async void UpdateSubjects(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        var allInOne = (await _databaseService.GetBlocks(picker?.SelectedItem.ToString()!,
            Preferences.Get(nameof(Preference.Course), string.Empty))).ToList();
        Preferences.Set(nameof(Preference.Course), picker?.SelectedItem.ToString());
        _views.ForEach(x => GroupsSelect.Remove(x));
        _views.Clear();
        AddSubjectPicker(SubjectType.Lecture, allInOne);
        AddSubjectPicker(SubjectType.Exercise, allInOne);
        AddSubjectPicker(SubjectType.ComputersLaboratory, allInOne);
        AddSubjectPicker(SubjectType.Laboratory, allInOne);
        AddSubjectPicker(SubjectType.Projects, allInOne);
        AddSubjectPicker(SubjectType.Seminars, allInOne);
        AddWfPicker(allInOne);
    }

    private void AddWfPicker(IEnumerable<StudentBlock> blocks)
    {
        var filteredBlocks = blocks.Where(x => x.Name == "WF").ToList();
        if (filteredBlocks.Count == 0) return;
        var picker = new Picker
        {
            ItemsSource = new List<string>
            {
                "Kobieta",
                "Mężczyzna"
            },
            WidthRequest = 120
        };
        picker.SelectedIndexChanged += GenderChanged;
        if (Preferences.ContainsKey("WF"))
            picker.SelectedIndex = Preferences.Get("WF", string.Empty).StartsWith("M") ? 1 : 0;
        else picker.SelectedIndex = 1;
        var layout = new HorizontalStackLayout
        {
            WidthRequest = 120,
            HorizontalOptions = LayoutOptions.Center,
            Children = { picker }
        };
        _views.Add(layout);
        GroupsSelect.Add(layout);
    }

    private static void GenderChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        Preferences.Set("WF", picker!.SelectedIndex == 1 ? "M" : "K");
    }

    private void AddSubjectPicker(SubjectType type, IEnumerable<StudentBlock> blocks)
    {
        var filteredBlocks = blocks.Where(x => x.Description == null && x.Group!.StartsWith((char)type) && x.Group!.Length != 1).ToList();
        if (filteredBlocks.Count == 0) return;
        var groupCount = filteredBlocks.Max(x => Convert.ToInt32(x.Group?.Substring(1, 2)));
        var list = new List<string>();
        for (var i = 1; i <= groupCount; i++) list.Add($"{(char)type}0{i}");
        var picker = new Picker
        {
            ItemsSource = list,
            WidthRequest = 100
        };
        var index = Preferences.Get(((char)type).ToString(), string.Empty).Last() - 49;
        if (index > groupCount - 1)
        {
            Preferences.Set(((char)type).ToString(), $"{(char)type}01");
            picker.SelectedIndex = 0;
        }
        else
            picker.SelectedIndex = index;

        picker.SelectedIndexChanged += PickerOnSelectedIndexChanged;


        var layout = new HorizontalStackLayout
        {
            WidthRequest = 100,
            HorizontalOptions = LayoutOptions.Center,
            Children = { picker }
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

    private void UpdateTeacher(object sender, EventArgs e)
    {
        var picker = sender as Picker;
        Preferences.Set(nameof(Preference.Teacher), picker?.SelectedItem.ToString());
        _databaseService.SaveTeacherBlocksToDb(Preferences.Get(nameof(Preference.Teacher), string.Empty));
    }

    private void ForceUpdate(object sender, EventArgs e)
    {
        var groups = _databaseService?.GetGroups(true).Result;
        GroupsPicker.SelectedIndexChanged -= UpdateSubjects;
        GroupsPicker.ItemsSource = groups!.Select(g => g.Key[..3]).Distinct().ToList();
        GroupsPicker.SelectedIndexChanged += UpdateSubjects;
        GroupsPicker.SelectedItem = Preferences.Get(nameof(Preference.Course), "11A");
    }
}