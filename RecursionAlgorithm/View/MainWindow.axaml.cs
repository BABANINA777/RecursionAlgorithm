using Avalonia.Controls;
using RecursionAlgorithm.ViewModels;

namespace RecursionAlgorithm;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}