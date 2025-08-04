using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IUserService _userService;

    public TasksController(ITaskService taskService, IUserService userService)
    {
        _taskService = taskService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskResponse>>> GetAllTasks(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskService.GetAllTasksAsync(cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponse>> GetTask(string id, CancellationToken cancellationToken = default)
    {
        var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var task = await _taskService.CreateTaskAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskResponse>> UpdateTask(string id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var task = await _taskService.UpdateTaskAsync(id, request, userId, cancellationToken);
        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(string id, CancellationToken cancellationToken = default)
    {
        var userId = _userService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _taskService.DeleteTaskAsync(id, userId, cancellationToken);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}/order")]
    public async Task<IActionResult> UpdateTaskOrder(string id, [FromBody] UpdateTaskOrderRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userService.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        request.TaskId = id; // Ensure the ID matches the route parameter
        var success = await _taskService.UpdateTaskOrderAsync(request, userId, cancellationToken);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<TaskResponse>> AddComment(string id, AddCommentRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = _userService.GetCurrentUser();
        if (currentUser == null)
            return Unauthorized();

        var task = await _taskService.AddCommentAsync(id, request, currentUser.Id, currentUser.Name, cancellationToken);
        if (task == null)
            return NotFound();

        return Ok(task);
    }
}
