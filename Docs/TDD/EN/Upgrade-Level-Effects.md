# Upgrade level effects

## Validation — 2026-09-14

666 EditMode and 348 PlayMode tests passed in the isolated Unity 6000.3.9f1 project. Eight new PlayMode cases cover replacement, full-slot progression, maximum/duplicate behavior, snapshots, unrelated effects sharing a source ID, missing effects, identity/build mismatch, invalid future levels and throwing subscribers. All eight changed/new Unity files matched the tested copy by SHA-256. This is automated validation, not manual acceptance of future reward UI.

Continue on the item-level-foundation branch after weapon effects. Each additional level defines the complete modifier set, not an additive delta. Validate and snapshot every level before acquisition. Defaults remain one level; the development damage upgrade uses three provisional levels (+20%, +30%, +40% weapon and ability damage), not a global upgrade cap.

`PlayerUpgradeController.TryLevelUp` requires matching definition reference, build level/maximum and installed modifier instances. Missing ownership and maximum level are normal results; mismatches fail before mutation. Replace only this upgrade's exact modifier instances, preserving all unrelated modifiers, even those sharing a source ID. No extra slot or acquisition event is produced.

Commit ownership, runtime level and complete modifier set before publishing one stats change notification. A subscriber exception happens after commit and must not leave a half-applied effect. Snapshots are unaffected by later asset edits. Ability levels and chest reward/UI integration remain subsequent increments; no merge or SCM write is performed by the agent.
