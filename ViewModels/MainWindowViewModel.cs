using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using zadanie.Models;
using zadanie.Services;

namespace zadanie.ViewModels;

    public partial class MainWindowViewModel : ObservableObject
    {
    private readonly DatabaseHelper _db = new();
    [ObservableProperty] private ObservableCollection<TaskItem> _tasks = [];
    [ObservableProperty] private string _newTaskTitle = string.Empty;
    [ObservableProperty] private string _newTaskDescription = string.Empty;

    public MainWindowViewModel() => _ = LoadTasksAsync();

        public IAsyncRelayCommand LoadTasksCommand => new AsyncRelayCommand(LoadTasksAsync);
    public IAsyncRelayCommand AddTaskCommand => new AsyncRelayCommand(AddTaskAsync);

        private async Task LoadTasksAsync()
        {
            try
            {
                var tasks = await _db.GetTasksAsync();
                Tasks.Clear();
            foreach (var t in tasks) Tasks.Add(t);
                }
        catch (Exception ex) { Console.WriteLine($"Ошибка загрузки: {ex.Message}"); }
            }

        private async Task AddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;
            try
            {
                var newTask = new TaskItem { Title = NewTaskTitle, Description = NewTaskDescription };
                await _db.AddTaskAsync(newTask);
                NewTaskTitle = "";
                NewTaskDescription = "";

            // Временное сообщение об успехе (будет в консоли)
            Console.WriteLine($"✅ Задача '{newTask.Title}' успешно добавлена!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка: {ex.Message}");
            }
    }
}