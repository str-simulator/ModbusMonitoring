using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KSOEModBus.Services;
using KSOEModBus.ViewModels;

namespace KSOEModBus;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private ScrollViewer? _logScrollViewer;
    private bool _isLogAutoScrollEnabled = true;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Logs.CollectionChanged += OnLogsCollectionChanged;
        LogListBox.Loaded += OnLogListBoxLoaded;
        LogListBox.LayoutUpdated += OnLogListBoxLayoutUpdated;
        await _viewModel.InitializeAsync();
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        DiagnosticLog.Write("Main window closing");
        _viewModel.Logs.CollectionChanged -= OnLogsCollectionChanged;
        LogListBox.Loaded -= OnLogListBoxLoaded;
        LogListBox.LayoutUpdated -= OnLogListBoxLayoutUpdated;
        Loaded -= OnLoaded;
        Closing -= OnClosing;
        await _viewModel.ShutdownAsync();
    }

    private void OnLogListBoxLoaded(object sender, RoutedEventArgs e)
    {
        _logScrollViewer ??= FindVisualChild<ScrollViewer>(LogListBox);
        if (_logScrollViewer is not null)
        {
            _logScrollViewer.ScrollChanged += OnLogScrollChanged;
            ScrollLogToEnd();
        }
    }

    private void OnLogListBoxLayoutUpdated(object? sender, EventArgs e)
    {
        if (_logScrollViewer is null)
        {
            _logScrollViewer = FindVisualChild<ScrollViewer>(LogListBox);
            if (_logScrollViewer is not null)
            {
                _logScrollViewer.ScrollChanged += OnLogScrollChanged;
                ScrollLogToEnd();
            }
        }
    }

    private void OnLogsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            _isLogAutoScrollEnabled = true;
        }

        if (_isLogAutoScrollEnabled)
        {
            Dispatcher.BeginInvoke((Action)ScrollLogToEnd);
        }
    }

    private void OnLogScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_logScrollViewer is null)
        {
            return;
        }

        _isLogAutoScrollEnabled = IsScrollAtBottom(_logScrollViewer);
    }

    private void ScrollLogToEnd()
    {
        if (_logScrollViewer is null)
        {
            return;
        }

        _logScrollViewer.ScrollToEnd();
        _isLogAutoScrollEnabled = true;
    }

    private static bool IsScrollAtBottom(ScrollViewer scrollViewer)
    {
        return scrollViewer.VerticalOffset + scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight - 1;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var index = 0; index < childCount; index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                return match;
            }

            var result = FindVisualChild<T>(child);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }
}
