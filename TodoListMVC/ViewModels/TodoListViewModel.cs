using TodoListMVC.Models;

namespace TodoListMVC.ViewModels
{
    public class TodoListViewModel
    {
        public IEnumerable<TodoItem> Items { get; set; } = Array.Empty<TodoItem>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalItems { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}