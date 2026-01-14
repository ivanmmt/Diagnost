using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diagnost.Misc;
using Diagnost.Misc.Avalonia;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Diagnost.Views;

public partial class TeacherAuthView : CustomUC
{
    public TeacherAuthView()
    {
        InitializeComponent();
    }
    
    private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
    {
        var passwordBox = this.FindControl<TextBox>("PasswordInput"); 
        
        if (passwordBox != null)
        {
            if (passwordBox.PasswordChar == '*')
            {
                passwordBox.PasswordChar = '\0'; 
            }
            else
            {
                passwordBox.PasswordChar = '*';
            }
        }
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        string? authResult = await SessionContext.ApiService.Login(email.Text, PasswordInput.Text);
        if (authResult != null)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Помилка під час авторизації!", authResult, ButtonEnum.Ok);
            var result = await box.ShowAsPopupAsync(this);
            return;
        }

        if (this.Parent is ContentControl pageHost)
        {
            pageHost.Content = new TeacherMenuView();
        }
    }
}