using Microsoft.AspNetCore.Mvc;
using TodoListMVC.Models;
using TodoListMVC.Services;
using TodoListMVC.ViewModels;

namespace TodoListMVC.Controllers
{
    public class TodoController : Controller
    {
        private readonly ITodoService _todoService;
        private readonly ILogger<TodoController> _logger;

        public TodoController(ITodoService todoService, ILogger<TodoController> logger)
        {
            _todoService = todoService;
            _logger = logger;
        }

        // GET: Todo/Index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 50)
        {
            try
            {
                var (items, totalCount) = await _todoService.GetAllTodoItemsAsync(page, pageSize);

                var viewModel = new TodoListViewModel
                {
                    Items = items,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize) // Calculate total pages
                };

                return View(viewModel); // Return the view with the updated data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching todo items");
                return View("Error"); // Show a generic error page in case of failure
            }
        }



        // GET: Todo/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var item = await _todoService.GetTodoItemAsync(id);

                if (item == null)
                {
                    return NotFound();
                }

                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching todo item with ID {id}");
                return View("Error");
            }
        }

        // GET: Todo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Todo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTodoItemDto createTodoItemDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create the new Todo item via the service
                    await _todoService.CreateTodoItemAsync(createTodoItemDto);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating todo item");
                    ModelState.AddModelError("", "An error occurred while creating the task.");
                }
            }

            return View(createTodoItemDto);
        }

        // GET: Todo/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var item = await _todoService.GetTodoItemAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                var updateDto = new UpdateTodoItemDto
                {
                    Title = item.Title,
                    Description = item.Description,
                    IsCompleted = item.IsCompleted,
                    DueDate = item.DueDate,
                    Priority = item.Priority
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching todo item for editing with ID {id}");
                return View("Error");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateTodoItemDto updateTodoItemDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure the ID matches
                    if (id != updateTodoItemDto.Id)
                    {
                        return BadRequest("ID mismatch.");
                    }

                    // Update the task status and other properties via the service
                    var result = await _todoService.UpdateTodoItemAsync(id, updateTodoItemDto);

                    // If the result is null, the item was not found
                    if (result == null)
                    {
                        return NotFound();
                    }

                    // Successfully updated, redirect to the Index view
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the error and add a model error to display in the view
                    _logger.LogError(ex, $"Error updating todo item with ID {id}");
                    ModelState.AddModelError("", "An error occurred while updating the task.");
                }
            }

            // If the model is invalid, return the same view with the current data
            return View(updateTodoItemDto);
        }



        // GET: Todo/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _todoService.GetTodoItemAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching todo item for deletion with ID {id}");
                return View("Error");
            }
        }

        // POST: Todo/GenerateRandomData
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRandomData()
        {
            try
            {
                // Generate 50 random todo items
                var random = new Random();
                var priorities = new[] { 1, 2, 3 }; // Low, Medium, High

                for (int i = 0; i < 50; i++)
                {
                    var createDto = new CreateTodoItemDto
                    {
                        Title = $"Random Task #{i + 1}",
                        Description = $"This is a randomly generated task #{i + 1} created on {DateTime.Now:g}",

                        Priority = priorities[random.Next(3)], // Random priority
                        DueDate = DateTime.Now.AddDays(random.Next(1, 31)) // Random due date within next month
                    };

                    await _todoService.CreateTodoItemAsync(createDto);
                }

                TempData["Message"] = "Successfully generated 50 random tasks!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating random todo items");
                TempData["Error"] = "An error occurred while generating random tasks.";
                return RedirectToAction(nameof(Index));
            }
        }



        // POST: Todo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _todoService.DeleteTodoItemAsync(id);
                if (result)
                {
                    TempData["Message"] = "Item successfully deleted.";
                }
                else
                {
                    TempData["Error"] = "Unable to delete item."; // Make sure this line is present
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting todo item with ID {id}");
                TempData["Error"] = "An error occurred while deleting the task.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
