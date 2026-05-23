using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using zadanie.Models;
using zadanie.Services;

namespace zadanie.ViewModels
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        private ObservableCollection<TaskItem> _tasks = new();
        private string _newTaskTitle = string.Empty;
        private string _newTaskDescription = string.Empty;

        public ObservableCollection<TaskItem> Tasks
        {
            get => _tasks;
            set { _tasks = value; OnPropertyChanged(); }
        }

        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set 
            { 
                _newTaskTitle = value; 
                OnPropertyChanged();
               
                (AddTaskCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set { _newTaskDescription = value; OnPropertyChanged(); }
        }

        public ICommand LoadTasksCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleCompleteCommand { get; }

        public MainWindowViewModel()
        {
            LoadTasksCommand = new RelayCommand(async () => await LoadTasksAsync());
            AddTaskCommand = new RelayCommand(async () => await AddTaskAsync(), () => !string.IsNullOrWhiteSpace(NewTaskTitle));
            DeleteTaskCommand = new RelayCommand<TaskItem>(async (task) => await DeleteTaskAsync(task));
            ToggleCompleteCommand = new RelayCommand<TaskItem>(async (task) => await ToggleCompleteAsync(task));

            _ = LoadTasksAsync();
        }

        private async Task LoadTasksAsync()
        {
            try
            {
                var tasks = await _db.GetTasksAsync();
                Tasks.Clear();
                foreach (var t in tasks)
                    Tasks.Add(t);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async Task AddTaskAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

                Console.WriteLine("=== Кнопка сработала, заголовок: " + NewTaskTitle);

                var newTask = new TaskItem { Title = NewTaskTitle, Description = NewTaskDescription };
                await _db.AddTaskAsync(newTask);

                NewTaskTitle = "";
                NewTaskDescription = "";
                await LoadTasksAsync();

                Console.WriteLine("=== Добавление успешно");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ОШИБКА: {ex.Message}");
                
            }
        }

        private async Task DeleteTaskAsync(TaskItem task)
        {
            if (task == null) return;
            try
            {
                await _db.DeleteTaskAsync(task.Id);
                await LoadTasksAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления: {ex.Message}");
            }
        }

        private async Task ToggleCompleteAsync(TaskItem task)
        {
            if (task == null) return;
            try
            {
                task.IsCompleted = !task.IsCompleted;
                await _db.UpdateTaskAsync(task);
                await LoadTasksAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка изменения: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
        public async void Execute(object? parameter) => await _execute();

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Func<T, Task> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || (parameter is T t && _canExecute(t));
        public async void Execute(object? parameter) => await _execute((T)parameter!);

        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}