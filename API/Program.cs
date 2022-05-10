using StackExchange.Redis;
using Core.Entities.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add Services to the Container

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<StoreContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddDbContext<AppIdentityDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

    ConfigureServices();
}
else if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<StoreContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddDbContext<AppIdentityDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

    ConfigureServices();
}

void ConfigureServices()
{
    builder.Services.AddAutoMapper(typeof(MappingProfiles));

    builder.Services.AddControllers();

    builder.Services.AddSingleton<IConnectionMultiplexer>(c => {
        var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"), true);
        return ConnectionMultiplexer.Connect(configuration);
    });

    builder.Services.AddApplicationServices();
    builder.Services.AddIdentityServices(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();

    builder.Services.AddCors(opt =>
    {
        opt.AddPolicy("CorsPolicy", policy =>
        {
            policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
        });
    });
}


// Configure the HTTP Request Pipeline

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseHttpsRedirection();

// app.UseRouting();
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerDocumentation();

app.MapControllers();


// var host = CreateHostBuilder(args).Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();

    try
    {
        var context = services.GetRequiredService<StoreContext>();
        await context.Database.MigrateAsync();  
        await StoreContextSeed.SeedAsync(context, loggerFactory);
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

        var identityContext = services.GetRequiredService<AppIdentityDbContext>();
        await identityContext.Database.MigrateAsync();
        await AppIdentityDbContextSeed.SeedUserAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occured during migration");
    }
}

await app.RunAsync();