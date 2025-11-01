using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Users;

namespace Api;

public class SuperHeroesFunctions
{

    [Function(nameof(SuperHeroesFunctions))]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        return new OkObjectResult(SuperHeros.SuperHeroes);
    }
}