using System.ComponentModel.DataAnnotations;
using System.Data;
using Aptabase.Application;
using Aptabase.Application.Authentication;
using Aptabase.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace Aptabase.Controllers;

public class Application
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string AppKey { get; set; } = "";
}

public class CreateAppRequestBody
{
    [Required]
    [StringLength(40, MinimumLength = 2)]
    public string Name { get; set; } = "";
}

[ApiController, IsAuthenticated]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AppsController : Controller
{
    private readonly IDbConnection _db;
    private readonly EnvSettings _env;

    public AppsController(IDbConnection db, EnvSettings env)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    [HttpGet("/api/_apps")]
    public async Task<IActionResult> ListApps()
    {
        var user = this.GetCurrentUser();
        var apps = await _db.QueryAsync<Application>(
            @"SELECT id, name, app_key
              FROM apps
              WHERE owner_id = @userId
              AND deleted_at IS NULL", new { userId = user.Id });
        return Ok(apps);
    }

    [HttpPost("/api/_apps")]
    public async Task<IActionResult> Create([FromBody] CreateAppRequestBody body, CancellationToken cancellationToken)
    {
        var user = this.GetCurrentUser();
        var app = new Application
        {
            Id = NanoId.New(),
            Name = body.Name,
            AppKey = $"A-{_env.Region}-{NanoId.Numbers(10)}"
        };

        await _db.ExecuteScalarAsync<string>("INSERT INTO apps (id, owner_id, name, app_key) VALUES (@appId, @ownerId, @name, @appKey)", new
        {
            appId = app.Id,
            ownerId = user.Id,
            name = app.Name,
            appKey = app.AppKey
        });

        return Ok(app);
    }

    [HttpPut("/api/_apps/{appId}")]
    public async Task<IActionResult> Update(string appId, [FromBody] CreateAppRequestBody body)
    {
        var app = await FindAppById(appId);
        if (app == null)
            return NotFound();

        app.Name = body.Name;

        await _db.ExecuteScalarAsync<string>("UPDATE apps SET name = @name WHERE id = @appId", new
        {
            appId = app.Id,
            name = app.Name,
        });

        return Ok(app);
    }

    [HttpDelete("/api/_apps/{appId}")]
    public async Task<IActionResult> Delete(string appId)
    {
        var app = await FindAppById(appId);
        if (app == null)
            return NotFound();

        await _db.ExecuteScalarAsync<string>("UPDATE apps SET deleted_at = now() WHERE id = @appId", new
        {
            appId = app.Id,
        });

        return Ok(new { });
    }

    private async Task<Application?> FindAppById(string id)
    {
        var user = this.GetCurrentUser();
        return await _db.QueryFirstOrDefaultAsync<Application>(
                @"SELECT id, name, app_key
                FROM apps
                WHERE id = @appId
                AND owner_id = @userId
                AND deleted_at IS NULL", new { appId = id, userId = user.Id });
    }
}
