using TodoListMVC.Models;

namespace TodoListMVC.Services
{
    public interface ITodoService
    {
        Task<(IEnumerable<TodoItem> Items, int TotalCount)> GetAllTodoItemsAsync(int page = 1, int pageSize = 50);
        Task<TodoItem?> GetTodoItemAsync(int id);
        Task<TodoItem> CreateTodoItemAsync(CreateTodoItemDto createTodoItemDto);
        Task<TodoItem?> UpdateTodoItemAsync(int id, UpdateTodoItemDto updateTodoItemDto);
        Task<bool> DeleteTodoItemAsync(int id);
    }
}