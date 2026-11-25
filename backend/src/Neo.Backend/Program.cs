using Neo.Backend;
using Neo.Domain.Model;
using Neo.Domain.Serialization;
using Neo.Domain.Validation;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var store = new TopologyStore(SampleTopology.Create());

app.MapGet("/api/topology", () => JsonResponse(TopologyJson.Serialize(store.Current)));

app.MapPut("/api/topology", async (HttpRequest request) =>
{
    var result = TopologyJson.Parse(await ReadBodyAsync(request));

    if (result is Valid valid)
    {
        store.Replace(valid.Topology);
        return JsonResponse(TopologyJson.Serialize(valid.Topology));
    }

    return JsonResponse(TopologyJson.ToReportJson(result), StatusCodes.Status422UnprocessableEntity);
});

app.MapPost("/api/topology/validate", async (HttpRequest request) =>
    JsonResponse(TopologyJson.ValidateToReportJson(await ReadBodyAsync(request))));

app.Run();

static async Task<string> ReadBodyAsync(HttpRequest request)
{
    using var reader = new StreamReader(request.Body);
    return await reader.ReadToEndAsync();
}

static IResult JsonResponse(string body, int statusCode = StatusCodes.Status200OK) =>
    Results.Content(body, "application/json", statusCode: statusCode);
