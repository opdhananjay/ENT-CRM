using Serilog;
using Serilog.Exceptions;
using ENT.Midderlwares;
using ENT.Repository.Account;
using ENT.Repository.Organization;
using ENT.Services.JWT;
using ENT.Repository.RBAC;


Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-log.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 20,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
    ).CreateLogger();

    try
    {
        Log.Information("Application Started => Running");
        
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();  // Use Serilog globally for the app

        #region Adding Services
        
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<IOrganizationRepository,OrganizationRepository>();
        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddScoped<IRBACRepository, RBACRepository>();    

        #endregion Adding Service

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();

        // Added CORS Policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder => builder
                .WithOrigins("https://localhost:4200")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod()
            );
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<GlobalExceptionMiddleware>(); // Using Custom Global MiddleWare For Exceptions

        app.UseHttpsRedirection();

        app.UseSerilogRequestLogging();   // Using Serolog Requet Logging

        app.UseCors("AllowAll");   // Using CORS Policy

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

        Log.Information("Application Ended => Stopped");

    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Application Startup Failed");
    }
    finally
    {
        Log.CloseAndFlush();
    }


