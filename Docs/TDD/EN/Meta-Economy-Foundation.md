# Meta economy foundation

Status: 9 November 2026. Initial wallet code is ready; Unity test acceptance and gameplay integration are pending.

## Purpose and prerequisite

Separate run earnings from the permanent wallet, calculate death/victory outcomes and settle each run only once. Design rules are in GDD 18.1.

Branch inspection places `feature/meta-economy` at content commit `02a1383`. Recent startup and shared flow work lives on `feature/runtime-foundation` (`979a8b9`). Integrate that foundation before implementation. These documentation changes do not perform the integration.

## Responsibilities

- Run earnings track only the current run and reset for a new run.
- Outcome calculation derives the transfer from earnings, outcome, retention and boss reward without UI or file access.
- Permanent storage keeps the wallet and settled run identity together to prevent duplicate payment.
- Flow integration receives death/finale outcomes and does not clear a run before its result is safely saved.
- Presentation displays calculated results; opening or redrawing UI never pays rewards.

These are ownership boundaries, not a mandatory class/interface per bullet. Choose concrete separations after inspecting existing code. No general service locator or broad economy framework is planned.

## Persistence and settlement contract

- Store player data in the user data location; never mutate a SO asset as a player save.
- First launch starts at zero. Save format has an explicit version.
- Corrupt/unreadable or unsupported-version saves are not silently reset and overwritten; expose an error and recovery path.
- Implement and verify temporary-file replacement and backup handling that preserves the last valid save across interrupted writes.
- Use integer resource amounts; reject negative amounts, invalid rates and arithmetic overflow. Fix rounding after design approval.
- Repeated settlement with the same run identity must not pay again. Save identity and balance in one operation.
- Failed saves must not appear successful. Retry the same result identity without creating another reward.
- Closing during settlement differs from closing an unfinished run. Resolve the unfinished-earnings policy listed in the GDD before integration.

## Implementation order and acceptance

| Work | Acceptance |
| --- | --- |
| Rules | Death/victory rules and abandon/exit/rounding decisions documented |
| Permanent wallet | First launch, reopen, corrupt saves and write failure handled |
| Run earnings | Accumulate, reset on new run and preserve prior permanent balance |
| Settlement | Death deductions, full victory transfer plus boss reward, no duplicate payment |
| Result view | Collected, lost, boss reward, transferred and total; keyboard/gamepad access |
| Gameplay sources | Data-driven amounts; duplicate enemy death does not award twice |

User gameplay acceptance and check-ins follow each meaningful slice. The user runs Unity EditMode/PlayMode suites; address specifically reported failures as needed. Write focused tests for critical calculation/storage behavior. Schedule Windows build validation separately after persistence integration.

## Research and later scope

The meta upgrade roster is not selected. Game research compares earning sources, death/victory differences, purchasing pace, level limits and interaction with run upgrades. Then complete purchase → save → application in a new run for one selected upgrade.

Outside the first scope: upgrade tree, detailed hub, difficulty/story progression, memory persistence, mid-run save/resume and cloud saves.

## First implementation slice — persistent wallet

The runtime foundation has been merged into feature/meta-economy. PersistentMetaWallet is one class under Progression. OpenDefault uses the user data directory; Open(directory) supports isolated test storage. Version 1 stores the balance. Run identity and single settlement will be added to the same record in the next slice; Credit is not a run reward API.

First launch creates a zero-balance save. Credits write and flush a temporary file, then replace the primary with a previous-version backup. Memory changes only after a successful write. Stale instances cannot overwrite newer file contents; a file lock prevents concurrent writers.

Corrupt and unknown-version saves remain untouched and report errors. A missing primary with recovery files does not initialize a new wallet. There is no automatic backup rollback; recovery UI belongs to subsequent scene integration. This slice has no scene wiring or visible gameplay change.

Focused EditMode coverage: initial/repeated opens, prior-save backup, invalid/missing/new-version records, missing primary, failed writes and retry, stale writers, invalid amounts and overflow. Unity test execution remains user-run; compilation does not imply passing tests.

## Second slice — run earnings and settlement

RunEarnings owns one run identity, collected amount and frozen result. Add accepts positive amounts; after Finish neither earnings nor the result can change. A new run uses a new instance.

RunEconomyResult calculates without file/UI dependencies. Death defaults to 40% retention rounded down and pays no boss reward. Victory transfers all earnings plus the boss reward. Retention is a parameter (0–100); Inspector configuration follows with scene integration. Invalid amounts and arithmetic overflow are rejected.

PersistentMetaWallet.Settle saves balance and run identity together. Subsequent calls for the same identity return false without payment, including zero-value results. History currently retains all settled identities; keeping only the latest would allow older results to pay again.

Save version is 2. Version 1 balance loads and migrates on the first successful mutation, preserving the previous save as a backup. Failed writes change neither balance nor in-memory history; retry with the same immutable result. Direct Credit remains a low-level wallet operation; gameplay outcomes must use Settle.

User-run coverage: RunEconomyTests and PersistentMetaWalletTests. Runtime/test compilation was checked; the agent did not run Unity suites. Scene wiring, result UI, enemy earnings and abandon/exit policy remain pending.

## Third slice — scene outcome and exit integration

Accepted temporary policy: Restart and menu Quit from an active run settle like death at 40%. Quit closes only after a successful save; Restart starts a fresh run. The demo finale counts as victory. Exit requests cannot change a previously frozen death/victory result. When run save/resume arrives, Quit will be revised to preserve the active run rather than end it. Forced exit/crash still has no unfinished-run resume save.

RunEconomyController is on Demo opening services in both demo scenes. Retention and boss reward are Inspector fields. Boss reward remains 0 pending a balance decision and can be changed for testing. Earnings sources and result summary UI are the next slice, not included here.

RunRestartController requires successful settlement before Restart/Quit. Failed saves prevent reload/exit. Death and completed finale screens report their first outcome without automatic save retries. An explicit exit request retries a failed save. Frozen outcomes disable pause Resume so failed loading cannot resume earning in the same run.

RunEconomyControllerTests and RunRestartTests cover failed saves, retry and duplicate-payment prevention. Restart tests use temporary storage. Runtime/EditMode/PlayMode assemblies compiled; Unity execution and gameplay acceptance remain user-run.
