using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WonderLab.Modules.Base;
using WonderLab.Modules.Toolkits;
using GithubLib;
using System.Net;
using System.IO;
using System.IO.Compression;
using Downloader;
using WonderLab.Views;
using Natsurainko.Toolkits.Network.Model;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace WonderLab.ViewModels
{
    public partial class OtherViewModel : ViewModelBase
    {
        public static Release? Updata()
        {
            string releaseUrl = GithubLib.GithubLib.GetRepoLatestReleaseUrl("Blessing-Studio", "WonderLab");
            Release? release = GithubLib.GithubLib.GetRepoLatestRelease(releaseUrl);
            return release;
        }
        public void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            BlessingView.IsTask = true;
            MainView.mv.FrameView.Navigate(typeof(BlessingView));
            MainView.mv.main.IsSelected = true;
            BlessingView.view.FrameView.Navigate(typeof(TaskView), null, new SlideNavigationTransitionInfo());
        }
        public async Task Check()
        {

        }
        public void DownloadUpdata(Release res)
        {
            var button = new HyperlinkButton()
            {
                Content = "转至 祝福终端>任务中心",
            };
            button.Click += Button_Click;
            MainWindow.ShowInfoBarAsync("提示：", $"开始下载更新  更新内容:\n {res.body} \n\n推送者{res.author.login} \n 可前往任务中心查看进度", InfoBarSeverity.Informational, 8000, button);
            string save = @"updata.zip";
            File.Delete(Path.Combine(save, "updata-cache"));
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
            DownItemView downItemView = new DownItemView(httpDownload, $"更新  {res.name} 下载", new AfterDo(After_do));
            TaskView.Add(downItemView);
        }
        public void After_do()
        {
        }
    }

    partial class OtherViewModel
    {
        public string Version
        {
            get => _Version;
            set => RaiseAndSetIfChanged(ref _Version, value);
        }
        public string ButtonContent
        {
            get => _ButtonContent;
            set => RaiseAndSetIfChanged(ref _ButtonContent, value);
        }

    }

    partial class OtherViewModel
    {
        public string _Version = MainWindow.GetVersion(); 
        public string _ButtonContent = "检查更新";
    }
}
