using Dima.Api;
using Dima.Api.Common.Api;
using Dima.Api.Endpoints;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddConfiguration();
        builder.AddSecurity();
        builder.AddDataContext();
        builder.AddCrossOrigin();
        builder.AddDocumentation();
        builder.AddServices();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
            app.ConfigureDevEnironment();

        app.UseCors(ApiConfiguration.CorsPolicyName);
        app.UseSecurity();
        app.MapEndpoints();

        app.Run();
    }
}