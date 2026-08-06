# Project Instructions

- Keep changes small and focused on the requested task.
- Do not change existing code when the requested work does not require it.
- Follow the current code style and existing project patterns.
- Do not stage changes or create commits. Leave all changes unstaged for review.
- Keep request/command behavior in the Application commands layer; web routing files should stay thin and delegate behavior instead of containing command logic.
- Prefer expression-bodied members over single-line method bodies when a method or property simply returns an expression.
- Place enums under `src/Domain/Enums`.
- Place static classes that contain logic under `src/Application/Helpers`, and give them a `Helper` postfix.
- Before reporting implementation complete, run all required verification steps: `dotnet test tests/Web.UnitTests/Web.UnitTests.csproj`, `dotnet build`, and `git diff --check`.
- If plain `dotnet build` hangs in this local environment, use `dotnet build hysite.sln --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false -nodeReuse:false` after a successful restore/test run, and report that substitution explicitly.
