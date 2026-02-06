using GraphQL.Api.Data;
using GraphQL.Api.DataLoaders;
using GraphQL.Api.GraphQL;
using GraphQL.Api.Services;
using GraphQL.Api.Types;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Services
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddHttpClient<IOpenLibraryClient, OpenLibraryClient>();

// 2. Configure GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<AuthorExtensions>()
    .AddDataLoader<BookBatchDataLoader>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

// 3. Map GraphQL Endpoint
app.MapGraphQL(); // Defaults to /graphql

app.Run();
