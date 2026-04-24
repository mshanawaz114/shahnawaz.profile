using Microsoft.AspNetCore.Mvc;
using ShahnawazProfile.Api.Services;

namespace ShahnawazProfile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumeController : ControllerBase
{
    private readonly IContentProvider _content;

    public ResumeController(IContentProvider content)
    {
        _content = content;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        // Return raw JSON so the client deserializes to its own typed model.
        return Content(_content.GetResumeRaw(), "application/json");
    }
}
