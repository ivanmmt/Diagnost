using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Diagnost.Misc;
using Diagnost.Misc.Avalonia;
using Diagnost.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;

namespace Diagnost.Views;

public partial class Form1View : CustomUC
{
    // Об'єкт, який буде передано далі
    public DiagnosticResult CurrentResult { get; private set; } = new DiagnosticResult();
    

    public Form1View()
    {
        InitializeComponent();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        bool isAccessCodeExists = await SessionContext.ApiService.VerifyAccessCode((AccessCode.Text ?? string.Empty).Trim());
        if (!isAccessCodeExists)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Не правильний код доступу!", 
                "Код доступу не існує або не дійсний!",
                ButtonEnum.Ok);

            var result = await box.ShowAsPopupAsync(this);
            return;
        }

        bool hasError = false;

        AccessCode.ClearValue(TemplatedControl.BorderBrushProperty);
        NameBox.ClearValue(TemplatedControl.BorderBrushProperty);
        SurnameBox.ClearValue(TemplatedControl.BorderBrushProperty);
        SportBox.ClearValue(TemplatedControl.BorderBrushProperty);
        QualificationBox.ClearValue(TemplatedControl.BorderBrushProperty);
        GenderBox.ClearValue(TemplatedControl.BorderBrushProperty);

        // Перевірка
        if (string.IsNullOrWhiteSpace(AccessCode.Text)) { AccessCode.BorderBrush = Brushes.Red; hasError = true; }
        if (string.IsNullOrWhiteSpace(NameBox.Text)) { NameBox.BorderBrush = Brushes.Red; hasError = true; }
        if (string.IsNullOrWhiteSpace(SurnameBox.Text)) { SurnameBox.BorderBrush = Brushes.Red; hasError = true; }
        if (string.IsNullOrWhiteSpace(SportBox.Text)) { SportBox.BorderBrush = Brushes.Red; hasError = true; }
        if (string.IsNullOrWhiteSpace(QualificationBox.Text)) { QualificationBox.BorderBrush = Brushes.Red; hasError = true; }
        if (GenderBox.SelectedItem == null) { GenderBox.BorderBrush = Brushes.Red; hasError = true; }

        if (hasError)
            return;
        
        // --- 2. Збереження даних користувача у CurrentResult ---
        SessionContext.AccessCode = AccessCode.Text.Trim();
        CurrentResult.StudentFirstName = NameBox.Text.Trim();
        CurrentResult.StudentLastName = SurnameBox.Text.Trim();
        CurrentResult.SportType = SportBox.Text.Trim();
        CurrentResult.SportQualification = QualificationBox.Text.Trim();
        CurrentResult.Gender = (GenderBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
        CurrentResult.TestDate = DateTime.Now; 

        // --- 3. Перехід до Form2View, передаючи об'єкт результату ---
        if (this.Parent is ContentControl pageHost)
        {
            // Викликаємо конструктор Form2View з 1 аргументом
            pageHost.Content = new Form2View(CurrentResult); 
        }
    }
}