using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<StudentDb>(opt => opt.UseInMemoryDatabase("StudentList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/studentitems", async (StudentDb db) =>
    await db.Students.ToListAsync());

app.MapGet("/studentitems/complete", async (StudentDb db) =>
    await db.Students.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/studentitems/{id}", async (int id, StudentDb db) =>
    await db.Students.FindAsync(id)
        is Student student
            ? Results.Ok(student)
            : Results.NotFound());

app.MapPost("/studentitems", async (Student student, StudentDb db) =>
{
    db.Students.Add(student);
    await db.SaveChangesAsync();

    return Results.Created($"/studentitems/{student.Id}", student);
});

app.MapPut("/studentitems/{id}", async (int id, Student inputStudent, StudentDb db) =>
{
    var student = await db.Students.FindAsync(id);

    if (student is null) return Results.NotFound();

    student.Name = inputStudent.Name;
    student.IsComplete = inputStudent.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/studentitems/{id}", async (int id, StudentDb db) =>
{
    if (await db.Students.FindAsync(id) is Student student)
    {
        db.Students.Remove(student);
        await db.SaveChangesAsync();
        return Results.Ok(student);
    }

    return Results.NotFound();
});

app.Run();

class Student
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class StudentDb : DbContext
{
    public StudentDb(DbContextOptions<StudentDb> options)
        : base(options) { }

    public DbSet<Student> Students => Set<Student>();
}