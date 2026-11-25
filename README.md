# Standard-Korean-Dictionary
<a href="https://www.microsoft.com/store/apps/9pps9l58110j?ocid=badge">Get it from Microsoft Store</a>

This repo contains a Universal Windows Platform (UWP) app that connects to the Standard Korean Language Dictionary (표준국어대사전) and searches and shows the results. The app runs on modern .NET and is compiled with Native AOT.

## Repository layout
| Folder | Description |
| --- | --- |
| `StdKoDict/` | **Current app.** UWP on .NET 10, compiled with Native AOT. |
| `표준국어대사전/` | Legacy app on .NET Native (`Microsoft.NETCore.UniversalWindowsPlatform`). **No longer used or maintained**; kept for reference only. |

## Requirements
- Windows 10 build 20348 or later (Windows 11 for desktop PCs)
- x86, x64, or ARM64

## Structure
This app uses the Open API of the Standard Korean Language Dictionary (`https://kdict.korean.go.kr/standard/api/`) to fetch and parse XML files that contain search results or word definitions, and displays them appropriately.

## Development
The `StdKoDict` project uses C# and requires:
- Visual Studio 2026 with UWP development tools for .NET
- .NET 10 SDK
- Windows SDK 10.0.26100
- **Desktop development with C++** workload for Native AOT (also install **C++ ARM64 build tools** to build ARM64 packages)

Build and package the app with Visual Studio (or Visual Studio's MSBuild). `dotnet build` does not run the UWP XAML compiler.

### Native AOT notes
- `PublishAot` is enabled, so dynamic code is disabled even in Debug builds. Debug runs behave like the published app.
- Classes used with `{Binding}` must be `partial` and marked with `[WinRT.GeneratedBindableCustomProperty]`.
- Collections bound to `ItemsSource` must be wrapped in `BindableVector` (`StdKoDict/Models/BindableVector.cs`). `ObservableCollection<T>` and `List<T>` are not exposed to XAML as bindable vectors under Native AOT.
