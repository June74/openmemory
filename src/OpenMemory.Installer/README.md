# OpenMemory installer — reserved

> **Status:** reserved directory. No project file exists here yet, and that is
> deliberate.

## Why there is no project here

A project file's `Sdk` attribute *is* the choice of build tool. A C# project
opens with `<Project Sdk="Microsoft.NET.Sdk">`; a WiX installer project is a
`.wixproj` opening with `<Project Sdk="WixToolset.Sdk">`. The file cannot be
written without first choosing the toolset.

That choice is deferred. `F-001` in the [decision register](../../docs/DECISION_REGISTER.md)
reserves "exact MSI authoring tool, bootstrapper, and upgrade implementation"
pending clean Windows install, update, and uninstall proof in Stage 1. The
register states that deferred items must not be silently chosen in unrelated
work, and scaffolding a project here would choose one.

The approved distribution format is not in question: `D-017` fixes a per-user
MSI installer. Only the authoring tool is open.

## What closes this

Stage 1's Windows packaging proof. When `F-001` is decided, the project is
created here with the chosen toolset and added to the build.
