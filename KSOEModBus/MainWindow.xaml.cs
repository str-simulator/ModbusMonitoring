using System.ComponentModel;
using System.Windows;
using KSOEModBus.ViewModels;

namespace KSOEModBus;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        Loaded -= OnLoaded;
        Closing -= OnClosing;
        await _viewModel.ShutdownAsync();
    }
}
