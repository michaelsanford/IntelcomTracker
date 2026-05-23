# Versioning Guide

This project uses [Semantic Versioning](https://semver.org/) adapted for a single-user desktop tool.

## MAJOR (X.0.0) — Breaking changes to user data or workflow

- Changing the `tracking.json` schema in a way that old files won't load
- Removing a key binding or fundamentally changing how the app works
- Dropping support for something (e.g., requiring a newer Windows version)

## MINOR (x.Y.0) — New features or significant enhancements

- Adding new views, columns, or key bindings
- Layout redesigns
- New functionality like export, notifications, or multi-carrier support

## PATCH (x.y.Z) — Small fixes and polish

- Colour changes, alignment tweaks, typo fixes
- Bug fixes (API parsing, crash on empty response)
- Updating the refresh interval or rate-limit timing

## Decision rule

Before tagging, ask: "If someone had the previous version, would this update surprise or break them?"

- Yes → major
- No, but it adds something → minor
- No, it just improves what's there → patch

## Example sequence

```text
v1.0.0  — initial release
v1.0.1  — fix: status colour for "Delivery failed" was wrong
v1.1.0  — feat: add nickname support
v1.2.0  — feat: detail view with event history
v2.0.0  — breaking: new tracking.json format (old files need migration)
```

## Release Codenames — Package Warning Labels

| # | Codename | Symbol |
| --- | --- | --- |
| 1 | This Side Up | arrows pointing skyward |
| 2 | Keep Dry | umbrella with raindrops |
| 3 | Handle With Care | cupped hands |
| 4 | Do Not Stack | crossed-out crate pile |
| 5 | Perishable | the half-eaten apple |
| 6 | Keep Frozen | snowflake / thermometer |
| 7 | No Hooks | crossed-out cargo hook |
| 8 | Centre of Gravity | the bullseye circle |
| 9 | Clamp Here | forklift clamp arrows |
| 10 | Do Not Drop | crossed-out falling box |
| 11 | Sling Here | chain/sling attachment point |
| 12 | Temperature Limit | thermometer with range |
| 13 | Electrostatic Sensitive | the reaching hand with arc |
| 14 | Live Animals | (self-explanatory) |
