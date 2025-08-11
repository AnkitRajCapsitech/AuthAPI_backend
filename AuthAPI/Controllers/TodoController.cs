using AuthAPI.DTOs;
using AuthAPI.Models;
using AuthAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TodoController : ControllerBase
    {
        private readonly TodoService _todoService;

        public TodoController(TodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<ApiResponse<List<Todo>>> Get()
        {
            var res = new ApiResponse<List<Todo>>();
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var todos = await _todoService.GetTodosByUserIdAsync(userId);

                res.Message = "Data retrived successfully";
                res.Status = true;
                res.Result = todos;

            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Status = false;
            }
            return res;

        }

        [HttpPost]
        public async Task<ApiResponse<Todo>> Create([FromBody] TodoDTO dto)
        {
            var res = new ApiResponse<Todo>();
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var todo = new Todo
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = dto.Title,
                    IsCompleted = dto.IsCompleted
                };

                var createTodo = await _todoService.CreateTodoAsync(todo);
                res.Message = "Todo created successfully";
                res.Status = true;
                res.Result = createTodo;

            }
            catch (Exception ex)
            {
                res.Message = ex.Message;
                res.Status = false;
            }
            return res;
        }

        [HttpPut("{id}")]
        public async Task<ApiResponse<Object>>Update(string id, [FromBody] TodoDTO dto)
        {
            var res = new ApiResponse<object>();
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var updatedTodo = new Todo
                {
                    Id = id,
                    UserId = userId,
                    Title = dto.Title,
                    IsCompleted = dto.IsCompleted
                };

                var success = await _todoService.UpdateTodoAsync(id, userId, updatedTodo);
                if (success)
                {
                    res.Message = "Todo updated successfully";
                    res.Status = true;
                }
                else
                {
                    res.Message = "Todo not found or not updated";
                    res.Status = false;
                }
            }
            catch(Exception ex)
            {
                res.Message = ex.Message;
                res.Status = false;
            }

            return res;
        }

        [HttpDelete("{id}")]
        public async Task<ApiResponse<Object>>Delete(string id)
        {
            var res = new ApiResponse<object>();
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var success= await _todoService.DeleteTodoAsync(id, userId);

                if (success)
                {
                    res.Message = "Todo deleted successfully";
                    res.Status = true;
                }
                else
                {
                    res.Message = "Todo not found or not deleted";
                    res.Status = false;
                }

            }
            catch(Exception ex)
            {
                res.Message = ex.Message;
                res.Status = false;
            }

            return res;
        }
    }
}
