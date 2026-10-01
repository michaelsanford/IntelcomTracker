# IntelcomTracker

[![CI](https://github.com/michaelsanford/IntelcomTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/michaelsanford/IntelcomTracker/actions/workflows/ci.yml)
[![CodeQL](https://github.com/michaelsanford/IntelcomTracker/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/michaelsanford/IntelcomTracker/actions/workflows/github-code-scanning/codeql)
[![Release](https://github.com/michaelsanford/IntelcomTracker/actions/workflows/release.yml/badge.svg)](https://github.com/michaelsanford/IntelcomTracker/actions/workflows/release.yml)

[![GitHub Release](https://img.shields.io/github/v/release/michaelsanford/IntelcomTracker)](https://github.com/michaelsanford/IntelcomTracker/releases/latest)

[![SLSA 3](https://slsa.dev/images/gh-badge-level3.svg)](https://slsa.dev)
[![Sigstore](https://img.shields.io/badge/sigstore-signed-blueviolet?logo=sigstore)](https://www.sigstore.dev/)
[![SBOM](https://img.shields.io/badge/SBOM-CycloneDX-informational?logo=owasp)](https://cyclonedx.org/)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-0078D4)](https://github.com/michaelsanford/IntelcomTracker)
[![License](https://img.shields.io/github/license/michaelsanford/IntelcomTracker)](LICENSE)

Terminal dashboard for tracking Intelcom courier packages in real time. Polls the Intelcom tracking API, persists tracking numbers between sessions, and auto-refreshes every hour. Manual refresh is available on demand but rate-limited to once every 5 minutes.

**Runs natively on Windows, Linux, and macOS** — self-contained binaries with no .NET installation required.

## Supported Platforms

| OS | Architecture | Binary |
| --- | --- | --- |
| Windows | x64, ARM64 | `.exe` |
| Linux (glibc) | x64, ARM64 | ELF |
| Linux (musl/Alpine) | x64, ARM64 | ELF |
| macOS | x64 (Intel), ARM64 (Apple Silicon) | Mach-O |

All 8 targets are built and released automatically on every tagged version.

## Supply Chain Security

Every release artifact includes:

| Measure | What it proves |
| --- | --- |
| **GitHub Attestation** | Binary was built by this repo's CI, not tampered post-build |
| **Sigstore cosign signature** | Keyless cryptographic signature tied to the GitHub Actions OIDC identity; verifiable by anyone |
| **CycloneDX SBOM** | Machine-readable bill of materials listing all dependencies and their versions |

### Verifying a release

```bash
# GitHub attestation
gh attestation verify IntelcomTracker-v1.1.1-linux-x64 --owner michaelsanford

# Sigstore cosign
cosign verify-blob IntelcomTracker-v1.1.1-linux-x64 \
  --bundle IntelcomTracker-v1.1.1-linux-x64.bundle \
  --certificate-oidc-issuer https://token.actions.githubusercontent.com \
  --certificate-identity https://github.com/michaelsanford/IntelcomTracker/.github/workflows/release.yml@refs/tags/v1.1.1
```

See [SECURITY_SIGNING.md](SECURITY_SIGNING.md) for full details.

## Using

## Privacy

This app collects no data. All tracking numbers and cached responses are stored locally on your machine.

See [PRIVACY.md](PRIVACY.md) for full details.

## Security

To report a security vulnerability, please use [GitHub Security Advisories](https://github.com/michaelsanford/IntelcomTracker/security/advisories/new). Do not open a public issue for security reports.

See [SECURITY.md](SECURITY.md) for full details on supported versions, response timelines, and disclosure policy.

### Package list

```text
╭──────┬─────────────────────┬──────────┬──────────────────────────┬─────────────────┬─────────────────────┬─────────╮
│   #  │ Tracking ID         │ Nickname │ Status                   │ Location        │ ETA Window          │ Updated │
├──────┼─────────────────────┼──────────┼──────────────────────────┼─────────────────┼─────────────────────┼─────────┤
│   1  │ INTLCMI000000001    │ —        │ Data received            │ —               │ —                   │ 1d ago  │
│   2  │ INTLCMI000000002    │ Best Buy │ Received                 │ Candiac, QC     │ —                   │ 4h ago  │
│   3  │ INTLCMI000000003    │ IKEA     │ At station               │ Montréal, QC    │ —                   │ 3m ago  │
│ > 4  │ INTLCMI000000004    │ Amazon   │ Loaded                   │ Brossard, QC    │ 1:28 PM – 2:28 PM   │ 5s ago  │
│   5  │ INTLCMI000000005    │ Costco   │ Delivered - No Signature │ Laval, QC       │ —                   │ 2h ago  │
│   6  │ INTLCMI000000006    │ Wayfair  │ Delivery failed          │ —               │ —                   │ 2d ago  │
╰──────┴─────────────────────┴──────────┴──────────────────────────┴─────────────────┴─────────────────────┴─────────╯
[A] Add  [D] Delete  [R] Refresh  [↑↓] Navigate  [Enter] Details  [Q] Quit   auto-refresh in 58m 30s
```

### Detail view

```text
╭─ Package Details ──────────────────────────────────────────────────────────────────────────────────────────────────╮
│ Tracking ID:  INTLCMI000000004                           Nickname:  Amazon                                         │
│ Status:  Loaded                                          Driver:  Rayen                                            │
│ ETA Window:  Fri May 22, 1:28 PM – Fri May 22, 2:28 PM                                                             │
╰────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
                                                    Event History
╭──────────────────────────┬────────────────────────────────────────────────┬────────────────────────────────────────╮
│ Time                     │ Event                                          │ Location                               │
├──────────────────────────┼────────────────────────────────────────────────┼────────────────────────────────────────┤
│ Fri May 22, 12:53 AM     │ Data received                                  │ —                                      │
│ Fri May 22, 11:09 AM     │ Received                                       │ Candiac, QC                            │
│ Fri May 22, 11:09 AM     │ Delivery station inbound scan                  │ Candiac, QC                            │
│ Fri May 22, 1:27 PM      │ Loaded                                         │ Brossard, QC                           │
╰──────────────────────────┴────────────────────────────────────────────────┴────────────────────────────────────────╯
[Esc] Back to list
```

## Key Bindings

| Key        | Action                                  |
|------------|-----------------------------------------|
| `A`        | Add a tracking number                   |
| `D`        | Delete the selected tracking number     |
| `R`        | Manual refresh (once per 5 minutes)     |
| `↑` / `↓`  | Navigate between packages               |
| `Enter`    | View full event history for selection   |
| `Esc`      | Return to dashboard from detail view    |
| `Q`        | Quit                                    |

## Persistence

Tracking numbers and cached data are stored at:

| OS | Path |
| --- | --- |
| Windows | `%LOCALAPPDATA%\IntelcomTracker\tracking.json` |
| Linux | `~/.local/share/IntelcomTracker/tracking.json` |
| macOS | `~/Library/Application Support/IntelcomTracker/tracking.json` |

The file is created automatically on first use. Deleting it resets the app to a clean state.

## Development

### Requirements

- .NET 10 SDK

```powershell
winget install Microsoft.DotNet.SDK.10
```

Open a new terminal after installation so PATH is updated.

### Build & Run

```powershell
cd P:\Projects\IntelcomTracker
dotnet run
```

First run downloads NuGet packages (Spectre.Console, Microsoft.Extensions.Http) then launches the dashboard.

To build a standalone executable:

```powershell
dotnet publish -c Release -r win-x64 --self-contained -o publish\
```

Supported runtime identifiers:

| OS | RIDs |
| --- | --- |
| Windows | `win-x64`, `win-arm64` |
| Linux | `linux-x64`, `linux-arm64`, `linux-musl-x64`, `linux-musl-arm64` |
| macOS | `osx-x64`, `osx-arm64` |

### API

Polls `https://intelcom.ca/cfworker/v3/tracking/{id}/` — the same endpoint the Intelcom website uses. No authentication required; the cookie seen in browser DevTools is a consent cookie, not a session token.

### Releases

Prebuilt self-contained binaries for Windows, Linux, and macOS are attached to each [GitHub Release](../../releases) — no .NET installation needed to run them.

To publish a new release, push a version tag:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

The release workflow builds self-contained executables for all supported platforms, creates a GitHub Release named after the tag with auto-generated notes, and attaches the following assets:

| Asset | Platform |
| --- | --- |
| `IntelcomTracker-<tag>-win-x64.exe` | Windows x64 |
| `IntelcomTracker-<tag>-win-arm64.exe` | Windows ARM64 |
| `IntelcomTracker-<tag>-linux-x64` | Linux x64 |
| `IntelcomTracker-<tag>-linux-arm64` | Linux ARM64 |
| `IntelcomTracker-<tag>-linux-musl-x64` | Linux musl x64 (Alpine) |
| `IntelcomTracker-<tag>-linux-musl-arm64` | Linux musl ARM64 |
| `IntelcomTracker-<tag>-osx-x64` | macOS x64 (Intel) |
| `IntelcomTracker-<tag>-osx-arm64` | macOS ARM64 (Apple Silicon) |

### Running Tests

```powershell
dotnet test IntelcomTracker.Tests/
```

### Project Structure

```text
IntelcomTracker/
├── Models/
│   ├── ApiModels.cs          JSON deserialization (data.result wrapper)
│   └── TrackedPackage.cs     Persistence model
├── Services/
│   ├── IntelcomApiClient.cs  HTTP wrapper with Edge UA headers
│   ├── TrackingStoreService  Read/write tracking.json
│   └── RefreshService.cs     Parallel refresh of all tracked packages
├── Ui/
│   ├── DashboardView.cs      Package table with color-coded status
│   ├── DetailView.cs         Full event history for one package
│   └── StatusColors.cs       Status code → terminal color mapping
├── App.cs                    Live loop, keyboard polling, view switching
└── Program.cs                Entry point, DI wiring
```

Intelcom is a trademark of Intelcom Courrier Canada Inc. This project is not affiliated with, endorsed by, or connected to Intelcom Courrier Canada Inc.
