/**
 * RotaLog - API Notificações
 * Notification microservice (.NET Core 6)
 * 
 * Legacy codebase with Clean Architecture abandoned in the middle
 * MediatrR with handlers gordos, credenciais em appsettings.json
 * Intentional technical debt for Alura course
 */

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: Add proper dependency injection
// TODO: Add logging configuration
// TODO: Add authentication
// TODO: Add CORS
// TODO: Add rate limiting
// TODO: Add caching

// FIXME: Clean Architecture abandoned - no proper layers
// FIXME: MediatrR not properly configured
// FIXME: No proper error handling

var app = builder.Build();

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
