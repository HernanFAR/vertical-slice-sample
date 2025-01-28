# Flow of framework updates
The "master" branch will always have the last production-ready release and given the support policy, we'll mantain a "v{version}/master" with the presciding version to generate security updates and bug fixes. When we are developing a new update to a specific versión, we will use the versioning rules to the branch naming. So if:

We have v7.0.0 as the last version and v6.2.0 in support: 
- In "master", we have the release code of the v7.0.0
- In "v6/master", we have the release code of the v6.2.0

If we got a bug report in v6: 
- In "master", we have the release code of v7.0.0
- In "v6/master", we have the release code of v6.2.0
- In "v6/fix/{fix name}", we have the bug fix:
  - When production-ready:
    - Will be merged into "v6/master", publishing v6.2.1
    - If applicable, will be merged into "master", publishing v7.0.1
    - Will be deleted

If we'll add new breaking-change features:
- In "master", we have the release code of v7.0.1
- In "v6/master", we have the release code of v6.2.1
- In "v8/master", we have the pre-release code of v8.0.0
  - When production-ready:
    - From master will create "v7/master"
    - Will be merged "v8/master" into "master", publishing v8.0.0
    - Will be deleted "v6/master" 
    - Will be deleted "v8/master", because "master" have the release code
- In "v8/feat/{feature name}" we have the new feature
  - When ready:
    - Will be merged into "v8/master", publishing v8.0.0-pre.1
    - Will be deleted

If we'll add NO breaking-change features:
- In "master", we have the release code of v8.0.0
- In "v7/master", we have the last production-ready publication of v7.0.1
  - In "v8/feat/{feature name}", we have the new feature
    - When ready:
      - Will be used to publish v8.1.0-pre.1
    - When production-ready
      - Will be merged into "master", publishing v8.1.0
      - Will be deleted
     
After all of that, we will have the folling branches:
- In "master", we have the release code of v8.1.0
- In "v7/master", we have the release code of v7.0.1

# Git convertions
We use the following classification to name branches and write commits. They are based on [convencional commits v1](https://www.conventionalcommits.org/en/v1.0.0/).

## Type naming
For branch and commits types, we use the followings:
- feat: Development of new functionalities in the system. Related to MAJOR or MINOR upgrades in the semantic versioning.
- fix: Fixing of detected bugs in the system. Related to PATCH upgrades in the semantic versioning.
- dx: For commits related to development experience.
- test: For commits related to testing.
- tools: For commits related to internal functionalities.
- docs: For commits related to the framework documentation.
- refac: For commits related to refactorizations in the framework.
- devops: For commits related to dev ops CI or CD.

## Commits conventions
The templates must be like this: "[Commit type]([Related issue number]): [Commit message]". Like this:
- fix(#10): Solved SharepointAccessPolicy bug in SharepointConnectionclass
- docs(#17): Adding branch nameing and commit messages conventions in readme.md
- tools(#10): Adding MinIO support

If the commit has breaking changes, must have "Breaking changes!" at the bottom and must update the issue, if any, to have the label "breaking change"

## Branches conventions
The templates are like this: "v[Major version number]/[Branch type]/[Branch name]": Like this
- v6/tools/readme.md-update
- v5/tools/mailtrap-implementation
- v5/sharepoint-access-policy
