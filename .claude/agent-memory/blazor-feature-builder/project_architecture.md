---
name: BlazorSocial Architecture
description: Full multi-project solution structure after Phase 0-8 + decoupling + CQRS refactor. 12 projects, clean build.
type: project
---

## Solution Projects (12 total)

- **BlazorSocial.Primitives** — UniqueId<T> base class and all concrete ID types (UserId, PostId, CommentId, GroupId, ViewId). No dependencies. Target: net11.0.
- **BlazorSocial.Shared** — DTOs, IPostsApi Refit interface, ApiRoute constants, NavigationRoute, ContentRoutes (merged in). References Primitives. Target: net11.0.
  - `ContentRoutes.cs` — namespace `BlazorSocial.Shared`, class `ContentRoutes` with `Posts` and `Internal` nested classes.
- **BlazorSocial.Infrastructure** — EF Core value converters (UniqueIdConverter<T>, SmartEnumConverter<T>), InternalHeaders. References Primitives. Target: net11.0.
- **BlazorSocial.Auth.Contracts** — IAuthApi Refit interface, LoginRequestDto, TokenResponseDto, RegisterRequestDto, AuthRoutes (merged in). Refit only. Target: net11.0.
  - `AuthRoutes.cs` — namespace `BlazorSocial.Auth.Contracts`, class `AuthRoutes` with Login and Register constants.
- **BlazorSocial.Data** — EF Core entities + CQRS services. See details below.
- **BlazorSocial.Auth** — Standalone ASP.NET Core Web API. AuthDbContext (IdentityDbContext<AuthUser>). Endpoints: POST /auth/login, POST /auth/register. Issues JWT tokens only. Seeds admin@blazorsocial.com. References Auth.Contracts, Infrastructure, ServiceDefaults. Target: net11.0.
- **BlazorSocial.Api** — Standalone ASP.NET Core Web API (content). ContentDbContext. JWT bearer auth. Endpoints: /api/posts/* only. Lazy SocialUser creation middleware. References Data, Infrastructure, ServiceDefaults, Shared. Target: net11.0.
- **BlazorSocial.WebServer** — Thin BFF proxy. No DB access. JWT bearer validation. Proxies /auth/* and /api/*. Hosts Blazor SSR + WASM. References Auth.Contracts, Client, Infrastructure, ServiceDefaults. Target: net11.0.
- **BlazorSocial.Client** — Blazor WASM. Refit IPostsApi and IAuthApi. References Auth.Contracts, Shared. Target: net11.0.
- **BlazorSocial.ServiceDefaults** — Aspire shared defaults.
- **BlazorSocial.AppHost** — Aspire orchestration. References Api, Auth, WebServer. Target: net11.0.
- **BlazorSocial.Tests** — Integration tests via WebApplicationFactory<Program> targeting Api.Program. SQLite in-memory. References Api, Data, Shared.

## BlazorSocial.Data — CQRS Architecture

### Entities
- SocialUser, Post, Comment, Vote, View, Group, PostGroup, PostMetadata (write-side)
- **PostReadModel** — denormalized read model: PostId (PK), Title, Content, PostDate, PostType (string), AuthorName (denormalized), Upvotes, Downvotes, NetVotes, ViewCount, CommentCount. No CurrentUserVote — resolved at query time.
- PostReadModels has a descending index on PostDate.

### CQRS Interfaces
- **IPostQueryService** — GetPostsAsync, GetPostByIdAsync (read path)
- **IPostCommandService** — CreatePostAsync (write path)
- IPostService is DELETED — no transitional interface

### Services
- **PostQueryService** — reads from PostReadModels (single table), overlays per-user votes via second Votes query only for authenticated users
- **PostCommandService** — inserts Post + PostReadModel in same SaveChanges call (new posts appear immediately)
- **CachedPostQueryService** (decorator for IPostQueryService) — caches base feed data (no per-user state) in Redis/memory, then overlays votes. Cache works for ALL users now.
- **ReadModelBuilder** — rebuilds entire PostReadModels table from write-side tables. Called on startup if table is empty (both dev and test paths in Api/Program.cs). Also called after DataGeneratorService bulk inserts.
- CommentService, CachedCommentService, VoteService, ViewTrackingService — unchanged

### Event Queues (fixed race condition)
- **MetadataEventQueue** — dedicated to VoteRecorded, CommentAdded, PostCreated events → consumed by MetadataUpdateWorker only
- **ViewEventQueue** — dedicated to ViewRecorded events → consumed by ViewTrackingWorker only
- Old single `PostEventQueue` (which both workers competed over) is deleted
- VoteService and CommentService inject MetadataEventQueue
- ViewTrackingService injects ViewEventQueue

### Background Workers
- **MetadataUpdateWorker** — drains MetadataEventQueue; after each vote update also ExecuteUpdateAsync on PostReadModels (Upvotes/Downvotes/NetVotes); after each comment update also updates PostReadModels.CommentCount; PostCreated is a no-op (read model row created synchronously in PostCommandService)
- **ViewTrackingWorker** — drains ViewEventQueue; after updating PostMetadata.ViewCount also ExecuteUpdateAsync on PostReadModels.ViewCount

### DI Registration (DependencyInjection.cs AddBlazorSocialDataServices)
```
Singleton: MetadataEventQueue, ViewEventQueue
HostedService: MetadataUpdateWorker, ViewTrackingWorker
Scoped: PostQueryService (concrete, for decorator injection)
Scoped: PostCommandService (concrete)
Scoped: CommentService (concrete, for decorator injection)
Scoped: IPostQueryService → CachedPostQueryService
Scoped: IPostCommandService → PostCommandService
Scoped: ICommentService → CachedCommentService
Scoped: IVoteService → VoteService
Scoped: IViewTrackingService → ViewTrackingService
Scoped: ReadModelBuilder
```

## API Endpoints (BlazorSocial.Api/Extensions/PostApiEndpoints.cs)
- GET /api/posts → injects IPostQueryService
- GET /api/posts/{id} → injects IPostQueryService
- POST /api/posts → injects IPostCommandService (requires auth)
- GET/POST /api/posts/{id}/comments → ICommentService
- POST /api/posts/{id}/vote → IVoteService
- POST /api/posts/{id}/view → IViewTrackingService

## Key Architectural Decisions

- SocialUser in ContentDb is a plain entity — no ASP.NET Identity. Auth concerns fully isolated in BlazorSocial.Auth.
- JWT tokens: issuer="blazorsocial-auth", audience="blazorsocial-api", sub=UserId (string "N" format), display_name claim, email claim.
- SocialUser creation is LAZY: Auth no longer calls Api on register. Api middleware checks on every authenticated request and auto-creates the SocialUser from JWT claims if absent.
- UniqueId types live in BlazorSocial.Primitives namespace.
- Global usings for BlazorSocial.Primitives must be explicitly added to each project csproj that needs ID types.
- Client Razor files need @using BlazorSocial.Primitives in _Imports.razor.
- WebServer Admin.razor uses literal "/Admin" route string (not NavigationRoute.Admin) since WebServer no longer references Shared.
- AuthRoutes constants now in BlazorSocial.Auth.Contracts namespace.
- ContentRoutes constants now in BlazorSocial.Shared namespace.
- PostReadModel.PostType is stored as a plain string — no SmartEnum converter on the read path.

## ProjectReference Table

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

**Why:** Phase 0-8 + decoupling + merging tiny single-file route projects + CQRS refactor for scalable read/write separation and universal caching.
**How to apply:** Reads go through IPostQueryService → CachedPostQueryService → PostQueryService (PostReadModels table). Writes go through IPostCommandService → PostCommandService (inserts Post + PostReadModel atomically). Never use IPostService — it is deleted. When adding features: put EF entities/services in Data, API endpoints in Api/Extensions, register in DependencyInjection.cs. Auth-related work goes in BlazorSocial.Auth. WebServer has no Shared reference.
