# Meta economy foundation

Status: Design draft, 9 November 2026. Not implemented.

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
