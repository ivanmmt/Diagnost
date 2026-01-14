using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Diagnost.Misc.Avalonia;
using Diagnost.Models;

namespace Diagnost.Views;

public partial class Form2View : CustomUC
{
    private Border? _firstBorder;
    private Border? _secondBorder;
    private Border? _thirdBorder;
    private Border? _introBorder;
    private Button? _confirmButton;
    private Button? _backButton;
    
    // Кнопки вибору тестів
    private Button? _btnFirst;
    private Button? _btnSecond;
    private Button? _btnThird;

    private string _selectedMode = "";
    
    // Флаг для определения, учитель это или нет
    private bool _isTeacherMode; 

    public DiagnosticResult DataResult { get; private set; }

    // Основной конструктор принимает необязательный параметр isTeacher
    public Form2View(bool isTeacher = false)
    {
        _isTeacherMode = isTeacher; // Сохраняем статус
        DataResult = new DiagnosticResult();
        
        InitializeComponent();
        
        _firstBorder = this.FindControl<Border>("FirstBorder");
        _secondBorder = this.FindControl<Border>("SecondBorder");
        _thirdBorder = this.FindControl<Border>("ThirdBorder");
        _introBorder = this.FindControl<Border>("IntroBorder");
        _confirmButton = this.FindControl<Button>("ConfirmButton");
        _backButton = this.FindControl<Button>("Back");

        _btnFirst = this.FindControl<Button>("First");
        _btnSecond = this.FindControl<Button>("Second");
        _btnThird = this.FindControl<Button>("Third");
        
        if (_btnFirst != null) _btnFirst.Click += Button_OnClick;
        if (_btnSecond != null) _btnSecond.Click += Button_OnClick;
        if (_btnThird != null) _btnThird.Click += Button_OnClick;
        
        if (_confirmButton != null) _confirmButton.Click += ConfirmButton_OnClick;
        if (_backButton != null) _backButton.Click += Back_OnClick; // Подписываемся на клик
        
        if (_backButton != null)
        {
            _backButton.IsVisible = _isTeacherMode; 
            
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    // Конструктор для возврата с результатами (по умолчанию не учитель, если не передано иное)
    public Form2View(DiagnosticResult previousResult) : this(isTeacher: false) 
    {
        DataResult = previousResult;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        HideAll();

        if (button.Name != null)
        {
            _selectedMode = button.Name;
        }

        switch (_selectedMode)
        {
            case "First":
                if (_firstBorder != null) _firstBorder.IsVisible = true;
                break;
            case "Second":
                if (_secondBorder != null) _secondBorder.IsVisible = true;
                break; 
            case "Third":
                if (_thirdBorder != null) _thirdBorder.IsVisible = true;
                break;
        }
        
        if (_introBorder != null) _introBorder.IsVisible = false;
        
        if (_confirmButton != null) _confirmButton.IsVisible = true;

        // ВАЖНО: Показываем кнопку "Назад в меню" ТОЛЬКО если это учитель
        if (_backButton != null && _isTeacherMode) 
        {
            _backButton.IsVisible = true;
        }
    }

    private void HideAll()
    {
        if (_firstBorder != null) _firstBorder.IsVisible = false;
        if (_secondBorder != null) _secondBorder.IsVisible = false;
        if (_thirdBorder != null) _thirdBorder.IsVisible = false;
        if (_introBorder != null) _introBorder.IsVisible = false;
    }

    private void ConfirmButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_confirmButton != null) _confirmButton.IsVisible = false;
            
        if (this.Parent is ContentControl contentControl)
        {
            switch (_selectedMode)
            {
                case "First":
                    contentControl.Content = new Test1View(DataResult);
                    break;
                case "Second":
                    contentControl.Content = new Test2View(DataResult);
                    break;
                case "Third":
                    contentControl.Content = new Test3View(DataResult);
                    break;
            }
        }
    }

    // Обработчик кнопки возврата в меню учителя
    private void Back_OnClick(object? sender, RoutedEventArgs e)
    {
        // Дополнительная защита: если не учитель, ничего не делаем
        if (!_isTeacherMode) return;

        if (this.Parent is ContentControl pageHost)
        {
            pageHost.Content = new TeacherMenuView();
        }
    }
}