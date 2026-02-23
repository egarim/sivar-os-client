# Sivar.Os Chat

A white-label Matrix chat client built with **Uno Platform** — iOS, Android, and Web from a single C# codebase.

Inspired by [FluffyChat](https://github.com/kriller-chan/fluffychat).

## Stack

- **UI:** Uno Platform (WinUI 3)
- **Protocol:** Matrix HTTP API (raw REST, no SDK dependency)
- **Targets:** iOS · Android · WebAssembly
- **Language:** C# / .NET 9

## Development Server (Local Synapse)

| Setting | Value |
|---|---|
| URL | `http://158.220.114.205:8008` |
| Admin | `@admin:158.220.114.205` |
| Test user | `@joche:158.220.114.205` |

## Getting Started

### Prerequisites

```bash
# Install .NET 9 SDK
# https://dotnet.microsoft.com/download

# Install Uno templates
dotnet new install Uno.Templates

# Install workloads
dotnet workload install uno-wasm-bootstrap android ios
```

### Run

```bash
# Web (fastest for development)
cd SivarOs
dotnet run --framework net9.0-browserwasm

# Android
dotnet run --framework net9.0-android

# iOS (Mac required)
dotnet run --framework net9.0-ios
```

## Architecture

```
SivarOs/
├── Models/
│   └── MatrixModels.cs      # Room, Message, User DTOs
├── Services/
│   └── MatrixClient.cs      # Matrix HTTP API wrapper
├── Views/
│   ├── LoginPage.xaml(.cs)  # Login screen
│   └── MainPage.xaml(.cs)   # Room list + Chat view
└── App.xaml(.cs)            # App entry point + navigation
```

## Roadmap

- [x] Login / Logout
- [x] Room list
- [x] Chat view with message bubbles
- [x] Send messages
- [ ] Real-time sync (`/sync` long-poll)
- [ ] Push notifications
- [ ] Media messages (images, files)
- [ ] Room search
- [ ] User profiles + avatars
- [ ] End-to-end encryption (Matrix E2EE)
