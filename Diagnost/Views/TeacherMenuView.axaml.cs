using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Diagnost.Misc;

namespace Diagnost.Views;

public partial class TeacherMenuView : UserControl
{
    public TeacherMenuView()
    {
        InitializeComponent();
        
        this.FindControl<Button>("BtnStudents").Click += (s, e) => ChangeView(new StudentResultsView());
        this.FindControl<Button>("BtnTeachers").Click += (s, e) => ChangeView(new AdminManagerView());
        this.FindControl<Button>("BtnCodes").Click += (s, e) => ChangeView(new AccessCodeView());
        
        // ВАЖНО: Передаем true, так как переход осуществляется из админки
        this.FindControl<Button>("Trans").Click += (s, e) => ChangeView(new Form2View(isTeacher: true));
        
        this.FindControl<Button>("LogoutBtn").Click += async (s, e) => 
        {
            await SessionContext.ApiService.Logout();
            ChangeView(new TeacherAuthView());
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void ChangeView(UserControl newView)
    {
        if (this.Parent is ContentControl p) p.Content = newView;
    }
}