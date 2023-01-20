using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Themes.Fluent;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media;
using GithubLib;
using Natsurainko.Toolkits.Network.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WonderLab.Modules.Base;
using WonderLab.Modules.Media;
using WonderLab.Modules.Models;
using WonderLab.Modules.Toolkits;
using WonderLab.ViewModels;
using WonderLab.Views;
using Brushes = Avalonia.Media.Brushes;
using Color = Avalonia.Media.Color;
using FluentAvalonia.UI.Media.Animation;
using WonderLab.Modules.Controls;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
namespace WonderLab
{
    public partial class MainWindow : Window
    {
        public bool IsWindows11 { get => (Environment.OSVersion.Version.Build >= 22000); }

        public void AcrylicColorChange()
        {
            if (!IsWindows11)
            {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                if (AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme is "Dark")
                {
                    AcrylicBorder.Material = new ExperimentalAcrylicMaterial()
                    {
                        BackgroundSource = AcrylicBackgroundSource.Digger,
                        TintColor = Colors.Black,
                        TintOpacity = 0.97,
                        MaterialOpacity = 0.65,
                    };
                }
                else
                {
                    AcrylicBorder.Material = new ExperimentalAcrylicMaterial()
                    {
                        BackgroundSource = AcrylicBackgroundSource.Digger,
                        TintColor = Colors.White,
                        TintOpacity = 0.97,
                        MaterialOpacity = 0.65,
                    };
                }
#pragma warning restore CS8602 // 解引用可能出现空引用。
            }
        }

        public void EnableMica()
        {
            if (IsWindows11)
                TransparencyLevelHint = WindowTransparencyLevel.Mica;
        }

        public static void ShowInfoBarAsync(string title, string message = "", InfoBarSeverity severity = InfoBarSeverity.Informational, int delay = 5000, IControl? button = null) =>
        TaskBase.InvokeAsync(async () =>
        {
            try
            {
                var viewData = new InfoBarModel()
                {
                    Button = button,
                    Delay = delay,
                    Description = message,
                    Title = title,
                    Severity = severity
                };
                InformationListBox.Items = null;
                InfoBarItems.Add(viewData);
                InformationListBox.Items = InfoBarItems;
                await Task.Delay(delay);

                if (!viewData.Removed)
                {
                    InfoBarItems.Remove(viewData);
                    InformationListBox.Items = null;
                    InformationListBox.Items = InfoBarItems;
                }
            }
            catch (Exception)
            {

            }
        });

        public async void ShowDialog(string title, string messages)
        {
            dialog.DataContext = new
            {
                Title = title,
            };
            message.DataContext = new { Message = messages };
            await dialog.ShowAsync();
        }

        private void TryEnableMicaEffect(FluentAvaloniaTheme thm)
        {
            if (thm.RequestedTheme == FluentAvaloniaTheme.DarkModeString)
            {
                var color = this.TryFindResource("SolidBackgroundFillColorBase", out var value) ? (Color2)(Color)value : new Color2(32, 32, 32);

                color = color.LightenPercent(-0.8f);

                Background = new ImmutableSolidColorBrush(color, 0.78);
            }
            else if (thm.RequestedTheme == FluentAvaloniaTheme.LightModeString)
            {
                // Similar effect here
                var color = this.TryFindResource("SolidBackgroundFillColorBase", out var value) ? (Color2)(Color)value : new Color2(243, 243, 243);

                color = color.LightenPercent(0.5f);

                Background = new ImmutableSolidColorBrush(color, 0.9);
            }
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e) => JsonToolkit.JsonWrite();
        public static string GetVersion()
        {
            return "1.0.1.2";
        }
        public static void AutoUpdata()
        {
            try
            {
                string releaseUrl = GithubLib.GithubLib.GetRepoLatestReleaseUrl("Blessing-Studio", "WonderLab");
                Release? release = GithubLib.GithubLib.GetRepoLatestRelease(releaseUrl);
                if (release != null)
                {
                    if (release.name != GetVersion())
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            var button = new HyperlinkButton()
                            {
                                Content = "点击下载",
                            };
                            button.Click += Button_Click;
                            ShowInfoBarAsync("自动更新", "发现新版本" + release.name + "  当前版本" + GetVersion() + "  ", InfoBarSeverity.Informational, 7000, button);
                        }
                        else
                        {
                            ShowInfoBarAsync("自动更新", "发现新版本" + release.name + "  当前版本" + GetVersion() + "  ", InfoBarSeverity.Informational, 7000);
                        }
                    }

                }
            }
            finally
            {

            };
        }

        public static void Button_Click(object? sender, RoutedEventArgs e)
        {
            var res = Updata();
            if (res != null)
            {
                DownloadUpdata(res);
            }
        }

        public static void DownloadUpdata(Release res)
        {
            var button = new HyperlinkButton()
            {
                Content = "转至 祝福终端>任务中心",
            };
            button.Click += (object? sender, RoutedEventArgs e) =>{ Page.NavigatedToTaskView(); };
            MainWindow.ShowInfoBarAsync("提示：", $"开始下载更新  更新内容:\n {res.body} \n\n推送者{res.author.login} \n 可前往任务中心查看进度", InfoBarSeverity.Informational, 8000, button);
            string save = @"updata.zip";
            if(Directory.Exists("updata-cache"))
            File.Delete(Path.Combine("updata-cache", save));
            string url = "";
            foreach (var asset in res.assets)
            {
                if (asset.name == "Results.zip")
                {
                    url = asset.browser_download_url;
                }
            }
            HttpDownloadRequest httpDownload = new HttpDownloadRequest();
            httpDownload.Url = url;
            httpDownload.FileName = save;
            httpDownload.Directory = new DirectoryInfo("updata-cache");
            DownItemView downItemView = new DownItemView(httpDownload, $"更新  {res.name} 下载", new AfterDo(After_Do));
            TaskView.Add(downItemView);
        }
        public static Release? Updata()
        {
            string releaseUrl = GithubLib.GithubLib.GetRepoLatestReleaseUrl("Blessing-Studio", "WonderLab");
            Release? release = GithubLib.GithubLib.GetRepoLatestRelease(releaseUrl);
            return release;
        }
        private static void After_Do()
        {
            File.Create(Path.Combine("updata-cache", "UpdataNextTime")).Close();
            MainWindow.ShowInfoBarAsync("提示：", $"更新下载完成  重启启动器以应用更新", InfoBarSeverity.Success, 20000);
        }

        public static void Button_Click1(object? sender, RoutedEventArgs e)
        {
            BlessingView.IsTask = true;
            MainView.mv.FrameView.Navigate(typeof(BlessingView));
            MainView.mv.main.IsSelected = true;
            BlessingView.view.FrameView.Navigate(typeof(TaskView), null, new SlideNavigationTransitionInfo());
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            var thm = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();
#pragma warning disable CS8602 // 解引用可能出现空引用。
            thm.RequestedThemeChanged += OnRequestedThemeChanged;
#pragma warning restore CS8602 // 解引用可能出现空引用。

            // Enable Mica on Windows 11
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // TODO: add Windows version to CoreWindow
                if (IsWindows11 && thm.RequestedTheme != FluentAvaloniaTheme.HighContrastModeString)
                {
                    TransparencyBackgroundFallback = Brushes.Transparent;
                    TransparencyLevelHint = WindowTransparencyLevel.Mica;

                    TryEnableMicaEffect(thm);
                }
            }

            thm.ForceWin32WindowToTheme(this);

            var screen = Screens.ScreenFromVisual(this);
            if (screen != null)
            {
                double width = Width;
                double height = Height;

                if (screen.WorkingArea.Width > 1280)
                {
                    width = 1280;
                }
                else if (screen.WorkingArea.Width > 1000)
                {
                    width = 1000;
                }
                else if (screen.WorkingArea.Width > 700)
                {
                    width = 700;
                }
                else if (screen.WorkingArea.Width > 500)
                {
                    width = 500;
                }
                else
                {
                    width = 450;
                }

                if (screen.WorkingArea.Height > 720)
                {
                    width = 720;
                }
                else if (screen.WorkingArea.Height > 600)
                {
                    width = 600;
                }
                else if (screen.WorkingArea.Height > 500)
                {
                    width = 500;
                }
                else
                {
                    width = 400;
                }
            }
            AutoUpdata();
        }

        private void OnRequestedThemeChanged(FluentAvaloniaTheme sender, RequestedThemeChangedEventArgs args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // TODO: add Windows version to CoreWindow
                if (IsWindows11 && args.NewTheme != FluentAvaloniaTheme.HighContrastModeString)
                {
                    TryEnableMicaEffect(sender);
                }
                else if (args.NewTheme == FluentAvaloniaTheme.HighContrastModeString)
                {
                    // Clear the local value here, and let the normal styles take over for HighContrast theme
                    SetValue(BackgroundProperty, AvaloniaProperty.UnsetValue);
                }
            }
        }

        private void InfoBar_CloseButtonClick(InfoBar sender, object args)
        {
            var viewData = sender.DataContext as InfoBarModel;
            if (viewData != null)
            {
                viewData.Removed = true;

                InfoBarItems.Remove(viewData);
                InformationListBox.Items = null;
                InformationListBox.Items = InfoBarItems;
            }
        }
        
        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
#pragma warning disable CS8602 // 解引用可能出现空引用。
            AcrylicBorder.Material = new ExperimentalAcrylicMaterial()
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintColor = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme is "Dark" ? Colors.Black : Colors.White,
                TintOpacity = 1,
                MaterialOpacity = 1,
            };
#pragma warning restore CS8602 // 解引用可能出现空引用。
        }

        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            AcrylicColorChange();
        }

        public void CancelButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            VersionDialog.Hide();
        }

        public void StartVersionOlateClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            App.Data.Isolate = true;
            VersionDialog.Hide();
        }

        public static async void ShowVersionDialogAsync()
        {
            await ContentDialogView.ShowAsync();
        }

        private void D_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            TipClose();
        }

        public void TipShow()
        {
            TeachingTipAnimation animation = new();
            animation.RunAsync(TeachingTipHost);
        }

        public void TipClose()
        {
            TeachingTipAnimation animation = new(true);
            animation.RunAsync(TeachingTipHost);
        }

        public void InitializeComponent()
        {
            InitializeComponent(true);
            TipClose();
            BarHost.Attach(this);
            win = this;
            MainWindowViewModel.TitleBar = BarHost;
            InformationListBox = InformationList;
            ContentDialogView = VersionDialog;
            AcrylicColorChange();
            EnableMica();
            //SplashScreen = new SplashScreenStyle();
            ViewModel = new MainWindowViewModel(this);
            DataContext = ViewModel;
            MainPanel.Children.Add(new MainView());
            //this.Activated += MainWindow_Activated;
            //Deactivated += MainWindow_Deactivated;
            Closed += MainWindow_Closed;
            FluentTheme theme = new(new Uri("avares://WonderLab"));
            theme.Mode = FluentThemeMode.Light;
            //var faTheme = AvaloniaLocator.Current.GetService<FluentTheme>();
            //faTheme.Mode = FluentThemeMode.Light;
            d.Click += D_Click;
        }
    }


    partial class MainWindow
    {
        public MainWindow() => InitializeComponent();

        //CancelButtonClick
        public MainWindowViewModel ViewModel { get; protected set; }
        private static List<InfoBarModel> InfoBarItems = new();
        private static ListBox InformationListBox { get; set; }
        private static ContentDialog ContentDialogView { get; set; }
        public static MainWindow win { get; set; }
    }
}
//Environment.OSVersion.Version