using Avalonia;
using Avalonia.Controls;
using System;

namespace Diagnost.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        PageHost.Content = new UserTypeView();
    }
}