# 第三方依赖记录

2026-10-08 核对 `dotnet list GoDoDevTools.csproj package --include-transitive` 与已还原 NuGet 包的 `.nuspec` 许可证元数据。

| 包 | 固定版本 | 许可证 |
|---|---|---|
| `Avalonia.Desktop`、`Avalonia.Themes.Fluent` 及同版本 Avalonia 传递包 | 12.1.3 | MIT |
| `Avalonia.BuildServices` | 11.3.2 | MIT |
| `Avalonia.Angle.Windows.Natives` | 2.1.27548.20260419 | 随包 `LICENSE`，BSD 类三条款文本 |
| `HarfBuzzSharp` 及各平台 NativeAssets | 8.3.1.3 | MIT |
| `SkiaSharp` 及各平台 NativeAssets | 3.119.4 | MIT |
| `MicroCom.Runtime` | 0.11.6 | MIT |
| `System.IO.Pipelines` | 8.0.0 | MIT |
| `Tmds.DBus.Protocol` | 0.94.1 | MIT |

来源：[Avalonia.Desktop NuGet 页面](https://www.nuget.org/packages/Avalonia.Desktop/12.1.3)、[Avalonia.Themes.Fluent NuGet 页面](https://www.nuget.org/packages/Avalonia.Themes.Fluent/12.1.3) 及 NuGet 包内元数据和许可证文件。分发二进制时须将相应版权声明、许可证和上游要求的 NOTICE 随发布物保留；本仓库目前只验证本地开发运行，尚未制作分发包。
