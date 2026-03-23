using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PomodoroWidget.ViewModels;

namespace PomodoroWidget.Views;

public partial class TodoListView : UserControl
{
    public TodoListView()
    {
        InitializeComponent();
    }

    private void TaskInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is TodoViewModel vm)
            vm.AddCommand.Execute(null);
    }

    private void TaskText_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2 && sender is FrameworkElement fe && fe.DataContext is TodoItemViewModel item)
            item.IsEditing = true;
    }

    private void EditBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TodoItemViewModel item)
            item.IsEditing = false;
    }

    private void EditBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is FrameworkElement fe && fe.DataContext is TodoItemViewModel item)
            item.IsEditing = false;
    }

    private void EditBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb) { tb.Focus(); tb.SelectAll(); }
    }

    private void PriorityDot_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TodoItemViewModel item
            && DataContext is TodoViewModel vm)
            vm.CyclePriorityCommand.Execute(item);
    }
}
