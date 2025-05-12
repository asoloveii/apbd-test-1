using apbd_test1.Services;

var builder = WebApplication.CreateBuilder(args);

// get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("Default");

// add services to the container
builder.Services.AddControllers();

// register DbService with DI and pass the connection string
builder.Services.AddScoped<IDbService>(provider => new DbService(connectionString));

var app = builder.Build();


// Configure the HTTP request pipeline
app.UseAuthorization();

app.MapControllers();

app.Run();