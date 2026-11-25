using System.Windows.Controls;
using System.Windows.Input;
using SpartonFaceAdmin.App.ViewModels;

namespace SpartonFaceAdmin.App.Views;

public partial class EnrollmentView : UserControl
{
    public EnrollmentView()
    {
        InitializeComponent();
    }

    private void Viewport_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && DataContext is EnrollmentViewModel vm)
        {
            vm.AdjustZoomByWheel(e.Delta);
            e.Handled = true;
        }
    }
}
