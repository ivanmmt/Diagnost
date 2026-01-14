using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diagnost.Misc;
using Diagnost.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnost.Views;

public partial class StudentResultsView : UserControl
{
    private DataGrid _grid;
    private TextBox _searchBox;
    private ComboBox _groupFilter;
    private TextBlock _countLabel;

    private List<ResultResponse> _allResults = new();

    public StudentResultsView()
    {
        InitializeComponent();
        _grid = this.FindControl<DataGrid>("ResultsGrid");
        _searchBox = this.FindControl<TextBox>("SearchBox");
        _groupFilter = this.FindControl<ComboBox>("GroupFilter");
        _countLabel = this.FindControl<TextBlock>("CountLabel");

        this.FindControl<Button>("BackBtn").Click += (s, e) => { if (this.Parent is ContentControl p) p.Content = new TeacherMenuView(); };
        this.FindControl<Button>("RefreshBtn").Click += (s, e) => LoadData();

        _searchBox.KeyUp += (s, e) => FilterData();
        _groupFilter.SelectionChanged += (s, e) => FilterData();

        LoadData();
    }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async void LoadData()
    {
        var (results, err) = await SessionContext.ApiService.GetResults();
        if (results != null && results.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: First Result PZMR = {results[0].PZMRLatet}");
            
            _allResults = results;
            
            // Загружаем список групп
            var groups = _allResults.Select(r => r.Group).Distinct().OrderBy(g => g).ToList();
            groups.Insert(0, "Всі");
            
            var current = _groupFilter.SelectedItem as string;
            _groupFilter.ItemsSource = groups;
            _groupFilter.SelectedItem = current ?? "Всі";

            FilterData();
        }
    }

    private void FilterData()
    {
        var query = _allResults.AsEnumerable();

        // Поиск по имени
        if (!string.IsNullOrWhiteSpace(_searchBox.Text))
            query = query.Where(r => r.StudentFullName != null && r.StudentFullName.Contains(_searchBox.Text, StringComparison.OrdinalIgnoreCase));

        // Фильтр по группе
        if (_groupFilter.SelectedItem is string grp && grp != "Всі")
            query = query.Where(r => r.Group == grp);

        var final = query.OrderBy(r => r.Group).ToList();
        _grid.ItemsSource = final;
        _countLabel.Text = $"{final.Count} записів";
    }

    private async void OnDeleteResultClick(object? sender, RoutedEventArgs e)
    {
        // Тут ID типа long, потому что у тебя в модели ResultResponse Id - это long
        if (sender is Button btn && btn.Tag is long idToDelete)
        {
            btn.IsEnabled = false; // Блокируем кнопку
            
            // Вызываем НАШ НОВЫЙ метод DeleteResult
            bool success = await SessionContext.ApiService.DeleteResult(idToDelete);
            
            if (success)
            {
                var item = _allResults.FirstOrDefault(x => x.Id == idToDelete);
                if (item != null) _allResults.Remove(item);
                FilterData();
            }
            else
            {
                btn.IsEnabled = true; // Возвращаем если ошибка
            }
        }
    }
}