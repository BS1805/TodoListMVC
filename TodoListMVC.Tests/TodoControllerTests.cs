using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TodoListMVC.Controllers;
using TodoListMVC.Models;
using TodoListMVC.Services;
using TodoListMVC.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
namespace TodoListMVC.Tests.Controllers
{
    public class TodoControllerTests
    {
        private readonly Mock<ITodoService> _mockTodoService;
        private readonly Mock<ILogger<TodoController>> _mockLogger;
        private readonly TodoController _controller;

        public TodoControllerTests()
        {
            // Setup mocks
            _mockTodoService = new Mock<ITodoService>();
            _mockLogger = new Mock<ILogger<TodoController>>();

            // Create controller with mocked dependencies
            _controller = new TodoController(_mockTodoService.Object, _mockLogger.Object);

            // Mock TempData
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        #region Index Tests
        [Fact]
        public async Task Index_ReturnsViewWithTodoListViewModel()
        {
            // Arrange
            int page = 1;
            int pageSize = 50;
            var todoItems = new List<TodoItem>
            {
                new TodoItem { Id = 1, Title = "Test Task 1", Description = "Test Description 1" },
                new TodoItem { Id = 2, Title = "Test Task 2", Description = "Test Description 2" }
            };
            int totalCount = 2;

            _mockTodoService
                .Setup(service => service.GetAllTodoItemsAsync(page, pageSize))
                .ReturnsAsync((todoItems, totalCount));

            // Act
            var result = await _controller.Index(page, pageSize);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<TodoListViewModel>(viewResult.Model);
            Assert.Equal(page, model.CurrentPage);
            Assert.Equal(pageSize, model.PageSize);
            Assert.Equal(totalCount, model.TotalItems);
            Assert.Equal(todoItems.Count, model.Items.Count());
        }

        [Fact]
        public async Task Index_WhenExceptionOccurs_ReturnsErrorView()
        {
            // Arrange
            _mockTodoService
                .Setup(service => service.GetAllTodoItemsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", (result as ViewResult).ViewName);
        }
        #endregion

        #region Details Tests
        [Fact]
        public async Task Details_WithValidId_ReturnsViewWithTodoItem()
        {
            // Arrange
            int id = 1;
            var todoItem = new TodoItem { Id = id, Title = "Test Task", Description = "Test Description" };

            _mockTodoService
                .Setup(service => service.GetTodoItemAsync(id))
                .ReturnsAsync(todoItem);

            // Act
            var result = await _controller.Details(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TodoItem>(viewResult.Model);
            Assert.Equal(id, model.Id);
            Assert.Equal(todoItem.Title, model.Title);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int id = 999;

            _mockTodoService
                .Setup(service => service.GetTodoItemAsync(id))
                .ReturnsAsync((TodoItem)null);

            // Act
            var result = await _controller.Details(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_WhenExceptionOccurs_ReturnsErrorView()
        {
            // Arrange
            int id = 1;

            _mockTodoService
                .Setup(service => service.GetTodoItemAsync(id))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Details(id);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", (result as ViewResult).ViewName);
        }
        #endregion

        #region Create Tests
        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var createDto = new CreateTodoItemDto
            {
                Title = "New Task",
                Description = "New Description",
                Priority = 1,
                DueDate = DateTime.Now.AddDays(1)
            };

            var createdTodoItem = new TodoItem
            {
                Id = 1,
                Title = createDto.Title,
                Description = createDto.Description,
                Priority = createDto.Priority,
                DueDate = createDto.DueDate
            };

            _mockTodoService
                .Setup(service => service.CreateTodoItemAsync(It.IsAny<CreateTodoItemDto>()))
                .ReturnsAsync(createdTodoItem); // Fix: Return a Task<TodoItem> instead of Task

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var createDto = new CreateTodoItemDto(); // Empty model will fail validation
            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateTodoItemDto>(viewResult.Model);
            Assert.Equal(createDto, model);
        }

        [Fact]
        public async Task Create_Post_WhenExceptionOccurs_ReturnsViewWithModelError()
        {
            // Arrange
            var createDto = new CreateTodoItemDto
            {
                Title = "New Task",
                Description = "New Description"
            };

            _mockTodoService
                .Setup(service => service.CreateTodoItemAsync(It.IsAny<CreateTodoItemDto>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<CreateTodoItemDto>(viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }
        #endregion

        #region Edit Tests
        [Fact]
        public async Task Edit_Get_WithValidId_ReturnsViewWithTodoItem()
        {
            // Arrange
            int id = 1;
            var todoItem = new TodoItem
            {
                Id = id,
                Title = "Test Task",
                Description = "Test Description",
                IsCompleted = false,
                Priority = 1,
                DueDate = DateTime.Now.AddDays(1)
            };

            _mockTodoService
                .Setup(service => service.GetTodoItemAsync(id))
                .ReturnsAsync(todoItem);

            // Act
            var result = await _controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateTodoItemDto>(viewResult.Model);
            Assert.Equal(todoItem.Title, model.Title);
            Assert.Equal(todoItem.Description, model.Description);
        }

        [Fact]
        public async Task Edit_Get_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int id = 999;

            _mockTodoService
                .Setup(service => service.GetTodoItemAsync(id))
                .ReturnsAsync((TodoItem)null);

            // Act
            var result = await _controller.Edit(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdateTodoItemDto
            {
                Id = id,
                Title = "Updated Task",
                Description = "Updated Description",
                IsCompleted = true,
                Priority = 2,
                DueDate = DateTime.Now.AddDays(2)
            };

            _mockTodoService
                .Setup(service => service.UpdateTodoItemAsync(id, It.IsAny<UpdateTodoItemDto>()))
                .ReturnsAsync(new TodoItem { Id = id });

            // Act
            var result = await _controller.Edit(id, updateDto);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Edit_Post_WithIdMismatch_ReturnsBadRequest()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdateTodoItemDto { Id = 2 };

            // Act
            var result = await _controller.Edit(id, updateDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            int id = 1;
            var updateDto = new UpdateTodoItemDto { Id = id };
            _controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = await _controller.Edit(id, updateDto);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateTodoItemDto>(viewResult.Model);
            Assert.Equal(updateDto, model);
        }
        #endregion

        #region Delete Tests
        [Fact]
        public async Task Delete_Get_WithValidId_ReturnsViewWithTodoItem()
        {
            // Arrange
            int id = 1;
            var todoItem = new TodoItem { Id = id, Title = "Test Task", Description = "Test Description" };

            _mockTodoService
                .Setup(service => service.GetTodoItemAsync(id))
                .ReturnsAsync(todoItem);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TodoItem>(viewResult.Model);
            Assert.Equal(id, model.Id);
            Assert.Equal(todoItem.Title, model.Title);
        }

        [Fact]
        public async Task Delete_Get_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int id = 999;

            _mockTodoService
                .Setup(service => service.GetTodoItemAsync(id))
                .ReturnsAsync((TodoItem)null);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            // Arrange
            int id = 1;

            _mockTodoService
                .Setup(service => service.DeleteTodoItemAsync(id))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteConfirmed(id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Contains("successfully deleted", _controller.TempData["Message"].ToString());
        }

        [Fact]
        public async Task DeleteConfirmed_WhenServiceReturnsFalse_RedirectsToIndexWithError()
        {
            // Arrange
            int id = 1;

            _mockTodoService
                .Setup(service => service.DeleteTodoItemAsync(id))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteConfirmed(id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Fix for NullReferenceException - check if TempData["Error"] exists first
            Assert.True(_controller.TempData.ContainsKey("Error"), "TempData should contain an Error message");
            var errorMessage = _controller.TempData["Error"]?.ToString();
            Assert.NotNull(errorMessage);
            Assert.Contains("Unable to delete", errorMessage);
        }
        #endregion


    }
}