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

public partial class Test2View : CustomUC
{
    private TextBlock? _label1;
    private Image? _displayImage; 
    private Border? _imageBorder; 
    private Button? _nextButton;
    private DispatcherTimer _timer1;
    private readonly Random _random = new Random();
    
    // --- Поля стану тесту ---
    private const int TotalAttempts = 120; // Количество попыток
    private int _currentInterval = 800;    // Стартовый интервал (мс)
    private const int MinAllowedInterval = 100;
    private const int IntervalChangeSuccess = 20; // На сколько ускорять при успехе
    private const int IntervalChangeError = 40;   // На сколько замедлять при ошибке
    
    private int _timeLeft = 0;
    private int _currentAttempt = 0; 
    private bool _isStimulusVisible = false; 
    private bool _hasClicked = false; 
    private bool _flag = true; // true = готовим показ, false = скрываем
    private bool _isTestRunning = false; // Блокировка кликов после теста
    
    // --- Поля часу та статистики ---
    private long _alltimeStart = 0;
    private long _alltimeEnd = 0;
    private long _allTimeOut = 0; // Время выхода на плато (минимальный интервал)
    
    private List<long> _reactionTimes = new List<long>(); 
    private long _startTick; 
    private int _error = 0;     // Ошибки (нажал не то)
    private int _misses = 0;    // Пропуски (не нажал вовсе)
    private int _goodpage = 0;  // Количество целевых стимулов (квадрат/круг/куб или животные)
    private int _currentType = 0; // 0 = Гео (ЛКМ), 1 = Аним (ПКМ), 2 = Транспорт (Пропуск)
    
    private readonly List<int> _findMinInter = new List<int>(); 
    private int _minIntervalReached = 700; 
    
    public DiagnosticResult CurrentResult { get; private set; }

    // --- Шляхи до SVG ---
    // ЛЕВАЯ кнопка (Геометрия)
    private readonly List<string> _leftSvgs = new() 
    { 
        "avares://Diagnost/SVGresource/Geo/square.svg",
        "avares://Diagnost/SVGresource/Geo/circle.svg",
        "avares://Diagnost/SVGresource/Geo/cube.svg"
    };
    // ПРАВАЯ кнопка (Животные)
    private readonly List<string> _rightSvgs = new() 
    { 
        "avares://Diagnost/SVGresource/Anim/dog.svg",
        "avares://Diagnost/SVGresource/Anim/cat.svg",
        "avares://Diagnost/SVGresource/Anim/bird.svg"
    };
    // ПРОПУСТИТЬ (Транспорт)
    private readonly List<string> _skipSvgs = new() 
    { 
        "avares://Diagnost/SVGresource/Other/airplane.svg",
        "avares://Diagnost/SVGresource/Other/lock.svg",
        "avares://Diagnost/SVGresource/Other/phone.svg"
    };
    private string _currentSvgPath = "";

    // --- Конструктори ---
    public Test2View()
    {
        CurrentResult = new DiagnosticResult();
        InitializeComponent();
        InitializeControls();
        StartCountdown();
    }

    public Test2View(DiagnosticResult dataResult) : this()
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
        _isTestRunning = true; // Разрешаем тест
        if (_label1 != null) { _label1.IsVisible = true; _label1.Text = _timeLeft.ToString(); }
        if (_imageBorder != null) _imageBorder.IsVisible = false;

        _timer1.Interval = TimeSpan.FromSeconds(1);
        _timer1.Start();
        _timer1.Tick += CountdownTick;
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
            if (_label1 != null) _label1.Text = ""; // Убираем цифры, но не скрываем контрол полностью (по желанию)
            
            _alltimeStart = Environment.TickCount64;
            _timer1.Tick += Timer1_Tick;
            // Сразу запускаем первый тик
            Timer1_Tick(null, EventArgs.Empty);
        }
    }

    private void Timer1_Tick(object? sender, EventArgs e)
    {
        if (_flag) // === ФАЗА ПОКАЗА СТИМУЛА ===
        {
            // 1. Фиксируем минимальный интервал
            _findMinInter.Add(_currentInterval);
            if (_currentInterval < _minIntervalReached)
            {
                _minIntervalReached = _currentInterval;
                _allTimeOut = Environment.TickCount64;
            }

            // 2. Визуализация
            if (_imageBorder != null)
            {
                _imageBorder.Background = Brushes.Transparent;
                _imageBorder.IsVisible = true;
            }

            // 3. Устанавливаем длительность показа
            _timer1.Interval = TimeSpan.FromMilliseconds(_currentInterval);
            
            // 4. Выбор типа стимула
            _currentType = _random.Next(0, 3); // 0, 1 или 2
            if (_currentType == 0) _currentSvgPath = _leftSvgs[_random.Next(_leftSvgs.Count)];
            else if (_currentType == 1) _currentSvgPath = _rightSvgs[_random.Next(_rightSvgs.Count)];
            else _currentSvgPath = _skipSvgs[_random.Next(_skipSvgs.Count)];
            
            // 5. Цвет
            var colorcase = _random.Next(3);
            byte r = 0, g = 0, b = 0;
            switch (colorcase) { case 0: r = 255; break; case 1: g = 255; break; case 2: b = 255; break; }
            SetImageSource(_currentSvgPath, r, g, b);

            // 6. Сброс состояния для попытки
            _startTick = Environment.TickCount64;
            _flag = false; // Следующий тик будет "Скрытие"
            _currentAttempt++;
            _isStimulusVisible = true;
            _hasClicked = false;
            
            if (_currentType == 0 || _currentType == 1)
            {
                _goodpage++; // Это был целевой стимул
            }
        }
        else // === ФАЗА СКРЫТИЯ (Пауза) ===
        {
            // Проверка на ПРОПУСК (если картинка ушла, а клика не было)
            if ((_currentType == 0 || _currentType == 1) && !_hasClicked)
            {
                // Пропустили целевой стимул -> замедляем
                _currentInterval += IntervalChangeError;
                // Можно считать как ошибку пропуска, если нужно для статистики
                // _misses++; // Рассчитывается в конце через _goodpage - reactionTimes.Count
            }

            _timer1.Interval = TimeSpan.FromMilliseconds(500); // Пауза между картинками
            if (_imageBorder != null) _imageBorder.IsVisible = false;
            _isStimulusVisible = false;
            _flag = true; // Следующий тик будет "Показ"
            
            if (_currentAttempt >= TotalAttempts)
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
        // Блокировка: если тест не идет, стимул не виден или уже кликали в эту попытку
        if (!_isTestRunning || !_isStimulusVisible || _hasClicked) return;
        
        var props = e.GetCurrentPoint(this).Properties;
        bool isActionTaken = false;

        // ЛЕВАЯ КНОПКА (нужна для Типа 0)
        if (props.IsLeftButtonPressed)
        {
            _hasClicked = true;
            isActionTaken = true;
            
            if (_currentType == 0) // Правильно!
            {
                _reactionTimes.Add(Environment.TickCount64 - _startTick);
                _currentInterval = Math.Max(MinAllowedInterval, _currentInterval - IntervalChangeSuccess);
            }
            else // Ошибка (нажали левую на Тип 1 или 2)
            {
                _error++;
                _currentInterval += IntervalChangeError;
            }
        }
        // ПРАВАЯ КНОПКА (нужна для Типа 1)
        else if (props.IsRightButtonPressed)
        {
            _hasClicked = true;
            isActionTaken = true;

            if (_currentType == 1) // Правильно!
            {
                _reactionTimes.Add(Environment.TickCount64 - _startTick);
                _currentInterval = Math.Max(MinAllowedInterval, _currentInterval - IntervalChangeSuccess);
            }
            else // Ошибка (нажали правую на Тип 0 или 2)
            {
                _error++;
                _currentInterval += IntervalChangeError;
            }
        }
        
        // ВАЖНО: Мы НЕ вызываем _timer1.Stop(). Картинка висит до конца _currentInterval.
    }

    private async void FinishTest()
    {
        _isTestRunning = false; // ЗАПРЕЩАЕМ ЛЮБЫЕ КЛИКИ
        _timer1.Stop();
        
        if (_imageBorder != null) _imageBorder.IsVisible = false;

        if (_reactionTimes.Count == 0)
        {
            if (_label1 != null)
            {
                _label1.IsVisible = true;
                _label1.Text = "Тест не пройдено: немає вірних реакцій!";
            }
            if (_nextButton != null) _nextButton.IsVisible = true;
            return;
        }

        _alltimeEnd = Environment.TickCount64;
        long totalTimeSeconds = (_alltimeEnd - _alltimeStart) / 1000;

        // Расчет статистики
        double avgReaction = _reactionTimes.Average();
        
        // Среднеквадратическое отклонение
        double sumSquares = _reactionTimes.Sum(t => Math.Pow(t - avgReaction, 2));
        double stdDev = _reactionTimes.Count > 1 ? Math.Sqrt(sumSquares / (_reactionTimes.Count - 1)) : 0;
        
        // Пропуски = (Всего целевых показов) - (Сколько раз успешно нажали)
        int misses = _goodpage - _reactionTimes.Count;
        if (misses < 0) misses = 0; // На всякий случай

        // Запись результатов
        CurrentResult.PV2_3Latet = Math.Round(avgReaction, 2);
        CurrentResult.PV2_StdDev_ms = Math.Round(stdDev, 4);
        CurrentResult.PV2_ErrorsMissed = misses;
        CurrentResult.PV2_ErrorsWrongButton = _error;
        CurrentResult.PV2_ErrorsTotal = misses + _error;

        if (_label1 != null)
        {
            _label1.IsVisible = true;
            _label1.Foreground = Brushes.Black;
            _label1.FontSize = 18;
            _label1.Text = $"Середній час реакції = {CurrentResult.PV2_3Latet} мс\n" +
                           $"Всього помилок = {CurrentResult.PV2_ErrorsTotal} " +
                           $"(Пропуски: {misses}, Хибні: {_error})\n" +
                           $"Відхилення = {CurrentResult.PV2_StdDev_ms} мс\n" +
                           $"Мін. експозиція = {_minIntervalReached} мс\n" +
                           $"Загальний час = {totalTimeSeconds} с.";
        }
        if (_nextButton != null) _nextButton.IsVisible = true;

        // ===============
        // Writing to API
        ApiService api = new ApiService();

        await SessionContext.ApiService.VerifyResultExists(CurrentResult);

        bool putPV2_3 = await SessionContext.ApiService.PutPV2_3(new PV2_3ResultRequest {
            AccessCode = SessionContext.AccessCode,
            ResultId = (long)SessionContext.ResultId,
            PV2_3Latet = CurrentResult.PV2_3Latet,
            PV2_StdDev_ms = CurrentResult.PV2_StdDev_ms,
            PV2_ErrorsTotal = CurrentResult.PV2_ErrorsTotal,
            PV2_ErrorsMissed = CurrentResult.PV2_ErrorsMissed,
            PV2_ErrorsWrongButton = CurrentResult.PV2_ErrorsWrongButton,
            PV2_ErrorsFalseAlarm = CurrentResult.PV2_ErrorsFalseAlarm
        });
    }

    private void NextButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Передаємо оновлений CurrentResult (User Info + Test 1 Stats) далі
        if (this.Parent is ContentControl p) p.Content = new Form2View(CurrentResult); // Або наступний тест
    }
}