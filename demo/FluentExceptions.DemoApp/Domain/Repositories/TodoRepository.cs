using FluentExceptions.DemoApp.Domain.Entities;
using FluentExceptions.DemoApp.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FluentExceptions.DemoApp.Domain.Repositories;

public class TodoRepository(TodoDataContext dataContext)
{
    public async Task<IReadOnlyCollection<Todo>> GetAll()
    {
        var entities = await dataContext.Todos.ToListAsync();
        return entities;
    }

    public async Task<Todo> GetById(int id)
    {
        var entity = await dataContext.Todos.FindAsync(id);
        return entity ?? throw new EntityNotFoundException($"Entity with id '{id}' not found.");
    }

    public Task Insert(Todo entity)
    {
        dataContext.Todos.Add(entity);
        return dataContext.SaveChangesAsync();
    }

    public Task Update(Todo entity)
    {
        dataContext.Todos.Update(entity);
        return dataContext.SaveChangesAsync();
    }

    public Task Delete(Todo entity)
    {
        dataContext.Todos.Remove(entity);
        return dataContext.SaveChangesAsync();
    }
}
