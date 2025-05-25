# LUNAR-Application
**LUNAR** (Location-based Unified Navigation using Augmented Reality) is a cross-platform mobile application built with **.NET MAUI** and **Unity** for guiding users through physical environments using **Augmented Reality pathfinding**.

## 🚀 Features

- Scan QR codes to determine the starting location.
- Select a destination from a dynamically populated list.
- Launch an embedded **Unity AR scene** for visual navigation.
- Uses **Dijkstra’s algorithm** to compute the shortest path.
- Displays directional path markers or moves XR Rig in editor for testing.
- Cross-platform support via .NET MAUI (Android, iOS, Windows, MacCatalyst).
- Unity AR integration targeted for Android.

---

## 🧱 Project Structure

### .NET MAUI App (UniApp)

- **QR Scanner** using ZXing.Net.MAUI
- **Destination Selector** UI in XAML
- **Unity Integration** triggered post-selection
- **Platform Services** interface (`IUnityLauncher`) used to communicate with Unity
- Platform-specific implementation in `MainActivity.cs` for Android

### Unity AR Scene

- Built with **Unity 2022.3.48f1**
- Contains:
  - Waypoints (named nodes) in the scene
  - `PathfindingManager.cs` using **Dijkstra’s algorithm**
  - AR path markers (e.g., pins, dots) instantiated along the path
  - Two operating modes:
    - **Option A**: Editor mode — XR Rig moves to `"START"` point
    - **Option B**: Device mode — AR pins appear overlaid on real-world view

---

## 🔧 Setup Instructions

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022/2025 with MAUI and Android workloads](https://learn.microsoft.com/en-us/dotnet/maui/get-started/)
- [Unity 2022.3.48f1](https://unity.com/releases/editor/qa/lts-releases)
- Android SDK and NDK for Unity build support

---

### Step 1: Unity Build (UnityLibrary)

1. Open the Unity project in **Unity 2022.3.48f1**
2. Go to `File > Build Settings`
3. Select **Android** and switch platform
4. Set build type to **Library**
5. Build the project to export the `unityLibrary` folder
6. Copy `unityLibrary` into the **UniApp.Android** project root

---

### Step 2: MAUI Android Integration

1. In `UniApp.Android.csproj`, add the following:

   ```xml
   <ProjectReference Include="unityLibrary\unityLibrary.csproj" />
