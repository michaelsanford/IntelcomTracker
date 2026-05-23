# Privacy Policy

**Effective date:** 2026-05-22

## Summary

IntelcomTracker collects no data. Everything stays on your computer.

---

## What this app does

IntelcomTracker is a local terminal application. It reads tracking numbers you enter, queries Intelcom's public tracking API on your behalf, and stores the results in a JSON file on your own machine. No data is sent to the developer or any third party controlled by the developer.

## Data stored locally

The file `%LOCALAPPDATA%\IntelcomTracker\tracking.json` contains:

- Tracking numbers you have added
- Optional nicknames you assign to them
- Cached API responses (status, ETA, event history)

This file never leaves your machine unless you copy it yourself. Deleting it removes all stored data.

## Intelcom's API

When you track a package, the app makes an HTTPS request directly to:

```
https://intelcom.ca/cfworker/v3/tracking/{your-tracking-number}/
```

This is the same request your browser makes when you visit Intelcom's tracking page. Intelcom's servers will see your tracking number and your IP address as part of normal HTTP traffic. The developer of this app has no visibility into these requests.

Refer to [Intelcom's Privacy Policy](https://intelcom.ca/en/privacy-policy/) for how Intelcom handles data on their end.

## What the developer receives

Nothing. The app has no telemetry, no analytics, no crash reporting, and no update checks. It makes no outbound connections other than the Intelcom API requests described above.

## Changes to this policy

This policy will be updated in this file if anything changes. The effective date at the top reflects the last revision.
