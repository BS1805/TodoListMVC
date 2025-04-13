
namespace TodoListMVC.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; }
    }

    public class CreateTodoItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; } = 2; // Default to Medium
    }

    public class UpdateTodoItemDto
    {
        public int Id { get; set; }  // Add Id property for identifying which task to update
        public string? Title { get; set; }  // Nullable, since user may not want to update the title
        public string? Description { get; set; }  // Nullable, description can be updated or not
        public bool? IsCompleted { get; set; }  // Nullable for optional update of completion status
        public DateTime? DueDate { get; set; }  // Nullable, the due date can be updated or not
        public int? Priority { get; set; }  // Nullable, priority can be updated or not
    }
}