namespace GraphQL.Api.Models;

public record ExternalBook(string Title, string Author, string CoverUrl);

// OpenLibrary Response Models (internal use)
public record OpenLibraryResponse(int NumFound, List<OpenLibraryDoc> Docs);
public record OpenLibraryDoc(string Title, List<string> Author_Name, int Cover_I);
