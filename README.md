# Oxide.Rust (ft. uMod)

[![Build](https://github.com/Calytic/Oxide.Rust-private/actions/workflows/build.yml/badge.svg?branch=staging)](#)

## Automation

Automatically deploys Oxide.Rust bundles with patched assemblies after Rust updates the `staging` or `public` branches.
Critical updates to the uMod patcher, and using the uMod build integration, have enabled the ability to automatically generate patched assemblies without requiring manual intervention for the vast majority of patches.
The new patching method relies almost entirely on pattern matching to eliminate the need to maintain injection indexes, operands, and local variable positions.

## When does this repository generate a build?

The continuous deployment of Oxide.Rust is triggered by a simple service that detects when the `public` or `staging` branches have changed.

## What about if Rust updates at 3AM and no one is online to patch?

Yep. It will work then too.

## Staging

Staging releases are marked as pre-release and not versioned. There will only be one pre-release build and it will automatically reflect the latest version of the Rust staging branch within minutes of an update.

## What are the risks?

Certain types of refactoring will still require maintenance. The automated build system will simply skip patches that are no longer valid due to refactoring. `staging` pre-releases may occasionally have missing patches when FP pushes specific types of refactoring, such as:
* Renaming methods or classes
* Overloading methods where the patched method is no longer default overload.
* Substantially changing a method or renaming subsequent methods in the call-stack.

## What are the benefits?

Maintainers will no longer need to be furiously clicking, hopped up on energy drinks, hoping their updates don't break entirely in the days preceding and the day of a protocol update from FP. Maintainers can now periodically and casually, in the month between protocol updates, fix refactoring issues [that crop up](#what-are-the-risks). The `staging` branch of _this_ repository is automatically merged with `master` (i.e. `public`) requiring *NO* manual intervention on patch day.

This approach is cheaper, far less time-consuming, almost entirely automated, less prone to human error, more stable over-all, and faster from the moment FP updates.

## Why is it cheaper & less time consuming?

Eliminating the majority of maintenance tasks will allow maintainers time to do other things like adding more patches.

## How is it deploying so much faster?

The old build process downloaded the entire Rust server to patch a few assemblies and required manual intervention before generating a bundle. The uMod build process only downloads the files needed and eliminates the vast majority of per-patch maintenance tasks. Rust updates to `staging` or `public` are checked automatically every 2 minutes and this repository can generate a fully functional bundle in under 2 minutes.

## Where is the source code?

The output assemblies are functionally identical to [OxideMod/Oxide.Rust](https://github.com/OxideMod/Oxide.Rust). The [releases](https://github.com/Calytic/Oxide.Rust/releases) in the repository are a drop-in replacement for existing Oxide.Rust installations. The updated patcher, a custom CI script, and an updated OPJ (now UPJ) project are different but the output assemblies are equivalent. The release of those assets is TBD, but likely will be open-sourced in the future.
