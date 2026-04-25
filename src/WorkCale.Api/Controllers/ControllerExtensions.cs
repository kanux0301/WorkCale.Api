using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace WorkCale.Api.Controllers;

public static class ControllerExtensions
{
    public static Guid GetUserId(this ControllerBase controller) =>
        Guid.Parse(controller.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
