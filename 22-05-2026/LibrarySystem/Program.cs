using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.API;
using LibrarySystem.Interfaces;
using LibrarySystem.Services;
using LibrarySystem.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// context
builder.Services.AddDbContext<LibraryDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

//repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();


//services
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBookFilter, BookFilterService>();
builder.Services.AddScoped<IBookFilterWithPagination, BookFilterWithPaginationService>();

var app = builder.Build();

/*
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    if (!db.Books.Any())
    {
        db.Books.Add(new Book
        {
            Title = "sample book",
            Author = "Test author",
            ISBN = "1234653",
            PublicationYear = 1999,
            NoOfCopies = 5
        });
        await db.SaveChangesAsync();
    }   
}
*/

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
