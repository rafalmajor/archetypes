# Java -> C# Migration Plan

Status: PLAN, code porting has not started.
Assessment date: 2026-09-20. Document language: English (ASCII).

## 1. Goal and Non-Negotiable Rules

Migrate the repository to modern C#, one module at a time, starting with
the shared components. Preserve the structure, names, relationships, and
observable code behavior. This is a language and tooling migration, not an
architectural redesign.

- Preserve all 11 module directories and their responsibility boundaries.
- Preserve the names of types, interfaces, methods, model fields, events, and
  scenarios. Do not automatically add the `I` prefix to interfaces or `Async`
  suffixes. Existing camelCase names may remain: modern C# does not require
  changing the API.
- Preserve the `com.softwarearchetypes.*` namespaces corresponding to Java
  packages and the subdirectory paths. Do not convert names to PascalCase
  without separate approval.
- Preserve inheritance, implemented contracts, composition, relationship
  direction, cardinality, optionality, entity identity, and aggregate lifecycles.
- Do not merge similar types from different modules, such as `Validity`,
  `Product`, `Instance`, identifiers, or local event contracts.
- Do not add references between modules merely because their domains are
  related. Ports such as `PricingService` or `InventoryService` do not imply
  dependencies on the `pricing` and `inventory` projects.
- Do not add a database, HTTP API, ORM, broker, CQRS, MediatR, or new
  architectural layers. Preserve in-memory implementations and synchronous behavior.
- Preserve educational examples, including `badimplementation`, and their purpose.
- Do not fix existing bugs, TODOs, or homework exercises along the way.
  Record discrepancies and separate decisions about fixes from the migration.
- Preserve `LICENSE` and author information. The port remains subject to the
  existing license terms; changing the language does not change those terms.

## 2. Facts and Directory Migration Order

Dependency sources: the root and module-level `pom.xml` files. Packages and
test examples were verified in the `src/main/java` and `src/test/java` trees.
The counts below refer to `.java` files, not types or executed tests;
the test column also includes fixtures, builders, and assertion helpers.

| Stage | Directory | Direct Internal Dependencies | Main / Test Files | Rationale |
| --- | --- | --- | --- | --- |
| M01 | common | none | 10 / 4 | Basic contracts and utilities |
| M02 | graphs | common, quantity | 35 / 15 | Process graphs second; temporarily defer the unused quantity reference as described below |
| M03 | quantity | common | 4 / 4 | Quantities, units, money, and percentages |
| M04 | party | common | 147 / 57 | Verify shared contracts against an extensive model |
| M05 | product | common, quantity | 67 / 19 | Products, features, packages, and relationships |
| M06 | pricing | quantity | 25 / 28 | Pricing after Money has stabilized |
| M07 | rules | quantity | 66 / 5 | Rules, discounts, and scoring |
| M08 | accounting | common, quantity | 50 / 17 | Postings and posting rules |
| M09 | inventory | common, quantity | 73 / 13 | Availability, locks, and reservations |
| M10 | ordering | common, quantity | 43 / 12 | Orders and local service ports |
| M11 | planvsexecution | quantity | 63 / 9 | Plans, execution, and deviation analysis |

M00 only prepares tooling in the root directory; M12 closes the migration.
After M03, the remaining modules have no declared Maven dependencies on one
another. Their order is organizational and does not imply additional
`ProjectReference` entries. `rules`, `pricing`, and `planvsexecution` use
`quantity` and, through it, indirectly use `common`. Do not treat `rules` as
an independent module.

Sequencing exception for M02: `graphs/pom.xml` declares common and quantity,
but the inspected graphs source and tests contain no references to either
module. Keep the common reference, since common is already migrated. Temporarily
defer only the C# quantity reference until M03; do not create a placeholder
project or change the Java POM. Recheck source, tests, and resources at M02 entry.
If quantity is actually required, mark M02 BLOCKED and request a sequencing
decision instead of silently porting quantity early. Record the deferral in
the decision log. In M03, restore the graphs -> quantity `ProjectReference`
and rerun graphs tests. The table describes the final dependency graph.

Working hypothesis: this order will allow each new project to build together
with its already migrated dependencies, without stubs for future projects.
Verification: compare `ProjectReference` entries with the table and build/test
after each stage. Before migrating a module, also inspect imports, including
static and test imports; any hidden dependency requires updating the plan,
not adding a temporary stub.

## 3. Target Tooling and File Layout

### 3.1. Stack

- .NET 10 LTS, `net10.0`, C# 14 with a stable SDK; no preview or `LangVersion=latest`.
- Pin a specific installed SDK 10.0.x version in `global.json`; record actual
  tool versions rather than assuming they are available on the machine.
- Use `archetypes.slnx` as a single solution. One production project and one
  test project per module. Do not create empty future projects in advance.
- In `Directory.Build.props`: shared target framework, language version,
  `Nullable=enable`, `ImplicitUsings=disable`, and deterministic builds.
  Explicit `using` directives simplify dependency audits and avoid name collisions.
- In `Directory.Packages.props`: centralized, explicit NuGet versions.
- Tests: xUnit, `Microsoft.NET.Test.Sdk`, and a compatible VSTest runner;
  pin mutually compatible stable versions. Use xUnit assertions and preserve
  the meaning of existing AssertJ helpers without requiring a new library.
- `.editorconfig`: C# formatting without forcing changes to existing domain
  names. Resolve nullable warnings instead of broadly suppressing them with
  `!` or `NoWarn`.

### 3.2. Preserving Structure During the Transition

Example for `common`; apply the same layout to each subsequent directory:

```text
archetypes.slnx
global.json
Directory.Build.props
Directory.Packages.props
common/
  pom.xml
  common.csproj
  src/
    main/
      java/com/softwarearchetypes/common/...       [Java reference]
      csharp/com/softwarearchetypes/common/...     [C# port]
    test/
      common.Tests.csproj
      java/com/softwarearchetypes/common/...       [reference tests]
      csharp/com/softwarearchetypes/common/...     [C# tests]
```

- File mapping: `<module>/src/main/java/<path>/<Type>.java` to
  `<module>/src/main/csharp/<path>/<Type>.cs`. Apply the same mapping to tests.
- `common/common.csproj`: disable `EnableDefaultCompileItems`; include only
  `src/main/csharp/**/*.cs`. This prevents compiling tests and their `obj` files.
- `common/src/test/common.Tests.csproj`: likewise, explicitly include
  `csharp/**/*.cs`; reference `../../common.csproj`.
- Keep assembly names aligned with modules: `common`, `quantity`, etc.; tests
  use `<module>.Tests`. The namespace comes from the package, not the assembly name.
- `rules` also contains `com.softwarearchetypes.scoring`: keep it in the
  `rules` project under its original path, without a new `scoring` project.
- Test utilities in `party/src/test/java/com/softwarearchetypes/common`
  remain test utilities for `party`; do not move them to the `common` library.
- Keep nested types nested. Do not artificially require one file per type
  if that would change the original organization.
- C# and Java coexist in Git. Do not bridge their runtimes through JNI/IKVM.
  C# projects reference only C# projects that have already been migrated.
- Java remains the reference source through the end of M11. Removing old code,
  POMs, and wrappers is a separate activity in M12 that requires approval.

## 4. Translation Rules for Modern C#

| Java | Migration Rule |
| --- | --- |
| `record` | `record class` or an explicitly implemented value type with the same equality and validation; use `readonly record struct` only when `default` does not bypass invariants |
| Entity / mutable aggregate | `class`; do not introduce value equality by mechanically using `record` |
| `sealed interface` and variants | Preserve the contract and variant names; restrict hierarchy extension through accessibility or enforce it with an architecture test, since C# has no identical `permits` mechanism |
| `final` | `readonly`, no setters, or `sealed`, according to its meaning; do not block existing inheritance |
| Package-private | The closest equivalent is `internal`, with its broader assembly-level access documented; use `InternalsVisibleTo` only for the module's tests |
| `Optional<T>` | Represent absence explicitly through `T?` or the `Try...` pattern if it preserves the contract; distinguish absence, `null`, and the default value |
| `UUID` | `Guid`; verify text format, sort order, and whether an empty value is allowed |
| `BigDecimal` / Moneta | Decision in M03; `decimal` is not automatically equivalent to arbitrary precision and scale |
| `Instant`, `Clock` | `DateTimeOffset` in UTC and an injected `TimeProvider`; check nanosecond/tick precision differences |
| `LocalDate`, `LocalTime` | `DateOnly`, `TimeOnly` |
| `LocalDateTime` | `DateTime` with `Kind=Unspecified`; do not assume UTC by default |
| `Duration`, `YearMonth` | `TimeSpan` with range/precision checks; for `YearMonth`, use a library type or a local type with an explicit contract, not an arbitrary day of the month |
| Stream, Optional pipelines | LINQ and pattern matching; preserve order, short-circuiting, eager/lazy evaluation, and side effects |
| `List`, `Set`, `Map` | Appropriate .NET collections with explicit equality and ordering semantics; a read-only interface does not guarantee immutability |
| `ConcurrentHashMap` | `ConcurrentDictionary`; verify atomicity of the entire operation, not just the safety of a single call |
| Domain functional interfaces | Preserve named interfaces and relationships; use `Func`/`Action` for technical callbacks, not automatically in place of every interface |
| `equals` / `hashCode` | Consistent `Equals` / `GetHashCode`; test dictionary keys, sets, numeric scale, and collections inside records |
| Java exceptions | .NET equivalents raised under the same conditions; do not turn `Result` failures into exceptions |
| Annotations / reflection | .NET attributes and reflection, preserving data names, visibility, conversions, and configuration validation |

Allowed modern features: file-scoped namespaces, pattern matching, switch
expressions, collection expressions, and primary constructors where they do
not change the contract. Do not introduce public `init`, `required`, or `with`
if they allow factories or validation to be bypassed. Treat replacing getters
with properties as an API change: preserve methods by default.

Resolve name collisions with .NET through explicit `using` aliases or qualified
names, e.g. for `Version`, `Unit`, `Process`, and `Expression`, not by changing
the domain. C# keywords can be escaped with `@`. Any unavoidable exception to
preserving names or relationships requires a recorded decision and approval.

## 5. Required Procedure for Each Module

Follow this sequence for every stage M01-M11, working in small, compilable batches.

1. Confirm that the dependencies listed in the table are DONE. Read the current
   POM, source code, tests, configuration, and resources for the module; check
   for changes since the plan's assessment date.
   The only temporary exception is the deferred graphs -> quantity reference
   in M02, governed by section 2.
2. Add all files and types to the migration manifest, including nested types,
   interfaces, enums, fixtures, and scenarios. Do not omit classes without a
   `Test` suffix.
3. Record contracts: constructors/factories, visibility, inheritance,
   relationships, invariants, errors, events, equality, mutability, and
   external dependencies.
4. Run the Java reference tests with dependencies. Record the number of cases
   actually executed, skips, and failures. An unavailable working reference
   limits verification; it does not automatically confirm compatibility.
5. Add the C# production and test projects in the agreed locations, with exactly
   the references from the table. Add both to the solution; do not generate
   code for future modules.
   In M02, apply the documented reference deferral; restore it in M03.
6. First port the required value types/contracts and their tests, then model
   behavior, repositories, facades, and configuration. Port cyclically dependent
   types within a module as the smallest compilable group.
7. Immediately after the first batch, run a focused test or build. After each
   subsequent batch, rerun tests for the changed area before moving on.
8. Port all tests and helpers. Preserve data and expectations, including
   negative cases. Add characterization tests where language mechanisms differ
   or an important contract lacks coverage.
9. For money, dates, and algorithms, compare Java/C# using shared input vectors.
   Normalize only agreed technical details, never domain results. Fix the time,
   random seed, and ordering independently of HashMap.
10. Run the entire module and all projects migrated so far. Audit the manifest
    for omitted symbols, extra references, accidentally exposed public types,
    and changed relationships.
11. Update the stage report and status. Mark DONE only after meeting the gate
    in section 8. Do not start the next module while the status is BLOCKED.

## 6. Detailed Stages

### M00. Root Directory: Preparation

1. Check `git status` and the availability of JDK 21, Maven/the wrapper, and
   .NET SDK 10. Wrappers are in the repository; also check their supporting
   files rather than assuming that the presence of `mvnw.cmd` is sufficient.
   Alternative: a local Maven installation.
2. Run the Java baseline, recording commands, versions, and Surefire reports.
   Default Surefire patterns may omit `*Scenarios` and `*Scenario`.
   Verify that all JUnit methods are actually discovered, not just that the build passes.
3. Add the .NET configuration from section 3, an empty solution, and `bin/`,
   `obj/`, and `TestResults/` entries to `.gitignore`; preserve existing Java rules.
4. Create `migration/STATUS.md` and `migration/manifest.csv` using the formats
   in section 9. Add other documents only when genuinely needed.
5. Resolve the time/numeric precision contract before migrating APIs that use
   it. Do not select a library solely because its name is similar.

Gate: baseline recorded, SDK and test runner configured, naming/path contract
agreed; the first actual compilation and test discovery check will take place in M01.

### M01. common

1. Port `Preconditions`, `StringUtils`, `Pair`, `Version`, and
   `CollectionTransformations`. Preserve validation conditions and copy semantics.
2. Port `Result<F,S>` without reversing the Failure/Success parameter order.
   Preserve variants and the contracts of `map`, `mapFailure`, `biMap`,
   `flatMap`, `fold`, `combine`, `peek`, and the list and set accumulators.
3. Verify `combine`: the failure combiner may receive `null` for a success
   branch. Reproduce this contract explicitly in C#; do not accidentally replace
   it with `default(int)`. Do not directly translate unsafe Java generic casts
   into C#.
4. Port `events`: published events, publisher, handler, and remaining
   implementations; preserve publication order and exception propagation.
5. Port `PairTest`, `PreconditionsTest`, `ResultTest`, and `VersionTest`.
   Add focused tests for events and collection transformations if not covered.

Gate: complete common API, correct behavior in all Result branches, fail-fast
accumulators, equality/hashing, and versioning; no dependencies on other modules.

### M02. graphs

Entry check: confirm that quantity is still unused, record the section 2
deferral, and reference only common until M03 restores the final dependency graph.

1. Create a matrix of JGraphT features used by the code: directed and undirected
   graphs and DAGs, all directed paths, connectivity, biconnectivity, greedy
   coloring, and topological order.
2. Run a small experiment with a .NET library, e.g. QuikGraph, checking
   maintenance, licensing, and available algorithms. It is a candidate, not
   an assumed 1:1 replacement. Any missing algorithm requires a separate
   decision and compatibility tests.
3. Port `cycles/math`, the cycles model, and reservation use cases; then
   `influence`, `scheduling` with `concurrency`, and `userjourney`.
   Preserve custom algorithms where they are part of the educational source code.
4. Reproduce the rules for edges, loops, duplicates, direction, vertex identity,
   and ordering stability. Do not merge the local Product with product.
5. Port the 15 test/helper files: cycles, intersections, bridges, influence,
   schedules, concurrency, and user journey paths.
6. Compare empty, disconnected, cyclic, and DAG graphs, plus cases with multiple
   results. Where ordering is not part of the contract, compare sets; where it
   is, reproduce tie-breaking. Preserve weighting results and cycle rejection
   behavior.

Gate: the JGraphT feature matrix is covered and scenarios match, without
silently simplifying algorithms or losing vertices/edges. The module builds
and tests without quantity; its deferred reference is recorded for M03.

### M03. quantity

1. Port `Unit`, `Quantity`, `money/Percentage`, and `money/Money` along with
   the four existing test classes. Preserve factories, units, and currency checks.
2. Before implementing arithmetic, establish the range and scale of all
   `BigDecimal`/Moneta operations. `Money.value()` rounds to 10 decimal places
   using HALF_UP, the specified division overload to 2 places, and the
   percentage multiplier calculation uses a scale of 30. Internal values and
   `value()` do not have the same contract.
3. Run a `decimal` compatibility experiment on edge cases. Accepting the limited
   range of `decimal` requires explicit approval of the contract change;
   passing existing tests alone does not prove compatibility with BigDecimal's
   arbitrary precision. If the restriction is unacceptable, select a maintained
   precision arithmetic library and test it behind the existing domain API.
4. Map HALF_UP explicitly, e.g. to `MidpointRounding.AwayFromZero` where it
   matches the specific operation. Do not rely on .NET's default ToEven behavior.
5. Check PLN/EUR/GBP/USD and other codes accepted by `of`, invalid currencies,
   cross-currency operations, division by zero, remainders, negative values,
   equality and hashing across different scales, text formatting, and
   culture-independent parsing.
6. Decide how to map the public JavaMoney `currencyUnit()` API without leaking
   Java types; record the .NET equivalent and test all call sites.
7. Record the existing issue: `Quantity.compareTo` always returns `0`.
   By default, preserve this behavior with a characterization test. A correct
   comparison implementation is a separately approved change, not a silent fix.
8. Restore the graphs -> quantity `ProjectReference` deferred in M02. Rebuild
   and rerun common, quantity, and graphs tests; close the deferral in the
   decision log before marking M03 DONE.

Gate: agreed precision contract, matching reference results, and tests for
units, percentages, and all Money factories; the final graphs dependency graph
is restored and its tests pass. Do not proceed to M04 until these gates are met.

### M04. party

1. Port identifiers, personal data, names, `Validity`, roles, capabilities,
   operating scopes, and registered identifiers with their validation policies.
2. Port the `Party`, `Person`, `Organization`, `Company`, and `OrganizationUnit`
   hierarchy, builders, and factories without changing entity identity.
3. Port addresses and their lifecycle, party relationships and their direction,
   constraints, and role requirements. Preserve the `commands` and `events` packages.
4. Port in-memory repositories, queries, facades, and `PartyConfiguration`.
   Do not unify local events with `common.events` based on their names.
5. Port all 57 test/helper files, especially registration, address, relationship,
   role, identifier, capability, and search scenarios.

Gate: matching role and relationship policies, lifecycle events, and repository
behavior; no references to quantity or future projects.

### M05. product

1. Port product identifiers and serial numbers, features, value constraints,
   metadata, validity, and applicability contexts.
2. Port products, types, instances, batches, tracking strategies, packages,
   product sets, and selection rules, preserving relationships and composition
   validation.
3. Port the catalog, product relationships, policies, repositories, commands,
   queries, views, builders, facades, and `ProductConfiguration`.
4. Map `ProductApplication`: preserve the entry point's name and runtime role.
   The POM includes Spring Web, but the starter alone does not mean controllers
   must be created. Verify runtime configuration and resources; use ASP.NET Core
   only for web behavior that actually needs to be preserved.
5. Port the 19 test files, including telecom package, logistics, relationship,
   and tracking scenarios, plus identifier and feature validation.

Gate: matching relationships, package composition, instance selection, and
catalog behavior; any host has a startup/shutdown smoke test, without new endpoints.

### M06. pricing

1. Port parameters, interpretations, adapters, ranges, step boundaries,
   validity, applicability, and the pricing context.
2. Port calculators and their types, simple/composite components, versioning,
   update strategies, and pricing result breakdowns.
3. Port `PricingFacade`, configuration, and `PricingApplication`. Replace
   Spring Boot with a minimal .NET host equivalent while preserving the entry
   point's role; do not add HTTP. Verify resources and startup configuration.
4. Port the 28 test/helper files: thresholds, ranges, time-based calculations,
   versioning, adapters, and banking, telco, and e-mobility scenarios.
5. Check all interval boundaries, the absence of a version valid on a given
   date, calculator composition, rounding, and a controlled clock.
6. Preserve the status and intent of `HomeworkTest`; do not complete the
   homework or disable it merely to obtain a passing report.

Gate: matching amounts and breakdowns, temporal rules, and versioning; quantity
is the only direct internal reference; the entry point has a smoke test.

### M07. rules

1. Port `rules/predicates`, preserving composition and short-circuiting.
2. Port `discounting/client`, `stock`, the offer model, modifiers, functors,
   guardians, visitors, and factories, preserving discount application order.
3. Port static and dynamic configuration, including `config/reflection`.
   Check attribute reading/writing, parameter names, defaults, and errors.
4. Port the entire `scoring` package: events, context, AST, visitors,
   score/fuzzy/explained algebras, and the engine. Do not replace it with an
   external rules engine.
5. Port `ChainOfferModifierTest`, `OfferItemModifierFactoryTest`,
   `ExpressionEvaluatorTest`, `ConfigImporter`, and `FakeDiscountRepository`.
   Add contract tests for reflection and scoring variants if coverage is missing.

Gate: matching discount order and calculation bases, margin protection, AST
interpretation, scoring results, and explanations; scoring stays inside rules.

### M08. accounting

1. Port identifiers, names, validity, accounts, entries, balances, transactions,
   allocations, projections, and versioning.
2. Port commands, builders, repositories, the facade, configuration, and events.
   Inject time through the agreed contract rather than retrieving it directly.
3. Port `postingrules`: context, eligibility, calculators, account lookup,
   builder, executor, event handling, and facade.
4. Port all 17 scenario/helper files; verify debit/credit, reversal, allocations,
   projections, bitemporality, and business scenarios.
5. Check failed transaction behavior, validity boundaries, totals, and event
   publication order. Do not introduce new transactional guarantees.

Gate: matching balances, projections, and transaction reversals; `*Scenarios`
tests are actually executed, not merely compiled.

### M09. inventory

1. Port identifiers, local product/instance definitions, resource specifications,
   and `availability/TimeSlot` with its interval boundary contract.
2. Port `availability`: individual, pool, temporal, grouped, and composite
   availability, locks, owners, expiration times, repositories, and the facade.
3. Port the inventory core, builders, validators, and configuration; then
   `reservation` and `waitlist` with selection policies and state transitions.
4. Port the 13 test files: facades, availability, reservations, and waitlists,
   plus fuel, medical, and hotel scenarios.
5. Check overlapping slots, touching boundaries, expiration, partial locks,
   pooled quantities, cancellation, retries, and concurrent access behavior
   where the relevant contract supports such access.

Gate: unchanged availability, allocation, and waiting-order decisions;
no dependencies added on product or party.

### M10. ordering

1. Port identifiers, statuses, order parties, roles, line specifications,
   quantity, line pricing, and price breakdowns.
2. Port `Order`, `OrderLine`, commands, the facade, repositories, and events.
3. Preserve local billing, payment, pricing, and inventory ports and their
   models. Do not replace them with direct calls to other projects.
4. Port the 12 test files, including corporate, e-commerce, pricing,
   line-level party scenarios, and failure cases.
5. Check allowed status transitions, quantity and line edits, arbitrary prices,
   confirmation/cancellation, and the effects of an external port failure.

Gate: matching state transitions and side effects; common and quantity are
the only references, and port test doubles preserve the reference behavior.

### M11. planvsexecution

1. Port `repaymentanalysis`: payment, schedule, delta, tolerance, modification,
   facade, and modification orchestration.
2. Port `productionanalysis` with its separate delta, tolerance, modification,
   plans, and orchestration; do not merge these similar hierarchies.
3. Port `resolutionmismatch`, preserving daily/monthly resolution and explicitly
   mapping YearMonth.
4. Port `badimplementation/deliveryscheduling` without changing its role as an
   example of a problematic implementation. Do not refactor it into a good model.
5. Port the 9 test/helper files, including production and repayment analyses
   and simulations. Check partial payments, tolerances, unmatched cases,
   schedule changes, buffers, and distribution of the remaining amount.

Gate: matching delta and simulation results, unchanged strategies, and two
separate analysis models; the intent of `ProblematicDeliverySchedulingTest`
is preserved.

### M12. Migration Closure

1. Verify the manifest for all modules and reconcile tests and scenarios.
2. Run restore/build/test in a clean checkout without local artifacts.
   Test Windows and Linux, especially paths, culture, time zones, and reflection.
3. Add/update .NET CI with exactly the same gates. The Java baseline remains
   runnable until compatibility of the final module has been approved.
4. Update the README: required SDK, building, testing, running existing entry
   points, and the module map. Preserve author and license information.
5. Present the user with a report of differences and limitations. Only after
   separate approval, remove the reference Java code, Maven, and unnecessary
   artifacts; preserve history and identify the reference revision. Do not
   delete unidentified files.

## 7. Commands and Test Reliability

The commands below are templates to run during the migration, not a report of
tests executed while writing this plan. Run them from the repository root.

Java reference for a selected module and its dependencies (PowerShell):

```powershell
$module = 'common'
.\mvnw.cmd -pl $module -am test '-Dtest=*Test,*Tests,*TestCase,*Scenarios,*Scenario' '-Dsurefire.failIfNoSpecifiedTests=false'
```

If the wrapper does not work, use `mvn` after checking JDK/Maven versions.
Do not treat name patterns as proof of complete coverage: compare the Surefire
report with the inventory of JUnit methods, parameterized tests, and nested
tests. `failIfNoSpecifiedTests=false` accommodates dependencies without matching
tests; zero executed tests in the module being migrated still blocks the gate.
Document existing build, dependency, and compiler issues as baseline blockers;
do not change the Java version or domain code while preparing the plan.

C# verification after adding the project and its tests:

```powershell
$module = 'common'
dotnet restore archetypes.slnx
dotnet build archetypes.slnx --no-restore --configuration Release
dotnet test "$module/src/test/$module.Tests.csproj" --no-build --configuration Release --logger trx
dotnet test archetypes.slnx --no-build --configuration Release --logger trx
dotnet format archetypes.slnx --verify-no-changes --no-restore
```

The solution contains only modules migrated so far and their tests. When
working on a single batch, first run `dotnet test ... --filter ...` without
`--no-build` if appropriate. Do not use stale binaries to verify a change.

Map JUnit tests to `[Fact]`/`[Theory]` and appropriate xUnit data sources.
Per-test lifecycles, nested tests, parameterization, and shared fixtures require
explicit mapping. Do not assume test execution order; isolate clocks, static
data, and repositories before enabling parallelism. Executed case counts may
differ between runners; the manifest must show where each scenario and
parameter set has been preserved.

## 8. Definition of Done and Blocking Rules

A stage is marked DONE only when every item has been verified:

- [ ] Every manifest file and symbol has an equivalent or an approved explanation.
- [ ] Names, package structure, visibility, and domain relationships match.
- [ ] `ProjectReference` entries match the table, except for the documented M02 deferral that must be closed in M03; no circular dependencies were added.
- [ ] Production code contains no placeholders, new TODOs, or `NotImplementedException` in place of a port.
- [ ] All scenarios, test data, and meaningful assertions have been preserved.
- [ ] The Release build passes; no compiler/nullable warnings were introduced.
- [ ] Tests for the module and all previously migrated modules pass and actually execute.
- [ ] Numeric, temporal, equality, and collection edge cases have evidence of compatibility.
- [ ] Every difference from Java has an explicit decision; no silent fixes or skipped tests.
- [ ] The report includes commands, results, executed/skipped test counts, and the next step.

Use BLOCKED for an unapproved contract change, unavailable tool, unresolved
precision, missing algorithm, or reference behavior that cannot be reproduced.
Report the specific issue and the smallest required decision. Do not weaken
assertions or mark DONE when tests have not been run.

## 9. Progress Tracking and LLM Contract

In M00, create `migration/STATUS.md` with the table below and update it after
each completed batch, not only after finishing the entire repository.

| Stage | Initial Status | Entry Condition |
| --- | --- | --- |
| M00 | TODO | Plan approval |
| M01 | TODO | M00 DONE |
| M02 | TODO | M01 DONE |
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

Allowed statuses: TODO, IN_PROGRESS, BLOCKED, DONE. Entry order is deliberately
sequential, even when modules are technically independent.

`migration/manifest.csv` should contain these columns:

```text
module,java_path,java_symbol,csharp_path,csharp_symbol,kind,status,test_evidence,decision_id
```

Paths are relative to the repository. Use standard CSV quoting for commas in
generic symbols. Also record nested symbols, configuration resources, and any
exceptions to 1:1 file mapping.

In `migration/STATUS.md`, also maintain a brief decision log: identifier,
Java observation, proposed C# equivalent, contract impact, test evidence, and
approval status. The stage report must identify the reference Git revision,
changed files, executed commands, results, remaining gaps, and the exact point
from which to resume.

### Prompt for Executing One Stage

```text
Execute only stage <Mxx> of MIGRATION_TO_CSHARP.md for module <module>.

Read the plan, migration/STATUS.md, the relevant manifest rows, and the module's
current Java source/tests. Confirm entry conditions and Git status.
If the tracking files do not exist yet, execute M00 rather than assuming their contents.

Preserve names, packages, relationships, and behavior. Use .NET 10 / C# 14 as
specified in the plan. Do not refactor the domain, fix existing bugs, remove
Java, add dependencies on modules not yet migrated, or change tests merely
to match the new implementation.

First select the smallest set of related types and tests that can compile.
Immediately after the first change, run a focused test or build.
Continue in small batches within the same module. Changing a shared API
requires an impact description and retesting all already migrated consumers.

Finally, run all stage gates. Update the manifest and STATUS.md.
Report executed tests and their results, discrepancies, and the next step.
Do not claim to have executed commands that were not run.
Do not automatically proceed to the next module. If blocked, record BLOCKED,
the evidence, and the specific decision required from the user.
```

First implementation task: M00, followed by M01/common. This document alone
does not authorize starting the migration or removing Java code.