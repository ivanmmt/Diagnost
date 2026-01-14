using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Diagnost.Models; 
using Diagnost.Helpers;
using Diagnost.Misc.Avalonia;
using Diagnost.Misc;

namespace Diagnost.Views;

public partial class Test1View : CustomUC 
{
    private TextBlock? _label1;
    private Border? _stimulusBorder;
    private Image? _stimulusImage;
    private Button? _nextButton;
    private DispatcherTimer _timer;             
    private readonly Random _random = new Random(); 
    private const int TotalAttempts = 30;
    
    private int _currentAttempt = 0;
    private long _startTime;
    private readonly List<long> _reactionTimes = new List<long>();
    private int _errors = 0;
    private bool _isStimulusVisible = false;
    private bool _isTestRunning = false;
    private bool _wasClickedInCurrentAttempt = false; // Флаг, чтобы считать только один клик за показ

    private readonly List<string> _svgPaths = new List<string>
    {
        "avares://Diagnost/SVGresource/Geo/square.svg",
        "avares://Diagnost/SVGresource/Geo/circle.svg",
        "avares://Diagnost/SVGresource/Geo/cube.svg",
        "avares://Diagnost/SVGresource/Anim/dog.svg",
        "avares://Diagnost/SVGresource/Anim/cat.svg",
        "avares://Diagnost/SVGresource/Anim/bird.svg",
        "avares://Diagnost/SVGresource/Other/airplane.svg",
        "avares://Diagnost/SVGresource/Other/lock.svg",
        "avares://Diagnost/SVGresource/Other/phone.svg" 
    };

    public DiagnosticResult CurrentResult { get; private set; }
    
    public Test1View()
    {
        CurrentResult = new DiagnosticResult();
        InitializeComponent();
        InitializeControls();
        StartCountdown();
    }
    
    public Test1View(DiagnosticResult previousResult) : this() 
    {
        CurrentResult = previousResult; 
    }

    private void InitializeComponent() { AvaloniaXamlLoader.Load(this); }
    
    private void InitializeControls()
    {
        _label1 = this.FindControl<TextBlock>("DisplayText");
        _stimulusBorder = this.FindControl<Border>("StimulusBorder");
        _nextButton = this.FindControl<Button>("NextButton");
        _stimulusImage = this.FindControl<Image>("StimulusImage");
        
        if (_nextButton != null) _nextButton.Click += NextButton_OnClick;
        this.PointerPressed += OnViewPointerPressed;
        _timer = new DispatcherTimer();
    }

    private void StartCountdown()
    {
        if (_stimulusBorder != null) _stimulusBorder.IsVisible = false;
        if (_label1 != null) { _label1.IsVisible = true; _label1.Text = "3"; }
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += CountdownTick;
        _timer.Start();
    }

    private void CountdownTick(object? sender, EventArgs e)
    {
        int t = 0;
        int.TryParse(_label1?.Text, out t);
        if (t > 0) 
        { 
            t--; 
            if (_label1 != null) _label1.Text = t > 0 ? t.ToString() : ""; 
        }
        
        if (t <= 0) {
            _timer.Tick -= CountdownTick;
            if (_label1 != null) _label1.IsVisible = false;
            _isTestRunning = true;
            _timer.Tick += Timer_Tick;
            PrepareNextAttempt();
        }
    }

    // Единственное место, где происходит ПЕРЕКЛЮЧЕНИЕ состояний
    private void Timer_Tick(object? sender, EventArgs e)
    {
        _timer.Stop();
        if (!_isStimulusVisible) 
        {
            ShowStimulus();
        }
        else 
        {
            // Время показа картинки истекло
            if (!_wasClickedInCurrentAttempt)
            {
                _errors++; // Если за время показа не кликнули - ошибка
            }
            
            _isStimulusVisible = false;
            if (_stimulusBorder != null) _stimulusBorder.IsVisible = false;
            PrepareNextAttempt();
        }
    }
    
    private void ShowStimulus()
    {
        _wasClickedInCurrentAttempt = false; // Сбрасываем флаг клика для новой картинки

        if (_stimulusImage != null) 
        {
            string path = _svgPaths[_random.Next(_svgPaths.Count)];
            byte r = 0, g = 0, b = 0;
            int colorCase = _random.Next(1, 4);
            switch (colorCase) {
                case 1: r = 255; break; 
                case 2: g = 255; break; 
                case 3: b = 255; break; 
            }
            _stimulusImage.Source = SvgHelper.LoadSvgWithColor(new Uri(path), r, g, b);
        }

        if (_stimulusBorder != null) _stimulusBorder.IsVisible = true;
        _isStimulusVisible = true;
        _startTime = Environment.TickCount64;
        
        _timer.Interval = TimeSpan.FromMilliseconds(800); 
        _timer.Start();
    }
    
    private void PrepareNextAttempt()
    {
        _currentAttempt++; 
        if (_currentAttempt > TotalAttempts) { FinishTest(); return; }
        
        if (_stimulusBorder != null) _stimulusBorder.IsVisible = false;
        
        // Пауза между стимулами (черный экран)
        _timer.Interval = TimeSpan.FromMilliseconds(_random.Next(700, 1500)); 
        _timer.Start();
    }
    
    private void OnViewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_isTestRunning || _wasClickedInCurrentAttempt) return;

        if (_isStimulusVisible) 
        {
            // Регистрируем время, но НЕ ОСТАНАВЛИВАЕМ таймер. 
            // Картинка исчезнет сама, когда сработает Timer_Tick
            _reactionTimes.Add(Environment.TickCount64 - _startTime);
            _wasClickedInCurrentAttempt = true;
        } 
        else if (_timer.IsEnabled) 
        {
            // Клик в пустой промежуток (фальстарт)
            _errors++; 
            // В случае фальстарта можно либо ждать следующей картинки, 
            // либо форсировать следующую попытку. Оставим ожидание следующей картинки.
            _wasClickedInCurrentAttempt = true; 
        }
    }

    private async void FinishTest()
    {
        _timer.Stop();
        _isTestRunning = false;

        var calculatedStats = StatisticsCalculator.CalculatePzmr(_reactionTimes, _errors);

        CurrentResult.PZMRLatet = calculatedStats.PZMRLatet;
        CurrentResult.PZMRvidhil = calculatedStats.PZMRvidhil;
        CurrentResult.PZMR_ErrorsTotal = calculatedStats.PZMR_ErrorsTotal;

        if (_label1 != null) {
            _label1.IsVisible = true;
            _label1.FontSize = 24;
            _label1.Text = $"РЕЗУЛЬТАТ ТЕСТУ 1:\n" +
                           $"Латентність: {CurrentResult.PZMRLatet} мс\n" +
                           $"Відхилення: {CurrentResult.PZMRvidhil}\n" +
                           $"Помилок: {CurrentResult.PZMR_ErrorsTotal}";
        }
        if (_nextButton != null) _nextButton.IsVisible = true;
        // ===============
        // Writing to APIice();
        await SessionContext.ApiService.VerifyResultExists(CurrentResult);

        bool putPzmrSuccess = await SessionContext.ApiService.PutPZMR(new PZMRResultRequest {
            AccessCode = SessionContext.AccessCode,
            ResultId = (long)SessionContext.ResultId,
            PZMRLatet = CurrentResult.PZMRLatet,
            PZMRvidhil = CurrentResult.PZMRvidhil,
            PZMR_ErrorsTotal = CurrentResult.PZMR_ErrorsTotal
            });
    }
    
    private void NextButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
         // Передаємо оновлений CurrentResult (User Info + Test 1 Stats) далі
         if (this.Parent is ContentControl p) p.Content = new Form2View(CurrentResult); // Або наступний тест
    }
}