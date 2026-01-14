# Diagnost Client

## [🛜 Backend Source Code](https://github.com/razenxc/Diagnost.API)

## Prerequisites

- .NET 8 SDK installed.

## IMPORTANT — set backend BaseUrl

Before building or running the application, set the backend base address in `Diagnost/Misc/ApiService.cs`:

```
private const string BaseUrl = "https://your-backend.example/";
```

- Replace `https://your-backend.example/` with your backend URL.
- This file currently contains an empty string. If `BaseUrl` is not set, API calls will fail at runtime.

## Build (solution-level)

From the repository root:

```
dotnet restore
dotnet build -c Release
```

If you prefer to build a single project, provide the path to that `.csproj` instead of the solution.

## Publish — Browser (Blazor WebAssembly)

Adjust the project path to your web/Blazor project if different. Example (publish for browser WASM):

```
dotnet publish ./Diagnost/Diagnost.csproj -c Release -f net8.0 -r browser-wasm --self-contained false -o ./publish/browser
```

- Output will be in `./publish/browser` and contains static files ready for hosting.
- If your solution is ASP.NET Core hosted (server + client), publish the server project and serve the client files from its `wwwroot`.
- For advanced options (AOT, linking, service worker), consult the Blazor WebAssembly docs for `dotnet publish` flags.

## Publish — Desktop (Avalonia)

Assumes the desktop project is `Diagnost.Desktop`. Replace the project path if needed.

Framework-dependent (smaller, requires .NET on target machine):

```
dotnet publish ./Diagnost.Desktop/Diagnost.Desktop.csproj -c Release -r win-x64 -o ./publish/desktop/win-x64 --self-contained false
```

Self-contained single-file (larger, does not require .NET installed):

```
dotnet publish ./Diagnost.Desktop/Diagnost.Desktop.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -o ./publish/desktop/win-x64
```

Examples for other RIDs:

- macOS (example):
  
  ```
  dotnet publish ./Diagnost.Desktop/Diagnost.Desktop.csproj -c Release -r osx-x64 -o ./publish/desktop/osx-x64 --self-contained false
  ```

- Linux x64:
  
  ```
  dotnet publish ./Diagnost.Desktop/Diagnost.Desktop.csproj -c Release -r linux-x64 -o ./publish/desktop/linux-x64 --self-contained false
  ```

Notes:

- Use `--self-contained true` or `-p:SelfContained=true` for self-contained deployments.
- Use `-p:PublishSingleFile=true` to produce a single executable on supported RIDs.
- For installers (MSIX, dmg, deb), use platform-specific packaging tools after publish.

## Output locations

- Browser publish: `./publish/browser` (static files)
- Desktop publish: `./publish/desktop/<rid>`

## Troubleshooting

- API calls failing in Browser: open browser DevTools Network tab, confirm `BaseUrl` is correct and backend CORS allows the origin.
- Cookie/auth differences between Browser and Desktop: verify backend cookie and CORS settings; WebAssembly runs in a browser environment with different constraints.
