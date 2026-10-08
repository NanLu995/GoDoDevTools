using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Platform.Storage;

namespace GoDoDevTools;

internal sealed class MainWindow : Window
{
    private readonly TextBox _projectPath = new();
    private readonly TextBox _godotPath = new();
    private readonly CheckBox _build = new() { IsChecked = true };
    private readonly CheckBox _godot = new();
    private readonly Button _run = new();
    private readonly Button _browseProject = new();
    private readonly Button _browseGodot = new();
    private readonly TextBlock _projectLabel = new();
    private readonly TextBlock _output = new() { TextWrapping = Avalonia.Media.TextWrapping.Wrap };
    private readonly ComboBox _language = new() { ItemsSource = new[] { "中文", "English" }, SelectedIndex = 0 };
    private IReadOnlyList<CheckResult>? _results;
    private string? _error;
    private bool _checking;

    private bool Chinese => _language.SelectedIndex != 1;
    private string T(string chinese, string english) => Chinese ? chinese : english;

    public MainWindow()
    {
        Width = 850;
        Height = 570;
        MinWidth = 600;
        MinHeight = 400;

        _language.SelectionChanged += (_, _) => ApplyLanguage();
        _browseProject.Click += async (_, _) => await BrowseAsync();
        _browseGodot.Click += async (_, _) => await BrowseGodotAsync();
        _run.Click += async (_, _) => await RunChecksAsync();

        var projectRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8 };
        projectRow.Children.Add(_projectPath);
        Grid.SetColumn(_browseProject, 1);
        projectRow.Children.Add(_browseProject);

        var godotRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto"), ColumnSpacing = 8 };
        godotRow.Children.Add(_godotPath);
        Grid.SetColumn(_browseGodot, 1);
        godotRow.Children.Add(_browseGodot);

        var options = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
        options.Children.Add(_build);
        options.Children.Add(_godot);

        var heading = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        heading.Children.Add(_projectLabel);
        Grid.SetColumn(_language, 1);
        heading.Children.Add(_language);

        var top = new StackPanel { Spacing = 10 };
        top.Children.Add(heading);
        top.Children.Add(projectRow);
        top.Children.Add(godotRow);
        top.Children.Add(options);
        top.Children.Add(_run);

        var root = new Grid { RowDefinitions = new RowDefinitions("Auto,*"), Margin = new Thickness(18), RowSpacing = 14 };
        root.Children.Add(top);
        var scroll = new ScrollViewer { Content = _output };
        Grid.SetRow(scroll, 1);
        root.Children.Add(scroll);
        Content = root;
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        Title = T("GoDo 开发工具", "GoDo Dev Tools");
        _projectLabel.Text = T("Godot 项目目录", "Godot project directory");
        _projectPath.PlaceholderText = T("选择项目目录", "Select project directory");
        _godotPath.PlaceholderText = T("Godot 可执行文件（可选）", "Godot executable (optional)");
        _browseProject.Content = T("浏览…", "Browse...");
        _browseGodot.Content = T("浏览…", "Browse...");
        _build.Content = T("构建解决方案", "Build solution");
        _godot.Content = T("运行 Starter UI 回归", "Run starter UI regression");
        _run.Content = T("运行检查", "Run checks");
        _output.Text = _checking ? T("检查中…", "Checking...")
            : _error ?? (_results is null ? T("请选择 Godot 项目开始。", "Select a Godot project to start.")
            : string.Join(Environment.NewLine + Environment.NewLine, _results.Select(FormatResult)));
    }

    private string FormatResult(CheckResult result)
    {
        if (!Chinese) return CheckRunner.Format(result);
        var name = result.Name switch
        {
            "Project" => "项目",
            "Localization" => "本地化",
            "Build" => "构建",
            "Godot UI regression" => "Godot UI 回归",
            _ => result.Name
        };
        return $"{(result.Passed ? "通过" : "失败")} {name}\n{string.Join("\n", result.Messages.Select(TranslateMessage))}";
    }

    private static string TranslateMessage(string message)
    {
        var summary = Regex.Match(message, @"^(\d+) catalog keys, (\d+) static references\.$");
        if (summary.Success)
            return $"目录键 {summary.Groups[1].Value} 个，静态引用 {summary.Groups[2].Value} 个。";
        return message
            .Replace("Select a directory containing project.godot.", "请选择包含 project.godot 的目录。", StringComparison.Ordinal)
            .Replace("Godot executable does not exist.", "Godot 可执行文件不存在。", StringComparison.Ordinal)
            .Replace("Timed out after three minutes.", "运行超过三分钟。", StringComparison.Ordinal)
            .Replace("Missing translation:", "缺少翻译：", StringComparison.Ordinal)
            .Replace("Duplicate key:", "重复键：", StringComparison.Ordinal)
            .Replace("Unsupported PO field:", "不支持的 PO 字段：", StringComparison.Ordinal)
            .Replace("zh_CN.po missing:", "zh_CN.po 缺少：", StringComparison.Ordinal)
            .Replace("en.po missing:", "en.po 缺少：", StringComparison.Ordinal);
    }

    private async Task BrowseAsync()
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = T("选择 Godot 项目", "Select Godot project"),
            AllowMultiple = false
        });
        if (folders.Count > 0)
            _projectPath.Text = folders[0].Path.LocalPath;
    }

    private async Task BrowseGodotAsync()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = T("选择 Godot 可执行文件", "Select Godot executable"),
            AllowMultiple = false
        });
        if (files.Count > 0)
            _godotPath.Text = files[0].Path.LocalPath;
    }

    private async Task RunChecksAsync()
    {
        _run.IsEnabled = false;
        _results = null;
        _error = null;
        _checking = true;
        ApplyLanguage();
        try
        {
            _results = await CheckRunner.RunAsync(
                _projectPath.Text ?? string.Empty,
                _build.IsChecked == true,
                _godot.IsChecked == true ? _godotPath.Text : null);
        }
        catch (Exception exception)
        {
            _error = exception.Message;
        }
        finally
        {
            _checking = false;
            _run.IsEnabled = true;
            ApplyLanguage();
        }
    }
}
