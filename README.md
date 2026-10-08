# GoDo Dev Tools

独立于游戏导出物的桌面开发工作台。第一版检查 Godot 项目的中英文翻译目录、静态翻译键，并可运行解决方案构建和 Starter UI 回归；不修改被检查项目的源文件。

## 运行

要求 .NET 8 SDK。桌面界面：

```text
dotnet run --project GoDoDevTools.csproj
```

界面默认使用中文，可在右上角切换为英文；切换时会重新显示已有检查结果。编译器和 Godot 的原始日志保留其输出语言。在界面中选择包含 `project.godot` 的项目目录。构建默认启用；运行 Godot 回归时还需填写 Godot .NET 可执行文件路径。

命令行使用同一套检查逻辑，失败返回非零状态：

```text
dotnet run --project GoDoDevTools.csproj -- check <project-directory> [--no-build] [--godot <executable>]
dotnet run --project GoDoDevTools.csproj -- self-test
```

当前构建入口为 `GoDoGame.sln`，Godot 回归入口为 `Verification/Automated/StarterUiFlowRegression.tscn`。这些是第一版针对 GoDoGame 的检查项；其他项目格式和转换器须在有真实样例后增加。

本地化检查读取 `Localization/en.po`、`Localization/zh_CN.po`，并扫描 `GameModules/`、`GameContents/`、`GameResources/`、`Shared/`、`Ui/` 中 `.cs`、`.gd`、`.tscn`、`.tres` 文件里的静态大写点分隔键。运行时拼接的键无法静态确认。暂不支持 gettext 的复数和上下文字段，遇到时检查失败。

## 依赖

桌面界面使用 Avalonia 12.1.3；许可证和传递依赖记录见 [THIRD_PARTY.md](THIRD_PARTY.md)。工具不引用游戏项目或 GoDoFramework，也不进入游戏的 Native AOT 导出闭包。
