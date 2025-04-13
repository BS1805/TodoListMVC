using System.Text;
using System.Text.Json;
using TodoListMVC.Models;
using Microsoft.Extensions.Logging;

namespace TodoListMVC.Services
{
    public class TodoService : ITodoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<TodoService> _logger;

        public TodoService(IConfiguration configuration, HttpClient httpClient, ILogger<TodoService> logger)
        {
            _httpClient = httpClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7146/api/TodoItems";  // Ensure the base URL is correct
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _logger = logger;
        }

        public async Task<(IEnumerable<TodoItem> Items, int TotalCount)> GetAllTodoItemsAsync(int page = 1, int pageSize = 50)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}?page={page}&pageSize={pageSize}");
                response.EnsureSuccessStatusCode();

                // Read total count from headers
                int totalCount = 0;
                if (response.Headers.TryGetValues("X-Total-Count", out var values))
                {
                    if (int.TryParse(values.FirstOrDefault(), out var count))
                    {
                        totalCount = count;
                    }
                }

                var content = await response.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<IEnumerable<TodoItem>>(content, _jsonOptions) ?? Array.Empty<TodoItem>();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching todo items from API.");
                throw new Exception("Failed to fetch todo items.", ex);
            }
        }

        public async Task<TodoItem?> GetTodoItemAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TodoItem>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching todo item with ID {id}.");
                throw new Exception("Failed to fetch todo item.", ex);
            }
        }

        public async Task<TodoItem> CreateTodoItemAsync(CreateTodoItemDto createTodoItemDto)
        {
            try
            {
                var json = JsonSerializer.Serialize(createTodoItemDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiBaseUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TodoItem>(responseContent, _jsonOptions)
                    ?? throw new Exception("Failed to deserialize the response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating todo item.");
                throw new Exception("Failed to create todo item.", ex);
            }
        }

        public async Task<TodoItem?> UpdateTodoItemAsync(int id, UpdateTodoItemDto updateTodoItemDto)
        {
            try
            {
                var json = JsonSerializer.Serialize(updateTodoItemDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{id}", content);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TodoItem>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating todo item with ID {id}.");
                throw new Exception("Failed to update todo item.", ex);
            }
        }

        public async Task<bool> DeleteTodoItemAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting todo item with ID {id}.");
                throw new Exception("Failed to delete todo item.", ex);
            }
        }
    }
}
