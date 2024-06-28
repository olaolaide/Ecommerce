using Ecommerce.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

app.Run();