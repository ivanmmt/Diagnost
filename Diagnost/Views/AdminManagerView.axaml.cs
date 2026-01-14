using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Diagnost.Misc;
using Diagnost.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnost.Views;

public partial class AdminManagerView : UserControl
{
    private DataGrid _grid;
    private TextBox _tbEmail, _tbPass;
    private TextBlock _status;
    private List<RegisterResponse> _admins = new();

    public AdminManagerView()
    {
        InitializeComponent();
        _grid = this.FindControl<DataGrid>("AdminsGrid");
        _tbEmail = this.FindControl<TextBox>("NewEmail");
        _tbPass = this.FindControl<TextBox>("NewPass");
        _status = this.FindControl<TextBlock>("StatusMsg");
        
        this.FindControl<Button>("BackBtn").Click += (s, e) => { if (this.Parent is ContentControl p) p.Content = new TeacherMenuView(); };
        this.FindControl<Button>("AddBtn").Click += AddAdmin;

        LoadAdmins();
    }
    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async void LoadAdmins()
    {
        // Твой метод получения списка админов
        var list = await SessionContext.ApiService.GetAllAdmins();
        if (list != null)
        {
            _admins = list;
            _grid.ItemsSource = _admins;
        }
    }

    private async void AddAdmin(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_tbEmail.Text) || string.IsNullOrWhiteSpace(_tbPass.Text)) return;
        
        var res = await SessionContext.ApiService.RegisterAdmin(_tbEmail.Text, _tbPass.Text);
        if (res != null)
        {
            _status.Text = "✅ Успішно!";
            _status.Foreground = Brushes.Green;
            _status.IsVisible = true;
            _tbEmail.Clear(); _tbPass.Clear();
            LoadAdmins();
        }
        else
        {
            _status.Text = "❌ Помилка";
            _status.Foreground = Brushes.Red;
            _status.IsVisible = true;
        }
    }

    private async void OnDeleteAdminClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid idToDelete) // Используем GUID как в твоем коде
        {
            bool success = await SessionContext.ApiService.DeleteAdmin(idToDelete);
            if (success) LoadAdmins();
        }
    }
}