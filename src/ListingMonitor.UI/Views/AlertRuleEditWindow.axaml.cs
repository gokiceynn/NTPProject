using Avalonia.Controls;
using ListingMonitor.UI.ViewModels;

namespace ListingMonitor.UI.Views;

public partial class AlertRuleEditWindow : Window
{
    public AlertRuleEditWindow()
    {
        InitializeComponent();
    }

    public AlertRuleEditWindow(AlertRuleEditViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
