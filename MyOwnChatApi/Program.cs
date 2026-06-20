using Microsoft.EntityFrameworkCore;
using MyOwnChatApi.Context;

var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<MyContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();



app.UseHttpsRedirection();


app.Run();