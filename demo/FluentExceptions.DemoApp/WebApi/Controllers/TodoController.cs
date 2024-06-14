using FluentExceptions.DemoApp.Domain.Entities;
using FluentExceptions.DemoApp.Domain.Repositories;
using FluentExceptions.DemoApp.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FluentExceptions.DemoApp.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodoController(TodoRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<Todo>> Get()
    {
        return await repository.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<Todo> Get(int id)
    {
        return await repository.GetById(id);
    }

    [HttpPost]
    public async Task<Todo> Post([FromBody] TodoModel body)
    {
        var todo = Todo.CreateNew(body.Title, body.Text);
        await repository.Insert(todo);
        return todo;
    }

    [HttpPut("{id}")]
    public async Task<Todo> Put(int id, [FromBody] TodoModel body)
    {
        var todo = await repository.GetById(id);
        todo.Update(body.Title, body.Text);
        await repository.Update(todo);
        return todo;
    }

    [HttpDelete("{id}")]
    public async Task Delete(int id)
    {
        var todo = await repository.GetById(id);
        await repository.Delete(todo);
    }
}
