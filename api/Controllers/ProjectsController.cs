using Microsoft.AspNetCore.Mvc;
using ShahnawazProfile.Api.Services;

namespace ShahnawazProfile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IContentProvider _content;

    public ProjectsController(IContentProvider content)
    {
        _content = content;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Content(_content.GetProjectsRaw(), "application/json");
    }
}
