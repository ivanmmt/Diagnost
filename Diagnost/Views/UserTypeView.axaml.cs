using Avalonia.Controls;
using Diagnost.Misc.Avalonia;
using Diagnost.Views;

namespace Diagnost;

public partial class UserTypeView : CustomUC
{
    public UserTypeView()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) // Student
    {
        if (this.Parent is ContentControl pageHost)
        {
            pageHost.Content = new Form1View();
        }
    }

    private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e) // Teacher
    {
        if (this.Parent is ContentControl pageHost)
        {
            pageHost.Content = new TeacherAuthView();
        }
    }
}