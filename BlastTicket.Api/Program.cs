using System.Text.Json.Serialization;
using BlastTicket.Infra.Data;
using Microsoft.AspNetCore.Mvc; // ProblemDetails用
using BlastTicket.Core.Interfaces;
using BlastTicket.Infra.Repositories;
using BlastTicket.Infra.Background;
using BlastTicket.Core.Models;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// DB設定
var connectionString = "Host=localhost;Port=5432;Database=blast_ticket;Username=admin;Password=password;Pooling=true;Maximum Pool Size=100";
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddSingleton<IOrderQueue, OrderQueue>();
builder.Services.AddHostedService<OrderProcessingWorker>();

var app = builder.Build();

app.MapGet("/health", (IDbConnectionFactory dbFactory) =>
{
    try
    {
        using var conn = dbFactory.CreateConnection();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT NOW()";
        var time = (DateTime)cmd.ExecuteScalar()!;

        // ここでHealthResponseを返す
        return Results.Ok(new HealthResponse("Healthy", time));
    }
    catch (Exception ex)
    {
        // 万が一のエラー時は文字列で返す
        return Results.Text(ex.ToString(), statusCode: 500);
    }
});

// 注文APIの実装
app.MapPost("/api/v1/orders", (OrderRequest request, IOrderQueue queue) =>
{
    // Request -> Domain Modelへの変換
    var order = new Order(
        Id: Guid.NewGuid(),
        ItemId: request.ItemId,
        UserId: request.UserId,
        Quantity: request.Quantity,
        CreatedAt: DateTime.UtcNow
        );
    　
    // キューに投げる
    if (queue.TryWrite(order))
    {
        // 成功したら202を返す
        return Results.Accepted();
    }
    else
    {
        // キューがいっぱいなら503 Service Unavailableを返す（流量制限）
        return Results.StatusCode(503);
    }
});

app.Run();

// レスポンスの型をここで定義（publicにするのが重要）
public record struct HealthResponse(string Status, DateTime DbTime);
public record struct OrderRequest(Guid ItemId, int Quantity, Guid UserId);

// 「JSONにしたい型」を全部列挙する
[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(OrderRequest))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }