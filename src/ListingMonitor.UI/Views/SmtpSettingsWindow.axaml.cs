using Avalonia.Controls;
using ListingMonitor.UI.ViewModels;

namespace ListingMonitor.UI.Views;

public partial class SmtpSettingsWindow : Window
{
    public SmtpSettingsWindow()
    {
        InitializeComponent();
    }

    public SmtpSettingsWindow(SmtpSettingsViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
