using CommunityToolkit.Mvvm.ComponentModel;

namespace zadanie.Models
{
    public partial class TaskItem : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private bool _isCompleted;
    }
}