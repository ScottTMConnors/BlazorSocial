# BlazorSocial

Reddit-clone social media app built with Blazor WebAssembly, C# / .NET 11, and ASP.NET Core Minimal APIs.

## Rules

### Fluent UI v5 — Hard Requirement
Every UI element **must** use a Microsoft Fluent UI Blazor v5 component. Never use raw HTML when a Fluent equivalent exists.

| Don't | Do |
|-------|-----|
| `<button>` | `<FluentButton>` |
| `<input>` | `<FluentTextInput>` |
| `<select>` | `<FluentSelect>` |
| `<dialog>` | `<FluentDialog>` |
| `<hr>` | `<FluentDivider>` |
| Raw SVG / emoji icons | `<FluentIcon>` |
| Layout divs | `<FluentStack>` |
| `<label>` | `<FluentLabel>` |
| `<a>` for navigation | `<FluentLink>` |
| `<progress>` | `<LoadingSpinner>` |
| `<input type="checkbox">` | `<FluentCheckbox>` |
| `<input type="radio">` | `<FluentRadioGroup>` / `<FluentRadio>` |
| Card containers | `<FluentCard>` |

Before adding any UI, check if a Fluent component supports the use case. If it does, use it.

### Build Verification — Required
After every code change, rebuild all projects and iterate until the build is clean:

```bash
dotnet build BlazorSocial.slnx
```

Never consider a task complete with compilation errors.

### No External CSS Frameworks
Do not add Bootstrap, Tailwind, or any other CSS framework. Styling comes from Fluent UI theming and scoped CSS.

### DTOs Only Across the Wire
Never expose EF entity classes to the client. Always map to records in `BlazorSocial.Shared/Models/`.

### Strongly-Typed IDs
Use the project's `UniqueId<T>` types (`PostId`, `UserId`, etc.) — never raw `Guid`. Factory: `PostId.New()`, default: `PostId.Empty`.

### Minimal APIs Only
No MVC controllers. API endpoints go in `BlazorSocial.WebServer/Extensions/` using the existing extension method pattern. Route constants live in `ApiRoute.cs`.

### Reddit UX
New features should follow Reddit's layout patterns: compact post cards, vote arrows, metadata rows (author, time, comment count).
