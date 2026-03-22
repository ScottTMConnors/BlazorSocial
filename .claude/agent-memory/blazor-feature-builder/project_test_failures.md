---
name: Test Status
description: All 23 integration tests pass after Phase 0-8 refactoring (tests now target BlazorSocial.Api.Program, not WebServer)
type: project
---

All 23 tests pass as of the Phase 0-8 architecture refactoring. The previous 2 pre-existing failures (VoteOnPost_Unauthenticated_ReturnsUnauthorized, CommentOnPost_Unauthenticated_ReturnsUnauthorized returning BadRequest instead of Unauthorized) are now resolved — the Api project uses JWT bearer auth which correctly returns 401 for unauthenticated requests.

Tests target `BlazorSocial.Api.Program` (not WebServer). TestWebAppFactory uses `WebApplicationFactory<Program>` with `BlazorSocial.Api.dll`.

**Why:** Phase 8 moved tests from WebServer to Api — simpler and no identity dependency.
**How to apply:** Run `dotnet test BlazorSocial.slnx`. Expect 23 passing.
