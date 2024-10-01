using Dima.Api.Data;
using Dima.Api.Endpoints;
using Dima.Api.Handlers;
using Dima.Api.Models;
using Dima.Core.Handlers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(x => { x.CustomSchemaIds(n => n.FullName); });

        builder.Services
            .AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();
        builder.Services.AddAuthorization();

        var connString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

        builder.Services.AddDbContext<AppDbContext>(x =>
        {
            x.UseSqlServer(connString);
        });

        builder.Services
            .AddIdentityCore<User>()
            .AddRoles<IdentityRole<long>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints();

        builder.Services.AddTransient<ICategoryHandler, CategoryHandler>();
        builder.Services.AddTransient<ITransactionHandler, TransactionHandler>();

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapGet("/", () => new { messsage = "OK" });
        app.MapEndpoints();
        app.MapGroup("v1/identity")
            .WithTags("Identity")
            .MapIdentityApi<User>();

        app.MapGroup("v1/identity")
         .WithTags("Identity")
         .MapPost("/logout", async (
             SignInManager<User> signInManager) =>
         {
             await signInManager.SignOutAsync();
             return Results.Ok();
         })
         .RequireAuthorization();

        app.MapGroup("v1/identity")
        .WithTags("Identity")
        .MapPost("/roles", (
            ClaimsPrincipal user) =>
        {
            if (user.Identity is null || !user.Identity.IsAuthenticated)
                return Results.Unauthorized();

            var identity = (ClaimsIdentity)user.Identity;
            var roles = identity.FindAll(identity.RoleClaimType).Select(c => new
            {
                c.Issuer,
                c.OriginalIssuer,
                c.Type,
                c.Value,
                c.ValueType
            });

            return TypedResults.Json(roles);
        })
        .RequireAuthorization();

        app.Run();
    }
}

//app.MapPost("/v1/categories",
//    async (CreateCategoryRequest request,
//    ICategoryHandler handler)
//    => await handler.CreateAsync(request))
//.WithName("Categories: Create")
//.WithSummary("Cria uma categoria")
//.Produces<Response<Category>>();

//app.MapPut("/v1/categories/{id}",
//    async (long id,
//    UpdateCategoryRequest request,
//    ICategoryHandler handler)
//    =>
//    {
//        request.Id = id;
//        request.UserId = "rafael.souza";
//        return await handler.UpdateAsync(request);
//    })
//.WithName("Categories: Update")
//.WithSummary("Atualiza uma categoria")
//.Produces<Response<Category>>();

//app.MapDelete("/v1/categories/{id}",
//    async (long id,
//    ICategoryHandler handler)
//    =>
//    {
//        var request = new DeleteCategoryRequest
//        {
//            Id = id,
//            UserId = "rafael.souza"
//        };
//        return await handler.DeleteAsync(request);
//    })
//.WithName("Categories: Delete")
//.WithSummary("Exclui uma categoria")
//.Produces<Response<Category?>>();

//app.MapGet("/v1/categories",
//    async (ICategoryHandler handler)
//    =>
//    {
//        var request = new GetAllCategoriesRequest
//        {
//            UserId = "rafael.souza",
//        };
//        return await handler.GetAllAsync(request);
//    })
//.WithName("Categories: Get All")
//.WithSummary("Retorna todas as categoria de um usuário")
//.Produces<PagedResponse<List<Category>?>>();

//app.MapGet("/v1/categories/{id}",
//    async (long id,
//    ICategoryHandler handler)
//    =>
//    {
//        var request = new GetCategoryByIdRequest
//        {
//            Id = id,
//            UserId = "rafael.souza"
//        };
//        return await handler.GetByIdAsync(request);
//    })
//.WithName("Categories: Get by Id")
//.WithSummary("Retorna uma categoria")
//.Produces<Response<Category?>>();

// Request - Requisição
// GET, POST, PUT e DELETE
// Obter, Criar, Atualizar, Excluir - CRUD
// GET (NÃO TEM CORPO)
// Requisição -> Cabeçalho e Corpo
// localhost:5000/v1/categories/1
// POST, PUT, DELETE (Normalmente não tem corpo)
// JSON - JavaScript Object Notatio
// { "name": "André" }
// Binding -> Vínculo, Ligação, Elo
// Transformar o objeto JSON em um objeto C#

// Response - Resposta da Requisição
// Cabeçalho e Corpo
// Status Code - 404, 401, 403, 200, 500, 400

// Valida o request
// Verificar se a categoria já existe
// Inserir a categoria no banco
// Monta a resposta
// Retornar a resposta