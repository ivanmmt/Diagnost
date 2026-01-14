using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diagnost.Misc;
using Diagnost.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diagnost.Views;

public partial class AccessCodeView : UserControl
{
    private List<AccessCodeResponse> _allCodes = new();
    private DataGrid _grid;
    private TextBlock _codeDisplay;

    public AccessCodeView()
    {
        InitializeComponent();
        _grid = this.FindControl<DataGrid>("CodesGrid");
        _codeDisplay = this.FindControl<TextBlock>("CodeDisplay");
        
        _ = LoadData();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async Task LoadData()
    {
        var codes = await SessionContext.ApiService.GetAllAccessCodes();
        if (codes != null)
        {
            _allCodes = codes;
            _grid.ItemsSource = null;
            _grid.ItemsSource = _allCodes;
        }
    }

    // Метод для кнопки "Згенерувати"
    private async void OnGenerateClick(object? sender, RoutedEventArgs e)
    {
        var response = await SessionContext.ApiService.CreateAccessCode();
        if (response != null)
        {
            _codeDisplay.Text = response.Code.ToUpper();
            await LoadData(); // Сразу обновляем таблицу
        }
    }

    // Метод для кнопки "Обновить"
    private async void OnRefreshClick(object? sender, RoutedEventArgs e) => await LoadData();

    // Исправленный метод удаления (теперь публичный для XAML)
    public async void OnDeleteCodeClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string codeToDelete)
        {
            btn.IsEnabled = false;
            bool success = await SessionContext.ApiService.DeleteAccessCode(codeToDelete);
            if (success)
            {
                var item = _allCodes.FirstOrDefault(x => x.Code == codeToDelete);
                if (item != null) _allCodes.Remove(item);
                _grid.ItemsSource = null;
                _grid.ItemsSource = _allCodes;
            }
            else btn.IsEnabled = true;
        }
    }

    private void OnBackClick(object? sender, RoutedEventArgs e)
    {
        if (this.Parent is ContentControl p) p.Content = new TeacherMenuView();
    }
}