# C# Migration Status

Reference revision: `b3839f0188b272c6b4332efbc10700aefd77e9b5`

| Stage | Status | Entry Condition |
| --- | --- | --- |
| M00 | DONE | Plan approval |
| M01 | DONE | M00 DONE |
| M02 | IN_PROGRESS | M01 DONE |
| M03 | TODO | M02 DONE |
| M04 | TODO | M03 DONE |
| M05 | TODO | M04 DONE |
| M06 | TODO | M05 DONE |
| M07 | TODO | M06 DONE |
| M08 | TODO | M07 DONE |
| M09 | TODO | M08 DONE |
| M10 | TODO | M09 DONE |
| M11 | TODO | M10 DONE |
| M12 | TODO | M11 DONE |

## Current Stage

### M01 - common

- Status: DONE
- Reference Git revision: `b3839f0188b272c6b4332efbc10700aefd77e9b5`
- Java baseline: 83 tests passed, 0 failed, 0 skipped under JDK 21.0.12
- C# verification: 99 tests passed, 0 failed, 0 skipped under .NET SDK 10.0.204
- Coverage: all 10 production and 4 test Java files have C# counterparts; 2 focused C# characterization test files were added
- Build and formatting: Release build passed with no warnings; `dotnet format --verify-no-changes` passed
- Resume point: M02 entry check for graphs, including rechecking that quantity is unused and recording its temporary reference deferral

## Decision Log

| ID | Java Observation | Proposed C# Equivalent | Contract Impact | Test Evidence | Approval Status |
| --- | --- | --- | --- | --- | --- |
| D001 | `Instant` supports nanosecond precision | `DateTimeOffset` constrained to UTC and 100 ns ticks, with injected `TimeProvider` where time is obtained | Values with sub-tick precision are rejected or explicitly normalized at module boundaries | InMemoryEventsPublisherTest 4/4 passed | APPROVED |
| D002 | `BigDecimal` and JavaMoney support range and scale beyond `decimal` | Select a maintained arbitrary-precision implementation behind the existing domain API before M03 code is written | No numeric API is migrated before this decision is approved | Pending M03 numeric experiment | DEFERRED TO M03 ENTRY |
| D003 | Java generic reference types can carry `null`, including boxed numeric failures supplied to `Result.combine` | Use nullable C# failure arguments, such as `Result<int?, S>`, when absence is part of the failure-combiner contract | Preserves null rather than substituting `default(int)`; callers must express nullability in the type argument | ResultTest 57/57 passed, including mixed success/failure combinations | APPROVED |
| D004 | `graphs` declares `quantity`, but its 35 production and 15 test/helper files contain no common or quantity imports | Reference migrated common only and defer graphs -> quantity until M03 | Temporary deviation from the final dependency graph; no placeholder project | Java graphs baseline 54/54 passed; import audit found no use | APPROVED BY PLAN |

## Stage Report

### M00

Changed files: `global.json`, `archetypes.slnx`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `.gitignore`, `migration/STATUS.md`, and `migration/manifest.csv`.

Commands and results:

- `git status --short`: migration plan was the only pre-existing untracked file.
- `git rev-parse HEAD`: `b3839f0188b272c6b4332efbc10700aefd77e9b5`.
- `java -version`: OpenJDK 17.0.2.
- `.\mvnw.cmd -version`: the root wrapper failed because `JAVA_HOME` was unset.
- `dotnet --info`: .NET SDK 10.0.204 is installed; no `global.json` existed before M00.
- `dotnet restore .\archetypes.slnx`: succeeded with the expected warning that the M00 solution has no projects.
- `dotnet build .\archetypes.slnx --no-restore --configuration Release`: succeeded with the same empty-solution warning.
- `$env:JAVA_HOME='C:\Program Files\Zulu\zulu-17'; .\common\mvnw.cmd test ...`: wrapper started and compiled 10 production files, then failed with `invalid target release: 21`; zero tests executed. No system Maven installation was found.

M00 completion update: JDK 21.0.12 was located at `C:\Program Files\Java\jdk-21.0.12.1`; the common wrapper completed the Java baseline with 83 passing tests. The numeric decision remains intentionally deferred until M03 entry, before any numeric API is migrated.

### M01

Changed files: `archetypes.slnx`, `common/common.csproj`, `common/src/test/common.Tests.csproj`, 10 production C# files, 5 C# test files, `migration/STATUS.md`, and `migration/manifest.csv`.

Commands and results:

- Common Java baseline under JDK 21.0.12: 83 tests passed, 0 failed, 0 skipped.
- Focused Pair and Version tests: 20 passed, 0 failed, 0 skipped.
- Focused utility tests: 18 passed, 0 failed, 0 skipped.
- Focused Result tests: 57 passed, 0 failed, 0 skipped.
- Focused event tests: 4 passed, 0 failed, 0 skipped.
- `dotnet build .\archetypes.slnx --configuration Release`: passed with 0 warnings and 0 errors.
- `dotnet test .\archetypes.slnx --no-build --configuration Release --logger trx`: 99 passed, 0 failed, 0 skipped.
- `dotnet format .\archetypes.slnx --verify-no-changes --no-restore`: passed.

Differences and limitations: `Instant` is represented by UTC `DateTimeOffset` at 100 ns precision. Java package-private Result variant constructors are represented by `internal` constructors, and the public abstract hierarchy has a `private protected` constructor to prevent external variants. Java `Set` and `List` result views are exposed as read-only collection interfaces, while each accumulation still creates a defensive copy. M02 has not started.