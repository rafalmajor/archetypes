# C# Migration Status

Reference revision: `b3839f0188b272c6b4332efbc10700aefd77e9b5`

| Stage | Status | Entry Condition |
| --- | --- | --- |
| M00 | DONE | Plan approval |
| M01 | DONE | M00 DONE |
| M02 | DONE | M01 DONE |
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

### M02 - graphs

- Status: DONE
- Reference Git revision: `b3839f0188b272c6b4332efbc10700aefd77e9b5`
- Java baseline: 54 tests passed, 0 failed, 0 skipped under JDK 21.0.12
- C# verification: 54 graphs tests and 153 solution tests passed, with 0 failed and 0 skipped under .NET SDK 10.0.204
- Coverage: all 35 production and 15 test/helper Java files have C# counterparts
- Build and formatting: Release build passed with no warnings; `dotnet format --verify-no-changes` passed
- Dependencies: common only under D004; quantity remains intentionally deferred
- Naming refactor: D006 applied to completed common and graphs C# namespaces, matching directory segments, and methods; 99 common, 54 graphs, and 153 solution tests passed
- Resume point: M03 entry, continue the D006 C# namespace/directory/method convention, resolve D002 before numeric implementation, port quantity, restore graphs -> quantity, and rerun common, graphs, and quantity tests

## Decision Log

| ID | Java Observation | Proposed C# Equivalent | Contract Impact | Test Evidence | Approval Status |
| --- | --- | --- | --- | --- | --- |
| D001 | `Instant` supports nanosecond precision | `DateTimeOffset` constrained to UTC and 100 ns ticks, with injected `TimeProvider` where time is obtained | Values with sub-tick precision are rejected or explicitly normalized at module boundaries | InMemoryEventsPublisherTest 4/4 passed | APPROVED |
| D002 | `BigDecimal` and JavaMoney support range and scale beyond `decimal` | Select a maintained arbitrary-precision implementation behind the existing domain API before M03 code is written | No numeric API is migrated before this decision is approved | Pending M03 numeric experiment | DEFERRED TO M03 ENTRY |
| D003 | Java generic reference types can carry `null`, including boxed numeric failures supplied to `Result.combine` | Use nullable C# failure arguments, such as `Result<int?, S>`, when absence is part of the failure-combiner contract | Preserves null rather than substituting `default(int)`; callers must express nullability in the type argument | ResultTest 57/57 passed, including mixed success/failure combinations | APPROVED |
| D004 | `graphs` declares `quantity`, but its 35 production and 15 test/helper files contain no common or quantity imports | Reference migrated common only and defer graphs -> quantity until M03 | Temporary deviation from the final dependency graph; no placeholder project | Java graphs baseline 54/54 passed; import audit found no use | APPROVED BY PLAN |
| D005 | Graphs uses JGraphT directed/undirected graphs, DAG rejection, all simple directed paths, connectivity, articulation points, greedy coloring, and topological order | Use small local graph representations and algorithms tailored to each use case; keep the educational `cycles.math` graph unchanged and add no graph NuGet dependency | Preserves directed edge identity and insertion order, undirected conflict/connectivity projection, cycle rejection, exhaustive simple paths, Tarjan cutpoints, lowest-available greedy colors, and stable Kahn topological order | Focused C# tests: cycles 19/19, scheduling 5/5, concurrency 4/4, influence 13/13, userjourney 13/13 | APPROVED FOR M02 |
| D006 | Java packages, directory segments, and methods use the original `com.softwarearchetypes` and lower-camel naming | For completed M01/M02, use PascalCase C# namespaces and matching directory segments (`SoftwareArchetypes.Common`, `SoftwareArchetypes.Graphs...`) and PascalCase C# methods; leave Java references, type names, and behavior unchanged | Source-breaking naming only for C# callers; no behavioral or type-name change | Naming-refactor verification: 99 common + 54 graphs + 153 solution tests passed | APPROVED BY USER 2026-09-20 |

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
- D006 naming-refactor verification: PascalCase C# namespace, directory, and method convention applied; 99 common tests and 153 solution tests passed.

Differences and limitations: `Instant` is represented by UTC `DateTimeOffset` at 100 ns precision. Java package-private Result variant constructors are represented by `internal` constructors, and the public abstract hierarchy has a `private protected` constructor to prevent external variants. Java `Set` and `List` result views are exposed as read-only collection interfaces, while each accumulation still creates a defensive copy. Under D006, C# namespaces, matching directory segments, and methods use PascalCase while Java references, type names, and behavior remain unchanged.

### M02 - graphs

- Status: DONE
- Java baseline: 54 tests passed, 0 failed, 0 skipped under JDK 21
- Dependency decision: common only; quantity remains deferred under D004
- Graph decision: local graph representations and algorithms under D005; no graph NuGet dependency
- Focused C# verification: cycles 19/19, scheduling 5/5, concurrency 4/4, influence 13/13, userjourney 13/13
- Full C# graphs verification: 54 passed, 0 failed, 0 skipped in Release
- Full C# solution verification: 153 passed, 0 failed, 0 skipped in Release (common 99 plus graphs 54)
- Build result: Release test builds completed with 0 warnings and 0 errors
- Formatting: `dotnet format .\archetypes.slnx --verify-no-changes --no-restore` passed
- D006 naming-refactor verification: PascalCase C# namespace, directory, and method convention applied; 54 graphs tests and 153 solution tests passed
- Coverage audit: all 35 production and 15 test/helper Java files have matching C# files; all 54 annotated Java test methods have matching C# test methods
- Remaining obligation: continue the D006 C# convention in M03, restore the deferred graphs -> quantity `ProjectReference`, and rerun common, graphs, and quantity tests