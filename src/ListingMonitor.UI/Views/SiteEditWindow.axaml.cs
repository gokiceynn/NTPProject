using Avalonia.Controls;
using Avalonia.Interactivity;
using ListingMonitor.UI.ViewModels;

namespace ListingMonitor.UI.Views;

public partial class SiteEditWindow : Window
{
    public SiteEditWindow()
    {
        InitializeComponent();
    }

    public SiteEditWindow(SiteEditViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
