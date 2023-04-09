using Avalonia.Controls;
using DiagramClassEditor.ViewModels;

namespace DiagramClassEditor.Views {
    public partial class MainWindow: Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);
        }
    }
}