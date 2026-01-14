using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Diagnost.Helpers;
using Diagnost.Misc;
using Diagnost.Misc.Avalonia;
using Diagnost.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diagnost.Views;

public partial class Test3View : CustomUC
{
    private TextBlock? _label1;
    private Image? _displayImage; 
    private Border? _imageBorder; 
    private Button? _nextButton;
    private DispatcherTimer _timer1;
    private readonly Random _random = new Random();
    
    // --- Поля стану тесту ---
    private const int TotalStimuliCount = 120; // 120 стимулов по методике УФП
    private const int MinAllowedInterval = 100;  
    private const int IntervalChangeSuccess = 20; 
    private const int IntervalChangeError = 40;   
    
    private int _timeLeft = 0;
    private int _currentInterval = 800; 
    private int _stimuliCounter = 0; 
    private bool _isStimulusVisible = false; 
    private bool _hasClicked = false;
    private bool _flag = true;
    private bool _isTestRunning = false; // Блокировка кликов
    
    // --- Поля часу та статистики ---
    private readonly List<long> _reactionTimes = new(); 
    private readonly List<int> _findMinInter = new(); 
    private long _startTick; 
    private long _alltimeStart = 0;
    private long _alltimeEnd = 0;
    private long _allTimeOut = 0;
    private int _minIntervalReached = 700;
    
    private int _error = 0;
    private int _goodpage = 0;
    
    private int _currentType = 0; 
    private string _currentSvgPath = "";
    
    // --- Поле результату ---
    public DiagnosticResult CurrentResult { get; private set; }

    // --- Шляхи до SVG ---
    private readonly List<string> _leftSvgs = new() 
    { 
        "avares://Diagnost/SVGresource/Geo/square.svg", 
        "avares://Diagnost/SVGresource/Geo/circle.svg",
        "avares://Diagnost/SVGresource/Geo/cube.svg" 
    };
    private readonly List<string> _rightSvgs = new() 
    { 
        "avares://Diagnost/SVGresource/Anim/dog.svg",
        "avares://Diagnost/SVGresource/Anim/cat.svg",
        "avares://Diagnost/SVGresource/Anim/bird.svg" 
    };
    private readonly List<string> _skipSvgs = new() 
    { 
        "avares://Diagnost/SVGresource/Other/airplane.svg",
        "avares://Diagnost/SVGresource/Other/lock.svg",
        "avares://Diagnost/SVGresource/Other/phone.svg" 
    };

    // --- Конструктори ---
    public Test3View()
    {
        CurrentResult = new DiagnosticResult();
        InitializeComponent();
        InitializeControls();
        StartCountdown();
    }
    
    public Test3View(DiagnosticResult dataResult) : this()
    {
        CurrentResult = dataResult;
    }

    private void InitializeControls()
    {
        _label1 = this.FindControl<TextBlock>("DisplayText");
        _displayImage = this.FindControl<Image>("DisplayImage");
        _imageBorder = this.FindControl<Border>("ImageBorder");
        _nextButton = this.FindControl<Button>("NextButton");
        
        if (_nextButton != null) _nextButton.Click += NextButton_OnClick;
        this.PointerPressed += OnViewClick; 
        _timer1 = new DispatcherTimer();
    }
    
    private void InitializeComponent() { AvaloniaXamlLoader.Load(this); }
    
    private void StartCountdown()
    {
        _timeLeft = 3;
        _isTestRunning = true;
        if (_label1 != null) { _label1.IsVisible = true; _label1.Text = _timeLeft.ToString(); }
        if (_imageBorder != null) _imageBorder.IsVisible = false;
        
        _timer1.Interval = TimeSpan.FromMilliseconds(900);
        _timer1.Tick += CountdownTick;
        _timer1.Start();
    }

    private void CountdownTick(object? sender, EventArgs e)
    {
        if (_timeLeft >= 1)
        {
            if (_label1 != null) _label1.Text = _timeLeft + " ";
            _timeLeft--;
        }
        else
        {
            _timer1.Tick -= CountdownTick;
            if (_label1 != null) _label1.Text = ""; // Убираем цифру
            
            _alltimeStart = Environment.TickCount64;
            _timer1.Tick += Timer1_Tick;
            Timer1_Tick(null, EventArgs.Empty); // Запуск первого стимула
        }
    }
    
    private void Timer1_Tick(object? sender, EventArgs e)
    {
        if (_flag) // === ПОКАЗ СТИМУЛА ===
        {
            // Фиксация минимального интервала для статистики
            _findMinInter.Add(_currentInterval);
            if (_currentInterval < _minIntervalReached)
            {
                _minIntervalReached = _currentInterval;
                _allTimeOut = Environment.TickCount64;
            }
            
            // Визуализация
            if (_imageBorder != null)
            {
                _imageBorder.Background = Brushes.Transparent;
                _imageBorder.IsVisible = true;
            }
            
            // Задаем время экспозиции
            _timer1.Interval = TimeSpan.FromMilliseconds(_currentInterval);
            
            // Выбор типа стимула
            _currentType = _random.Next(0, 3);
            if (_currentType == 0) _currentSvgPath = _leftSvgs[_random.Next(_leftSvgs.Count)];
            else if (_currentType == 1) _currentSvgPath = _rightSvgs[_random.Next(_rightSvgs.Count)];
            else _currentSvgPath = _skipSvgs[_random.Next(_skipSvgs.Count)];
            
            // Выбор цвета
            int colorcase = _random.Next(3);
            byte r = 0, g = 0, b = 0;
            switch (colorcase) { case 0: r = 255; break; case 1: g = 255; break; case 2: b = 255; break; }
            
            SetImageSource(_currentSvgPath, r, g, b);
            
            _startTick = Environment.TickCount64;
            _flag = false; // Следующий шаг -> Скрытие
            _stimuliCounter++;
            _isStimulusVisible = true;
            _hasClicked = false;
            
            // Считаем общее количество целевых сигналов
            if (_currentType == 0 || _currentType == 1)
            {
                _goodpage++;
            }
        }
        else // === СКРЫТИЕ (Пауза) ===
        {
            // Проверка на ПРОПУСК
            if ((_currentType == 0 || _currentType == 1) && !_hasClicked)
            {
                // Если был целевой стимул, а клика не было -> увеличиваем интервал
                _currentInterval += IntervalChangeError;
            }

            _timer1.Interval = TimeSpan.FromMilliseconds(200); // Пауза 200 мс
            if (_imageBorder != null) _imageBorder.IsVisible = false;
            _isStimulusVisible = false;
            _flag = true; // Следующий шаг -> Показ
            
            if (_stimuliCounter >= TotalStimuliCount)
            {
                FinishTest();
            }
        }
    }
    
    private void SetImageSource(string uriPath, byte r, byte g, byte b)
    {
        if (_displayImage == null) return;
        try {
             var img = SvgHelper.LoadSvgWithColor(new Uri(uriPath), r, g, b);
             if (img != null) _displayImage.Source = img;
        } catch {}
    }

    private void OnViewClick(object? sender, PointerPressedEventArgs e)
    {
        // Блокировка: если тест не идет, стимул не виден или уже кликнули
        if (!_isTestRunning || !_isStimulusVisible || _hasClicked) return;
        
        var props = e.GetCurrentPoint(this).Properties;
        
        if (props.IsLeftButtonPressed)
        {
            _hasClicked = true;
            if (_currentType == 0) // Правильно (Геометрия)
            {
                _reactionTimes.Add(Environment.TickCount64 - _startTick);
                _currentInterval = Math.Max(MinAllowedInterval, _currentInterval - IntervalChangeSuccess);
            }
            else // Ошибка (нажали ЛКМ на Животное или Транспорт)
            {
                _error++;
                _currentInterval += IntervalChangeError;
            }
        }
        else if (props.IsRightButtonPressed)
        {
            _hasClicked = true;
            if (_currentType == 1) // Правильно (Животные)
            {
                _reactionTimes.Add(Environment.TickCount64 - _startTick);
                _currentInterval = Math.Max(MinAllowedInterval, _currentInterval - IntervalChangeSuccess);
            }
            else // Ошибка (нажали ПКМ на Геометрию или Транспорт)
            {
                _error++;
                _currentInterval += IntervalChangeError;
            }
        }
        
        // Таймер НЕ останавливаем. Картинка висит до конца _currentInterval.
    }

    private async void FinishTest()
    {
        _isTestRunning = false; // Блокируем клики
        _timer1.Stop();
        
        if (_imageBorder != null) _imageBorder.IsVisible = false;

        if (_reactionTimes.Count == 0)
        {
            CurrentResult.UFPLatet = 0;
            CurrentResult.UFP_StdDev_ms = 0;
            CurrentResult.UFP_ErrorsTotal = _goodpage;

            if (_label1 != null)
            {
                _label1.IsVisible = true;
                _label1.Text = "Тест не пройдений: немає реакцій!";
            }
            if (_nextButton != null) _nextButton.IsVisible = true;
            return;
        }

        _alltimeEnd = Environment.TickCount64;
        
        // Статистика
        double avgReaction = _reactionTimes.Average();
        double sumSquares = _reactionTimes.Sum(t => Math.Pow(t - avgReaction, 2));
        double stdDev = _reactionTimes.Count > 1 ? Math.Sqrt(sumSquares / (_reactionTimes.Count - 1)) : 0;
        
        // Общие ошибки = Пропуски + Неверные нажатия
        // Пропуски = (Всего целевых) - (Успешные нажатия)
        int errorsTotal = (_goodpage - _reactionTimes.Count) + _error; 

        long totalTimeSeconds = (_alltimeEnd - _alltimeStart) / 1000;
        long timeToPlatoSeconds = (_allTimeOut - _alltimeStart) / 1000;

        // --- Запись у Result ---
        CurrentResult.UFPLatet = Math.Round(avgReaction, 2);
        CurrentResult.UFP_StdDev_ms = Math.Round(stdDev, 4);
        CurrentResult.UFP_ErrorsTotal = errorsTotal;

        if (_label1 != null)
        {
            _label1.IsVisible = true;
            _label1.Foreground = Brushes.Black;
            _label1.FontSize = 18;
            _label1.Text = $"Тест 3 (УФП) Результати:\n" +
                          $"Латентність = {CurrentResult.UFPLatet} ms\n" +
                          $"Відхилення = {CurrentResult.UFP_StdDev_ms} ms\n" +
                          $"Загальні помилки = {CurrentResult.UFP_ErrorsTotal}\n" +
                          $"Мін. експозиція = {_minIntervalReached} ms\n" +
                          $"Загальний час = {totalTimeSeconds} с.\n" +
                          $"Час виходу на мін. експозицію = {timeToPlatoSeconds} с.";
        }
        if (_nextButton != null) _nextButton.IsVisible = true;
        // ===============
        // Writing to API
        await SessionContext.ApiService.VerifyResultExists(CurrentResult);

        bool putPV2_3 = await SessionContext.ApiService.PutUFP(new UFPResultRequest {
            AccessCode = SessionContext.AccessCode,
            ResultId = (long)SessionContext.ResultId,
            UFPLatet = CurrentResult.UFPLatet,
            UFP_StdDev_ms = CurrentResult.UFP_StdDev_ms,
            UFP_ErrorsTotal = CurrentResult.UFP_ErrorsTotal
            });
    }
    
    private void NextButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Передаємо оновлений CurrentResult (User Info + Test 1 Stats) далі
        if (this.Parent is ContentControl p) p.Content = new Form2View(CurrentResult); // Або наступний тест
    }
}