using System.Collections.ObjectModel;
using System.Windows.Input;
using PomodoroWidget.Models;

namespace PomodoroWidget.ViewModels;

public class TodoViewModel : ViewModelBase
{
    public ObservableCollection<TodoItemViewModel> Todos { get; } = [];

    private string _newTaskText = string.Empty;
    public string NewTaskText
    {
        get => _newTaskText;
        set => SetProperty(ref _newTaskText, value);
    }

    private int _newTaskPriority = 1; // Medium
    public int NewTaskPriority
    {
        get => _newTaskPriority;
        set => SetProperty(ref _newTaskPriority, value);
    }

    private TodoItemViewModel? _linkedTask;
    public TodoItemViewModel? LinkedTask
    {
        get => _linkedTask;
        set
        {
            if (_linkedTask != null) _linkedTask.IsActive = false;
            SetProperty(ref _linkedTask, value);
            if (_linkedTask != null) _linkedTask.IsActive = true;
        }
    }

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleCompleteCommand { get; }
    public ICommand ClearCompletedCommand { get; }
    public ICommand LinkToTimerCommand { get; }
    public ICommand CyclePriorityCommand { get; }

    public TodoViewModel()
    {
        AddCommand = new RelayCommand(_ => AddTask());
        DeleteCommand = new RelayCommand(p => DeleteTask(p as TodoItemViewModel));
        ToggleCompleteCommand = new RelayCommand(p => ToggleComplete(p as TodoItemViewModel));
        ClearCompletedCommand = new RelayCommand(_ => ClearCompleted());
        LinkToTimerCommand = new RelayCommand(p => LinkToTimer(p as TodoItemViewModel));
        CyclePriorityCommand = new RelayCommand(p => CyclePriority(p as TodoItemViewModel));
    }

    private void AddTask()
    {
        var text = NewTaskText.Trim();
        if (string.IsNullOrEmpty(text)) return;
        var item = new TodoItem { Text = text, Priority = (Priority)NewTaskPriority };
        Todos.Insert(0, new TodoItemViewModel(item));
        NewTaskText = string.Empty;
    }

    private void DeleteTask(TodoItemViewModel? item)
    {
        if (item == null) return;
        if (LinkedTask == item) LinkedTask = null;
        Todos.Remove(item);
    }

    private void ToggleComplete(TodoItemViewModel? item)
    {
        if (item == null) return;
        item.IsCompleted = !item.IsCompleted;
    }

    private void ClearCompleted()
    {
        for (int i = Todos.Count - 1; i >= 0; i--)
        {
            if (Todos[i].IsCompleted)
            {
                if (LinkedTask == Todos[i]) LinkedTask = null;
                Todos.RemoveAt(i);
            }
        }
    }

    private void LinkToTimer(TodoItemViewModel? item)
    {
        if (item == null) return;
        LinkedTask = LinkedTask == item ? null : item;
    }

    private void CyclePriority(TodoItemViewModel? item)
    {
        if (item == null) return;
        item.Priority = item.Priority switch
        {
            Priority.Low => Priority.Medium,
            Priority.Medium => Priority.High,
            _ => Priority.Low
        };
    }

    public void LoadFrom(IEnumerable<TodoItem> items)
    {
        Todos.Clear();
        foreach (var item in items)
            Todos.Add(new TodoItemViewModel(item));
    }

    public List<TodoItem> ToModels() => Todos.Select(t => t.Model).ToList();

    public void IncrementLinkedPomodoro()
    {
        if (LinkedTask != null) LinkedTask.PomodoroCount++;
    }
}
