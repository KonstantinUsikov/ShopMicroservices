var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddCarter(new DependencyContextAssemblyCatalogCustom());
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

var app = builder.Build();

// Configure HTTP pipeline
app.MapCarter();

app.Run();