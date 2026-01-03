using Avalonia.Controls;
using Avalonia.Interactivity;
using ListingMonitor.UI.ViewModels;

namespace ListingMonitor.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
    
    private void ClearFiltersButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectedSiteFilterIndex = 0;
            vm.FilterStartDate = null;
            vm.FilterEndDate = null;
            vm.RefreshListingsCommand.Execute(null);
        }
    }
}