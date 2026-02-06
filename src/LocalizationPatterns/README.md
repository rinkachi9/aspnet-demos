# LocalizationPatterns

Demonstrates **Internationalization (I18n)** in ASP.NET Core.

## Key Concepts
1.  **RequestLocalizationMiddleware**: Automatically detects culture from `Accept-Language` header.
2.  **IStringLocalizer<T>**: Injection service to retrieve localized strings.
3.  **Supported Cultures**: Configured for `en-US` (Default) and `pl-PL`.

## How to Test
1.  **Swagger**: `http://localhost:5010/swagger`
2.  **English (Default)**:
    - `GET /welcome`
    - Header: (None)
    - Response: "Welcome to the .NET World!"
3.  **Polish**:
    - `GET /welcome`
    - Header: `Accept-Language: pl-PL`
    - Response: "Witaj w Świecie .NET!"
