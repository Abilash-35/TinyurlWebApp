using Microsoft.EntityFrameworkCore;
using TinyUrl.Api.Data;
using TinyUrl.Api.Models;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "44335";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔹 CORS setup — allow both local and Render frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins(
                "http://localhost:4200",
                "https://tinyurl-frontend.onrender.com"  
            )
            .AllowAnyMethod()
            .AllowAnyHeader());
});


// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Unhandled exception: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
        throw; 
    }
});


// Create Short Url
app.MapPost("/api/tinyurl", async (HttpContext context,AppDbContext db, TinyUrlAddDto dto) =>
{
    // Generate unique short code
    var code = Guid.NewGuid().ToString("N")[..6];
    var shortUrl = $"{context.Request.Scheme}://{context.Request.Host}/{code}";

    var entity = new TinyUrls
    {
        Code = code,
        ShortURL = shortUrl,
        OriginalURL = dto.OriginalURL,
        IsPrivate = dto.IsPrivate,
        TotalClicks = 0,
        CreatedAt = DateTime.UtcNow
    };

    db.TinyUrls.Add(entity);
    await db.SaveChangesAsync();

    return Results.Ok(new { shortUrl = entity.ShortURL });

    //return Results.Ok(entity);
});


// List all public urls
app.MapGet("/api/tinyurl", async (AppDbContext db) =>
    await db.TinyUrls.Where(t => !t.IsPrivate).ToListAsync());


// search urls by term
app.MapGet("/api/tinyurl/search/{term}", async (string term, AppDbContext db) =>


    await db.TinyUrls
        .Where(t => t.OriginalURL.Contains(term) || t.ShortURL.Contains(term))
        .ToListAsync());


// delete the urls by id
app.MapDelete("/api/tinyurl/{id}", async (int id, AppDbContext db) =>
{
    var url = await db.TinyUrls.FindAsync(id);
    if (url == null) return Results.NotFound();
    db.TinyUrls.Remove(url);
    await db.SaveChangesAsync();
    return Results.Ok();
});


// redirect using code
app.MapGet("/{code}", async (string code, AppDbContext db) =>
{
    var url = await db.TinyUrls.FirstOrDefaultAsync(t => t.Code == code);
    if (url == null)
        return Results.NotFound();

    url.TotalClicks++;
    await db.SaveChangesAsync();

    return Results.Redirect(url.OriginalURL);
});

app.Run();
