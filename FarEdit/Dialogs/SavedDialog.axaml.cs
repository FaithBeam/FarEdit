using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace FarEdit.Dialogs;

public partial class SavedDialog : Window
{
    public SavedDialog()
    {
        InitializeComponent();
    }

    private void OkBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
