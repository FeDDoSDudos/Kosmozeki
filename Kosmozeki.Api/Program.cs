using Kosmozeki.Api.Hubs;
using Kosmozeki.Api.Realtime;
using Kosmozeki.Application.Common;
using Kosmozeki.Application.DependencyInjection;
using Kosmozeki.Infrastructure.DependencyInjection;
using Kosmozeki.Infrastructure.Persistence.Postgre;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// место для наших DIев
builder.Services.AddApplication();
builder.Services.AddPostgreSQL(builder.Configuration);
builder.Services.AddCache();
builder.Services.AddInfrastructure();
builder.Services.AddScoped<IRoomEventsPublisher, SignalRRoomEventsPublisher>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<RoomHub>("/hubs/room");

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

