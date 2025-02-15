using System.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

/* 
    Get all todo
*/
app.MapGet("/todoitems", async (TodoDb db) => await db.Todos.ToListAsync());

/* 
    Get all todo that are completed
 */
app.MapGet("/todositems/complete", async (TodoDb db) => await db.Todos.Where(t => t.IsComplete).ToListAsync());

/* 
    Get specific todo by id
 */
app.MapGet("/todoitems/{id}", async (TodoDb db, int id) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound()
);

/* 
    Post a new todo
 */
app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        return Results.Created($"/todoitems/{todo.Id}", todo);
    }
);

/* 
    Put a todo by id
 */
app.MapPut("/todoitems/{id}", async (Todo inputTodo, int id, TodoDb db) => 
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null) return Results.NotFound();
        todo.Name = inputTodo.Name;
        todo.IsComplete = inputTodo.IsComplete;

        await db.SaveChangesAsync();

        return Results.NoContent();
    }
);

/* 
    Delete a todo by id
 */
app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
    {
        if (await db.Todos.FindAsync(id) is Todo todo)
        {
            db.Todos.Remove(todo);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }

        return Results.NotFound();
    });

app.Run();
