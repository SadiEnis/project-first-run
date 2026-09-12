# Plastic and GitHub workflow

Plastic SCM remains the primary development history. GitHub provides a cloneable source repository and a readable record of continued development for portfolio review.

Repository: https://github.com/SadiEnis/project-first-run

## Starting point

The initial source snapshot is based on Plastic `cs:138`, the chest-foundation milestone. The existing GitHub README commit is preserved. Historical Plastic changesets are not recreated as newly authored or backdated Git commits.

Include `Assets/` (with `.meta` files), `Packages/`, `ProjectSettings/`, `Docs/`, and repository configuration. Include the full required Unity project, not only `Assets/_Project/`.

Unity-generated directories, builds, local IDE state, credentials, `.plastic/`, and temporary test projects are excluded by `.gitignore`. Plastic excludes `.git` and `.codex-temp` through `ignore.conf`.

## Each subsequent development increment

Start each new development stage from the latest integrated Plastic `/main/dev`, after the preceding work has been merged there. Verify the actual base changeset; a branch name beneath `/main/dev/` alone does not establish that it was created from the current `dev` head. Use the corresponding integrated Git baseline for its Git feature branch. Preserve any branch-specific exception explicitly agreed with the project owner.

1. Finish a coherent change and run checks appropriate to that change.
2. Review the Plastic diff and explicitly check in only the intended paths.
3. Stage the same source changes in Git, including required `.meta` files and documentation. Inspect `git diff --cached` and `git diff --cached --stat` before committing.
4. Use a descriptive Git subject and record the matching changeset and verification in the body, for example:

   ```text
   feat: add chest interaction feedback

   Plastic-Changeset: cs:<actual changeset>
   Plastic-Branch: <actual source branch>
   Validation: <checks actually completed>
   ```

5. Push the Git branch after checking remote state. Never overwrite remote history to resolve a divergence.

This is an explicit two-step workflow; a Plastic check-in does not automatically create a Git commit or push it. Confirm both outcomes separately. If a push fails, retain the local commit and report that it is still unpublished.

Before a Plastic branch switch or merge, make sure Git has no uncommitted work. Plastic changes the working files without switching the Git branch: select the corresponding Git feature branch before staging changes. Integrate completed work into GitHub `main` after validation; avoid repeatedly replacing the repository with disconnected snapshots. Keep personal Git configuration local to this repository.

## Size and asset policy

At initial inspection, `Assets/`, `Packages/`, `ProjectSettings/`, and `Docs/` together occupied approximately 2.17 MiB before Git compression. No existing file required Git LFS.

- Keep source code, scenes, prefabs, and other text-serialized content in normal Git.
- Review binary assets at 10 MiB or larger before their first Git commit; decide whether to track their paths with Git LFS. This is a project review threshold, not a GitHub limit.
- GitHub warns above 50 MiB and blocks normal Git files above 100 MiB. Review file sizes before staging and pushing.
- Configure and commit LFS tracking rules before adding large assets. Changing `.gitattributes` later does not remove earlier large blobs from Git history.
- LFS has separate storage and download quotas. New versions of an LFS binary consume additional storage. Do not enable paid overages as part of routine repository setup.
- Store distributable builds outside source history, for example in GitHub Releases. Keep large source-art archives outside this repository when they are not needed to open the prototype.
- Before publishing third-party assets, check that their licenses permit source redistribution. If an asset cannot be distributed, document its setup requirement or provide a redistributable placeholder.

References: [GitHub large-file limits](https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github), [Git LFS billing](https://docs.github.com/en/billing/concepts/product-billing/git-lfs).
