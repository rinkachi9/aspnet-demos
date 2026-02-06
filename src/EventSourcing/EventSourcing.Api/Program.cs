using EventSourcing.Api.Aggregates;
using EventSourcing.Api.Events;
using EventSourcing.Api.Projections;
using Marten;
using Marten.Events.Projections; // Namespace for ProjectionLifecycle
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. Marten Configuration
// Explicitly typing the lambda parameter to avoid ambiguity with factory overload
builder.Services.AddMarten((StoreOptions opts) =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection") 
                     ?? "Host=localhost;Database=event_store;Username=postgres;Password=postgres";

    opts.Connection(connString);

    // Register Projections
    opts.Projections.Add<BankAccountDetailsProjection>(Marten.Events.Projections.ProjectionLifecycle.Inline);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Endpoints
app.MapPost("/accounts", async (IDocumentSession session, string owner, decimal initialBalance) =>
{
    var id = Guid.NewGuid();
    var opened = new BankAccountOpened(id, owner, initialBalance);
    
    // Start Stream
    session.Events.StartStream<BankAccount>(id, opened);
    await session.SaveChangesAsync();

    return Results.Created($"/accounts/{id}", new { Id = id });
});

app.MapPost("/accounts/{id}/deposit", async (IDocumentSession session, Guid id, decimal amount) =>
{
    // Append Event
    session.Events.Append(id, new MoneyDeposited(id, amount));
    await session.SaveChangesAsync();
    return Results.Ok("Deposited");
});

app.MapPost("/accounts/{id}/withdraw", async (IDocumentSession session, Guid id, decimal amount) =>
{
    // Append Event
    session.Events.Append(id, new MoneyWithdrawn(id, amount));
    await session.SaveChangesAsync();
    return Results.Ok("Withdrawn");
});

// 3. Read Side (Projection)
app.MapGet("/accounts/{id}", async (IQuerySession session, Guid id) =>
{
    // Query the Projected View
    var account = await session.LoadAsync<BankAccountDetails>(id);
    return account is not null ? Results.Ok(account) : Results.NotFound();
});

// 4. Rehydrated Aggregate
app.MapGet("/accounts/{id}/aggregate", async (IQuerySession session, Guid id) =>
{
    // Marten replays events
    var account = await session.Events.AggregateStreamAsync<BankAccount>(id);
    return account is not null ? Results.Ok(account) : Results.NotFound();
});

app.Run();
