using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WonderLab.Modules.Base;
using WonderLab.Modules.Media;
using WonderLab.Modules.Models;
using WonderLab.Modules.Styles;
using WonderLab.Modules.Toolkits;
using WonderLab.ViewModels;
using WonderLab.Views;
using static WonderLab.ViewModels.OtherViewModel;
using static WonderLab.ByDdggdd135.utils.StaticVar;
using Brushes = Avalonia.Media.Brushes;
using Color = Avalonia.Media.Color;
using FluentAvalonia.UI.Media.Animation;

namespace WonderLab
{
    public partial class MainWindow : Window
    {
        public bool IsWindows11 { get => (Environment.OSVersion.Version.Build >= 22000); }

        public void AcrylicColorChange()
        {
            if (!IsWindows11)
            {
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
            return "1.0.1.0";
        }
        public static void AutoUpdata()
        {
            try{
                string releaseUrl = GithubLib.GithubLib.GetRepoLatestReleaseUrl("Blessing-Studio", "WonderLab");
                Release? release = GithubLib.GithubLib.GetRepoLatestRelease(releaseUrl);
                if (release != null)
                {
                    if (release.name != GetVersion())
                    {
                        var button = new HyperlinkButton()
                        {
                            Content = "�������",
                        };
                        button.Click += Button_Click;
                        ShowInfoBarAsync("�Զ�����", "�����°汾" + release.name + "  ��ǰ�汾" + GetVersion() + "  ", InfoBarSeverity.Informational, 7000, button);
                    }
                }
            }
            finally
            {

            };
        }

        public static void Button_Click(object? sender, RoutedEventArgs e)
        {
            CanUpdata = false;
            var res = Updata();
            var button = new HyperlinkButton()
            {
                Content = "ת�� ף���ն�>��������"
            };
            button.Click += Button_Click1;
            MainWindow.ShowInfoBarAsync("��ʾ��", $"��ʼ���ظ���  ��������:\n {res.body} \n\n������{res.author.login} \n ��ǰ���������Ĳ鿴����", InfoBarSeverity.Informational, 8000, button);
            string save = @"updata.zip";
            if (Directory.Exists("updata-cache"))
            {
                File.Delete(Path.Combine("updata-cache", save));
            }
            string url = null;
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
            DownItemView downItemView = new DownItemView(httpDownload, $"����  {res.name} ����", new AfterDo(After_Do));
            TaskView.Add(downItemView);
        }

        private static void After_Do()
        {
            File.Create(Path.Combine("updata-cache", "UpdataNextTime")).Close();
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
            thm.RequestedThemeChanged += OnRequestedThemeChanged;

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
            viewData.Removed = true;

            InfoBarItems.Remove(viewData);
            InformationListBox.Items = null;
            InformationListBox.Items = InfoBarItems;
        }
        
        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            AcrylicBorder.Material = new ExperimentalAcrylicMaterial()
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintColor = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>().RequestedTheme is "Dark" ? Colors.Black : Colors.White,
                TintOpacity = 1,
                MaterialOpacity = 1,
            };
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