using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zadanie.Models;
using zadanie.Services;

namespace zadanie.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly DatabaseHelper _db = new DatabaseHelper();

        [ObservableProperty]
        private ObservableCollection<TaskItem> _tasks = new();

        [ObservableProperty]
        private string _newTaskTitle = string.Empty;

        [ObservableProperty]
        private string _newTaskDescription = string.Empty;

        public MainWindowViewModel()
        {
            _ = LoadTasksAsync();
        }

        public IAsyncRelayCommand LoadTasksCommand => new AsyncRelayCommand(LoadTasksAsync);
        public IAsyncRelayCommand AddTaskCommand => new AsyncRelayCommand(AddTaskAsync, () => !string.IsNullOrWhiteSpace(NewTaskTitle));
        public IAsyncRelayCommand<TaskItem?> DeleteTaskCommand => new AsyncRelayCommand<TaskItem?>(DeleteTaskAsync);
        public IAsyncRelayCommand<TaskItem?> ToggleCompleteCommand => new AsyncRelayCommand<TaskItem?>(ToggleCompleteAsync);

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
                Console.WriteLine($"Ошибка загрузки задач: {ex.Message}");
            }
        }

        private async Task AddTaskAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;
                var newTask = new TaskItem { Title = NewTaskTitle, Description = NewTaskDescription };
                await _db.AddTaskAsync(newTask);
                NewTaskTitle = "";
                NewTaskDescription = "";
                await LoadTasksAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления задачи: {ex.Message}");
            }
        }

        private async Task DeleteTaskAsync(TaskItem? task)
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

        private async Task ToggleCompleteAsync(TaskItem? task)
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
                Console.WriteLine($"Ошибка изменения статуса: {ex.Message}");
            }
        }
    }
}