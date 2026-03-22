---
name: BlazorSocial Architecture
description: Full multi-project solution structure after Phase 0-8 + decoupling + AuthRoutes/ContentRoutes merged into Auth.Contracts/Shared. 12 projects, all 23 tests pass.
type: project
---

## Solution Projects (12 total — AuthRoutes and ContentRoutes merged away)

- **BlazorSocial.Primitives** — UniqueId<T> base class and all concrete ID types (UserId, PostId, CommentId, GroupId, ViewId). No dependencies. Target: net11.0.
- **BlazorSocial.Shared** — DTOs, IPostsApi Refit interface, ApiRoute constants, NavigationRoute, ContentRoutes (merged in). References Primitives. Target: net11.0.
  - `ContentRoutes.cs` — namespace `BlazorSocial.Shared`, class `ContentRoutes` with `Posts` and `Internal` nested classes.
- **BlazorSocial.Infrastructure** — EF Core value converters (UniqueIdConverter<T>, SmartEnumConverter<T>), InternalHeaders. References Primitives. Target: net11.0.
- **BlazorSocial.Auth.Contracts** — IAuthApi Refit interface, LoginRequestDto, TokenResponseDto, RegisterRequestDto, AuthRoutes (merged in). Refit only. Target: net11.0.
  - `AuthRoutes.cs` — namespace `BlazorSocial.Auth.Contracts`, class `AuthRoutes` with Login and Register constants.
- **BlazorSocial.Data** — EF Core entities: SocialUser, Post, Comment, Vote, View, Group, PostGroup, PostMetadata. ContentDbContext. Services: PostService, CommentService, VoteService, ViewTrackingService + cached decorators. Background jobs: MetadataUpdateWorker, ViewTrackingWorker. DataGeneratorService. AddBlazorSocialDataServices() extension. References Infrastructure. Target: net11.0.
- **BlazorSocial.Auth** — Standalone ASP.NET Core Web API. AuthDbContext (IdentityDbContext<AuthUser>). Endpoints: POST /auth/login, POST /auth/register. Issues JWT tokens only. Seeds admin@blazorsocial.com. References Auth.Contracts, Infrastructure, ServiceDefaults. Target: net11.0.
- **BlazorSocial.Api** — Standalone ASP.NET Core Web API (content). ContentDbContext. JWT bearer auth. Endpoints: /api/posts/* only. Lazy SocialUser creation middleware. References Data, Infrastructure, ServiceDefaults, Shared. Target: net11.0.
- **BlazorSocial.WebServer** — Thin BFF proxy. No DB access. JWT bearer validation. Proxies /auth/* and /api/*. Hosts Blazor SSR + WASM. References Auth.Contracts, Client, Infrastructure, ServiceDefaults. Target: net11.0.
- **BlazorSocial.Client** — Blazor WASM. Refit IPostsApi and IAuthApi. References Auth.Contracts, Shared. Target: net11.0.
- **BlazorSocial.ServiceDefaults** — Aspire shared defaults.
- **BlazorSocial.AppHost** — Aspire orchestration. References Api, Auth, WebServer. Target: net11.0.
- **BlazorSocial.Tests** — Integration tests via WebApplicationFactory<Program> targeting Api.Program. SQLite in-memory. 23 tests, all passing. References Api, Data, Shared.

## Key Architectural Decisions

- SocialUser in ContentDb is a plain entity — no ASP.NET Identity. Auth concerns fully isolated in BlazorSocial.Auth.
- JWT tokens: issuer="blazorsocial-auth", audience="blazorsocial-api", sub=UserId (string "N" format), display_name claim, email claim.
- SocialUser creation is LAZY: Auth no longer calls Api on register. Api middleware checks on every authenticated request and auto-creates the SocialUser from JWT claims if absent.
- InternalSecret header and parameter are gone. Only internal header: X-User-Id (UserId) injected by AddUserIdTransformer in WebServer.
- UniqueId types live in BlazorSocial.Primitives namespace.
- Global usings for BlazorSocial.Primitives must be explicitly added to each project csproj that needs ID types.
- Client Razor files need @using BlazorSocial.Primitives in _Imports.razor.
- WebServer Admin.razor uses literal "/Admin" route string (not NavigationRoute.Admin) since WebServer no longer references Shared.
- AuthRoutes constants now in BlazorSocial.Auth.Contracts namespace. IAuthApi alias: `using Routes = BlazorSocial.Auth.Contracts.AuthRoutes;`. AuthEndpoints alias: `using AuthR = BlazorSocial.Auth.Contracts.AuthRoutes;`
- ContentRoutes constants now in BlazorSocial.Shared namespace. IPostsApi alias: `using Content = BlazorSocial.Shared.ContentRoutes;`

## ProjectReference Table (post-merge)

| Project | References |
|---------|-----------|
| BlazorSocial.Api | Data, Infrastructure, ServiceDefaults, Shared |
| BlazorSocial.AppHost | Api, Auth, WebServer |
| BlazorSocial.Auth.Contracts | — (Refit package only) |
| BlazorSocial.Auth | Auth.Contracts, Infrastructure, ServiceDefaults |
| BlazorSocial.Client | Auth.Contracts, Shared |
| BlazorSocial.Data | Infrastructure |
| BlazorSocial.Infrastructure | Primitives |
| BlazorSocial.Primitives | — |
| BlazorSocial.ServiceDefaults | — |
| BlazorSocial.Shared | Primitives |
| BlazorSocial.Tests | Api, Data, Shared |
| BlazorSocial.WebServer | Auth.Contracts, Client, Infrastructure, ServiceDefaults |

## Build Command
`dotnet build BlazorSocial.slnx`

**Why:** Phase 0-8 + decoupling + merging tiny single-file route projects (AuthRoutes, ContentRoutes) into their natural homes (Auth.Contracts, Shared) to eliminate unnecessary project count.
**How to apply:** When adding features: put EF entities/services in Data, API endpoints in Api/Extensions, register in DependencyInjection.cs. Auth-related work goes in BlazorSocial.Auth. WebServer has no Shared reference — do not use NavigationRoute or other Shared types there. Route constants for auth live in BlazorSocial.Auth.Contracts.AuthRoutes; route constants for content live in BlazorSocial.Shared.ContentRoutes.
