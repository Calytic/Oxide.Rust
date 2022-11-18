# Oxide.Rust (ft. uMod)

[![Build](https://github.com/Calytic/Oxide.Rust-private/actions/workflows/build.yml/badge.svg)](https://github.com/Calytic/Oxide.Rust-private/actions/workflows/build.yml)

## Automation

This repository provides Oxide.Rust distributions *with* patched assemblies that are automatically generated.
Using updated uMod technology, [uMod LLC](https://umod.org) has added the ability to automatically generate patched Oxide.Rust assemblies without requiring manual intervention for the vast majority of patches.
The new patching method relies almost entirely on IL pattern matching to determine injection indexes and local variable positions.

## When does this repository generate a build?

The continuous deployment of Oxide.Rust is triggered by a simple service that detects when the `public` or `staging` branches have changed.

## What about if Rust updates at 3AM and no one is online to patch?

Yep. It will work then too.

## Staging

Staging releases are marked as pre-release and not versioned. There will only be one pre-release build and it will automatically reflect the latest version of the Rust staging branch within minutes of an update.

## Where is the source code?

The output assemblies are functionally identical to [OxideMod/Oxide.Rust](https://github.com/OxideMod/Oxide.Rust). The [releases](https://github.com/Calytic/Oxide.Rust/releases) in the repository are a drop-in replacement for existing Oxide.Rust installations. The updated patcher, a custom CI script, and an updated OPJ (now UPJ) project are different but the output assemblies are equivalent. The release of those assets is TBD, but likely will be open-sourced in the future.
