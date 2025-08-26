using Microsoft.EntityFrameworkCore;
using MiniLMS.Infrastructure.Data;
using MiniLMS.Core.Interfaces;
using MiniLMS.Infrastructure.Repositories;
using MiniLMS.Infrastructure.Services;
using MiniLMS.Worker;
using MassTransit;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddSerilog();

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IImportJobRepository, ImportJobRepository>();

// Register services
builder.Services.AddScoped<ICsvProcessor, CsvProcessor>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Configure MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessEnrollmentConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration.GetValue<string>("RabbitMQ:Username") ?? "admin");
            h.Password(builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? "admin");
        });

        cfg.ReceiveEndpoint("process-enrollment", e =>
        {
            e.ConfigureConsumer<ProcessEnrollmentConsumer>(context);
        });
    });
});

var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

host.Run();
