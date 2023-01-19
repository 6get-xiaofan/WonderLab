﻿using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using JetBrains.Annotations;
using MinecraftLaunch.Modules.Installer;
using MinecraftLaunch.Modules.Models.Download;
using MinecraftLaunch.Modules.Models.Install;
using MinecraftLaunch.Modules.Models.Launch;
using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentCore.Class.Model.Install.Vanilla;
using Natsurainko.FluentCore.Module.Installer;
using Natsurainko.FluentCore.Service;
using Natsurainko.Toolkits.Network;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WonderLab.Modules.Base;
using WonderLab.Modules.Controls;
using WonderLab.Modules.Enum;
using WonderLab.Modules.Models;
using WonderLab.Views;

namespace WonderLab.ViewModels
{
    //Binding
    public partial class DownGameViewModel : ViewModelBase
    {        
        public string CurrentGameCore { get => _CurrentGameCore; set => RaiseAndSetIfChanged(ref _CurrentGameCore, value); }
        public string CurrentForgeDescription { get => _CurrentForgeDescription; set => RaiseAndSetIfChanged(ref _CurrentForgeDescription, value); }
        public string CurrentFabricDescription { get => _CurrentFabricDescription; set => RaiseAndSetIfChanged(ref _CurrentFabricDescription, value); }
        public string CurrentOpitfineDescription { get => _CurrentOpitfineDescription; set => RaiseAndSetIfChanged(ref _CurrentOpitfineDescription, value); }
        public string CurrentGameCoreType { get => _CurrentGameCoreType; set => RaiseAndSetIfChanged(ref _CurrentGameCoreType, value); }
        public string DownloadCore { get => _DownloadCore; set => RaiseAndSetIfChanged(ref _DownloadCore, value); }
        public string DownloadCoreType { get => _DownloadCoreType; set => RaiseAndSetIfChanged(ref _DownloadCoreType, value); }
        public bool IsEnableIndependencyCore { get => _IsEnableIndependencyCore; set => RaiseAndSetIfChanged(ref _IsEnableIndependencyCore, value); }
        public bool IsExpand { get => _IsExpand; set => RaiseAndSetIfChanged(ref _IsExpand, value); }
        public bool Normal { get => _Normal; set => RaiseAndSetIfChanged(ref _Normal, value); }
        public bool New { get => _New; set => RaiseAndSetIfChanged(ref _New, value); }
        public bool Old { get => _Old; set => RaiseAndSetIfChanged(ref _Old, value); }
        public bool CanForgeInstall { get => _CanForgeInstall; set => RaiseAndSetIfChanged(ref _CanForgeInstall, value); }
        public bool CanOptiFineInstall { get => _CanOptiFineInstall; set => RaiseAndSetIfChanged(ref _CanOptiFineInstall, value); }
        public bool CanFabricInstall { get => _CanFabricInstall; set => RaiseAndSetIfChanged(ref _CanFabricInstall, value); }
        public bool ForgeRemoveVisible { get => _ForgeRemoveVisible; set => RaiseAndSetIfChanged(ref _ForgeRemoveVisible, value); }
        public bool FabricRemoveVisible { get => _FabricRemoveVisible; set => RaiseAndSetIfChanged(ref _FabricRemoveVisible, value); }
        public bool OptifineRemoveVisible { get => _OptifineRemoveVisible; set => RaiseAndSetIfChanged(ref _OptifineRemoveVisible, value); }
        public bool HasModLoader { get => _HasModLoader; set => RaiseAndSetIfChanged(ref _HasModLoader, value); }
        public bool HasGameCore { get => _HasModLoader; set => RaiseAndSetIfChanged(ref _HasModLoader, value); }
        public bool IsGameListLoadOk { get => _IsGameListLoadOk; set => RaiseAndSetIfChanged(ref _IsGameListLoadOk, value); }
        public bool IsForgeListLoadOk { get => _IsForgeListLoadOk; set => RaiseAndSetIfChanged(ref _IsForgeListLoadOk, value); }
        public bool IsFabricListLoadOk { get => _IsFabricListLoadOk; set => RaiseAndSetIfChanged(ref _IsFabricListLoadOk, value); }
        public bool IsOptiFineListLoadOk { get => _IsOptiFineListLoadOk; set => RaiseAndSetIfChanged(ref _IsOptiFineListLoadOk, value); }
        public List<GameCore> GameCores { get => _GameCores; set => RaiseAndSetIfChanged(ref _GameCores, value); }
        public List<ModLoaderInformationViewData> Forges { get => _Forges; set => RaiseAndSetIfChanged(ref _Forges, value); }
        public List<ModLoaderInformationViewData> Fabrics { get => _Fabrics; set => RaiseAndSetIfChanged(ref _Fabrics, value); }
        public List<ModLoaderInformationViewData> OptiFines { get => _OptiFines; set => RaiseAndSetIfChanged(ref _OptiFines, value); }
        public GameCore SelectedGameCore 
        {
            get => _SelectedGameCore;
            set 
            {
                if (RaiseAndSetIfChanged(ref _SelectedGameCore, value) && value is not null)
                {
                    HasGameCore = true;
                    IsExpand = false;
                    CurrentGameCore = value.Id;
                    CurrentGameCoreType = value.Type;
                    DownloadCore = value.Id;
                    Clear();
                    Change();
                }
                else
                {
                    HasModLoader = false;
                    HasGameCore = false;
                }
            }
        }
        public ModLoaderInformationViewData SelectForge
        {
            get => _Forge;
            set
            {
                if (RaiseAndSetIfChanged(ref _Forge, value) && value is not null)
                {
                    CanFabricInstall = false;
                    ModLoaders.Add(value);
                    if (OldForge is not null)
                        ModLoaders.Remove(OldForge);
                    OldForge = value;
                    ForgeRemoveVisible = true;
                    if(SelectOptiFine is not null)
                        CurrentFabricDescription = "此加载器与 Forge 和 Opitfine 不兼容";
                    else if (Fabrics is not null && Fabrics.Count >= 0) CurrentFabricDescription = "此加载器与 Forge 不兼容";                    
                    CurrentForgeDescription = value.Data.Version;
                    DownloadCoreType = $"当前选中的加载器：{string.Join(",", ModLoaders.Select(x => x.Data.LoaderName + "_" + x.Data.Main.Version))}";
                }
            }
        }
        public ModLoaderInformationViewData SelectFabric
        {
            get => _Fabric;
            set
            {
                if (RaiseAndSetIfChanged(ref _Fabric, value) && value is not null)
                {
                    if (value.Data.CanSelected is 0 || value.Data.CanSelected is 2)
                    {
                        CanForgeInstall = false;
                        CanOptiFineInstall = false;
                        ModLoaders.Add(value);
                        if (OldFabric is not null)
                            ModLoaders.Remove(OldFabric);
                        OldFabric = value;
                        FabricRemoveVisible = true;
                        if (Forges.Count >= 0)
                        {
                            CurrentForgeDescription = "此加载器与 Fabric 不兼容";
                        }
                        else if(OptiFines.Count >= 0)
                        {
                            CurrentOpitfineDescription = "此加载器与 Fabric 不兼容";
                        }
                        CurrentFabricDescription = value.Data.Version;
                        DownloadCoreType = $"当前选中的加载器：{string.Join(",", ModLoaders.Select(x => x.Data.LoaderName + "_" + x.Data.Main.Version))}";
                    }
                }
            }
        }
        public ModLoaderInformationViewData SelectOptiFine
        {
            get => _OptiFine;
            set
            {
                if (RaiseAndSetIfChanged(ref _OptiFine, value) && value is not null)
                {
                    CanFabricInstall = false;
                    ModLoaders.Add(value);
                    if (OldOptifine is not null)
                        ModLoaders.Remove(OldOptifine);
                    OldOptifine = value;
                    OptifineRemoveVisible = true;
                    if (SelectForge is not null)
                        CurrentFabricDescription = "此加载器与 Forge 和 Opitfine 不兼容";
                    else if (Fabrics.Count >= 0) CurrentFabricDescription = "此加载器与 OptiFine 不兼容";
                    CurrentOpitfineDescription = value.Data.Version;
                    DownloadCoreType = $"当前选中的加载器：{string.Join(",", ModLoaders.Select(x => x.Data.LoaderName + "_" + x.Data.Main.Version))}";
                }
            }
        }
    }
    //Method
    partial class DownGameViewModel
    {
        public DownGameViewModel()
        {
            LoadVersionList();
        }
        public void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            BlessingView.IsTask = true;
            MainView.mv.FrameView.Navigate(typeof(BlessingView));
            MainView.mv.main.IsSelected = true;
            BlessingView.view.FrameView.Navigate(typeof(TaskView), null, new SlideNavigationTransitionInfo());
        }
        public async void LoadVersionList()
        {
            //拉取列表
            var v1 = await Task.Run(async () =>
            {
                CoreManifest core = null;
                try
                {
                    core = await MinecraftVanlliaInstaller.GetCoreManifest();
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        MainWindow.ShowInfoBarAsync("错误", "游戏版本列表拉取失败", InfoBarSeverity.Error);
                    });
                }
                return core;
            });
            //转换
            if (v1 is not null)
            {
                await Task.Run(() =>
                {
                    foreach (var i in v1.Cores)
                    {
                        GameCore tm = new();
                        if (i.Type is "release")
                        {
                            tm.Type = $"正式版 {i.Id}";
                            tm.Id = i.Id;
                            Release.Add(tm);
                        }
                        else if (i.Type is "snapshot")
                        {
                            tm.Type = $"快照版 {i.Id}";
                            tm.Id = i.Id;
                            Snapshot.Add(tm);
                        }
                        else if (i.Type is "old_alpha")
                        {
                            tm.Type = $"远古版 {i.Id}";
                            tm.Id = i.Id;
                            OldData.Add(tm);
                        }
                    }
                    GameCores = Release;
                    IsGameListLoadOk = false;
                });
            }
            else
            {
                MainWindow.ShowInfoBarAsync("错误", "版本列表拉取失败！", InfoBarSeverity.Error);
            }
        }
        public void Kill()
        {
            Normal = false;
            New = false;
            Old = false;
            CurrentGameCore = DefaultCurrentGameCore;
            CurrentGameCoreType = DefaultCurrentGameCoreType;
        }
        public void SelectedRelease()
        {
            Kill();
            GameCores = Release;
            Normal = true;
        }
        public void SelectedSnapshot()
        {
            Kill();
            GameCores = Snapshot;
            New = true;
        }
        public void SelectedOldData()
        {
            Kill();
            GameCores = OldData;
            Old = true;
        }
        public void InstallAsync()
        {
            bool isinstall = false;

            if (DownloadCore is "未选择核心")
            {
                MainWindow.ShowInfoBarAsync("错误", "未选择任何核心，无法进行安装", InfoBarSeverity.Error);
                return;
            }

            if (App.Data.SelectedGameFooter == string.Empty)
            {
                MainWindow.ShowInfoBarAsync("错误", "未选择任何游戏文件夹路径，无法进行安装", InfoBarSeverity.Error);
                return;
            }

            if (DownloadCoreType is not "未选择额外安装组件" && DownloadCoreType is not "未选择任何版本")
            {
                if (string.IsNullOrEmpty(App.Data.JavaPath))
                {
                    MainWindow.ShowInfoBarAsync("错误", "未选择 Java 运行时，无法安装", InfoBarSeverity.Error);
                    return;
                }

                if (!App.Data.Isolate && !IsEnableIndependencyCore)
                {
                    MainWindow.ShowVersionDialogAsync();
                    return;
                }

                DownItemView downItemView1 = new(ModLoaders);
                TaskView.Add(downItemView1);
                isinstall = true;
            }

            var button = new HyperlinkButton()
            {
                Content = "转至 祝福终端>任务中心",
            };

            button.Click += Button_Click;

            if(isinstall is not true)
            {
                DownItemView downItemView = new DownItemView(DownloadCore, DownType.Vanllia);
                TaskView.Add(downItemView);
            }

            MainWindow.ShowInfoBarAsync("提示", $"开始安装游戏核心：{DownloadCore}，可前往任务中心查看进度", InfoBarSeverity.Informational, 5000, button);

            Page.NavigatedToDownView();
        }
        public async void Change()
        {
            #region Load
            IsFabricListLoadOk = true;
            IsForgeListLoadOk = true;
            IsOptiFineListLoadOk = true;
            CurrentForgeDescription = "加载中...";
            CurrentFabricDescription = "加载中...";
            CurrentOpitfineDescription = "加载中...";
            #endregion

            #region Forge Load
            Forges = (await ForgeInstaller.GetForgeBuildsOfVersionAsync(DownloadCore)).Select(x =>
            {
                var data = new ModLoaderInformation()
                {
                    LoaderType = ModLoaderType.Forge,
                    McVersion = x.McVersion,
                    Version = x.ForgeVersion,
                }.CreateViewData<ModLoaderInformation, ModLoaderInformationViewData>();

                data.Build = x;
                return data;
            }).ToList();

            if (Forges.Count <= 0)
                CurrentForgeDescription = "没有可用版本";
            else if (SelectFabric is null || string.IsNullOrEmpty(SelectFabric.Data.Version)) CurrentForgeDescription = "未选择任何版本";
            else CurrentForgeDescription = "此加载器与 Fabric 不兼容";

            IsForgeListLoadOk = false;
            #endregion

            #region Fabric Load
            Fabrics = (await FabricInstaller.GetFabricBuildsByVersionAsync(DownloadCore)).Select(x =>
            {
                var data = new ModLoaderInformation()
                {
                    LoaderType = ModLoaderType.Fabric,
                    McVersion = DownloadCore,
                    Version = x.Loader.Version
                }.CreateViewData<ModLoaderInformation, ModLoaderInformationViewData>();

                data.Build = x;
                return data;
            }).ToList();

            if (Fabrics.Count <= 0)
                CurrentFabricDescription = "没有可用版本";
            else if ((SelectForge is null && SelectOptiFine is null))
                CurrentFabricDescription = "未选择任何版本";
            else if (SelectOptiFine is null && SelectForge is not null) CurrentFabricDescription = "此加载器与 Forge 不兼容";
            else if (SelectForge is null && SelectOptiFine is not null) CurrentFabricDescription = "此加载器与 Optifine 不兼容";
            else if (SelectForge is not null && SelectOptiFine is not null) CurrentFabricDescription = "此加载器与 Forge 和 Opitfine 不兼容";

            IsFabricListLoadOk = false;
            #endregion

            #region OptiFine Load
            OptiFines = (await OptiFineInstaller.GetOptiFineBuildsFromMcVersionAsync(DownloadCore)).Select(x =>            
            {
                var data = new ModLoaderInformation()
                {
                    LoaderType = ModLoaderType.OptiFine,
                    McVersion = x.McVersion,
                    Version = $"{x.Type}_{x.Patch}"
                }.CreateViewData<ModLoaderInformation, ModLoaderInformationViewData>();
                data.Build = x;
                return data;
            }).ToList();

            if (OptiFines.Count <= 0)
                CurrentOpitfineDescription = "没有可用版本";
            else if(SelectFabric is null || string.IsNullOrEmpty(SelectFabric.Data.Version))
                CurrentOpitfineDescription = "未选择任何版本";
            else CurrentOpitfineDescription = "此加载器与 Fabric 不兼容";

            IsOptiFineListLoadOk = false;
            #endregion
        }
        public void BaseRemove()
        {
            if (ModLoaders.Count > 0)
                DownloadCoreType = $"当前选中的加载器：{string.Join(",", ModLoaders.Select(x => x.Data.LoaderName + "_" + x.Data.Main.Version))}";
            else
                DownloadCoreType = DefaultInstall;
        }
        public void RemoveForge()
        {
            if (ModLoaders is not null)
            {
                ModLoaders.Remove(SelectForge);
                SelectForge = null;
                CurrentForgeDescription = "未选择任何版本";
                if (SelectOptiFine is not null)
                    CurrentFabricDescription = "此加载器与 OptiFine 不兼容";
                else if (Fabrics is not null && Fabrics.Count >= 0) CurrentFabricDescription = "未选择任何版本";
                OldForge = null;
                ForgeRemoveVisible = false;
                BaseRemove();
            }
        }
        public void RemoveFabric()
        {
            if (ModLoaders is not null)
            {
                ModLoaders.Remove(SelectFabric);
                SelectFabric = null;
                OldFabric = null;
                if (Forges is not null && Forges.Count >= 0)
                {
                    CurrentForgeDescription = "未选择任何版本";
                }
                else if (OptiFines is not null && OptiFines.Count >= 0) CurrentOpitfineDescription = "未选择任何版本";
                
                CurrentFabricDescription = "未选择任何版本";
                FabricRemoveVisible = false;
                BaseRemove();
            }
        }
        public void RemoveOptiFine()
        {
            if (ModLoaders is not null)
            {
                ModLoaders.Remove(SelectOptiFine);
                SelectOptiFine = null;
                OldOptifine = null;
                CurrentOpitfineDescription = "未选择任何版本";
                if (SelectForge is not null)
                    CurrentFabricDescription = "此加载器与 Forge 不兼容";
                else if (Fabrics is not null && Fabrics.Count >= 0) CurrentFabricDescription = "未选择任何版本";

                OptifineRemoveVisible = false;
                BaseRemove();
            }
        }
        public void Clear()
        {
            RemoveForge();
            RemoveFabric();
            RemoveOptiFine();
            ModLoaders = new();
            Forges = null;
            Fabrics = null;
            OptiFines = null;
        }
        public void ClearAll()
        {
            #region Game
            DownloadCore = DefaultDownCore;
            CurrentGameCore = DefaultCurrentGameCore;
            CurrentGameCoreType = DefaultCurrentGameCoreType;
            #endregion

            #region ModLoader
            RemoveForge();
            RemoveFabric();
            RemoveOptiFine();
            ModLoaders = new();
            HasModLoader = false;
            Forges = null;
            Fabrics = null;
            OptiFines = null;
            DownloadCoreType = DefaultInstall;
            #endregion
        }
    }
    //OtherModel
    partial class DownGameViewModel
    {        
        public bool _CanForgeInstall = true;
        public bool _CanFabricInstall = true;
        public bool _CanOptiFineInstall = true;
        public bool _Normal = true;
        public bool _IsGameListLoadOk = true;
        public bool _IsForgeListLoadOk = true;
        public bool _IsFabricListLoadOk = true;
        public bool _IsOptiFineListLoadOk = true;
        public bool _ForgeRemoveVisible = false;
        public bool _FabricRemoveVisible = false;
        public bool _OptifineRemoveVisible = false;
        public bool _New = false;
        public bool _Old = false;
        public bool _HasModLoader = false;
        public bool _IsEnableIndependencyCore = false;
        public bool _HasGameCore = false;
        public bool _IsExpand = false;
        public string _CurrentGameCore = DefaultCurrentGameCore;
        public string _CurrentOpitfineDescription = "未选择任何版本";
        public string _CurrentFabricDescription = "未选择任何版本";
        public string _CurrentForgeDescription = "未选择任何版本"; 
        public string _CurrentGameCoreType = DefaultCurrentGameCoreType;
        public string _DownloadCore = DefaultDownCore;
        public string _DownloadCoreType = DefaultInstall;
        public GameCoresEntity CoreManifest;
        public GameCore _SelectedGameCore  = null;
        public List<GameCore>? _GameCores = new();
        public List<ModLoaderInformationViewData> ModLoaders = new();
        public List<GameCore> Release = new();
        public List<GameCore> OldData = new();
        public List<GameCore> Snapshot = new();
        public List<ModLoaderInformationViewData> _Forges;
        public List<ModLoaderInformationViewData> _Fabrics;
        public List<ModLoaderInformationViewData> _OptiFines;
        public ModLoaderInformationViewData _Forge;
        public ModLoaderInformationViewData _Fabric;
        public ModLoaderInformationViewData _OptiFine;
        public ModLoaderInformationViewData OldForge;
        public ModLoaderInformationViewData OldFabric;
        public ModLoaderInformationViewData OldOptifine;
        //public ModLoaderInformationViewData _SelectedModLoader;
    }
    //Consts
    partial class DownGameViewModel
    {
        const string DefaultCurrentGameCore = "下载列表";
        const string DefaultCurrentGameCoreType = "选择一个游戏核心";
        const string DefaultInstall = "未选择额外安装组件";
        const string DefaultDownCore = "未选择核心";
    }

    public class ModLoaderInformation : ViewModelBase
    {
        public ModLoaderInformation() => Debug.Write("");

        public ModLoaderInformation(int iss) => CanSelected = iss;

        public ModLoaderType LoaderType { get; set; }

        public string Version { get; set; }

        public string McVersion { get; set; }

        public string LoaderName => LoaderType.ToString();

        public DateTime? ReleaseTime { get; set; }

        public ModLoaderInformation Main => this;

        public int CanSelected
        {
            get => _CanSelected;
            set => RaiseAndSetIfChanged(ref _CanSelected, value);
        }

        public int _CanSelected = 0;//1: fa no 2: (f o) no
    }

    public static class T
    {
        public static TResult CreateViewData<TData, TResult>(this TData data) where TResult : ViewDataBase<TData>                      
                      => Activator.CreateInstance(typeof(TResult), data) as TResult;
    }

    public class ViewDataBase<T> : ViewModelBase
    {
        public T Data { get => _Data; set => RaiseAndSetIfChanged(ref _Data, value); }
        public T _Data;

        public ViewDataBase(T data) : base()
        {
            this.Data = data;
        }
    }

    public class ModLoaderInformationViewData : ViewDataBase<ModLoaderInformation>
    {
        public ModLoaderInformationViewData(ModLoaderInformation data) : base(data)
        {

        }

        public object Build { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Data.Version))
            {
                return "未选择任何加载器";
            }
            return Data.Version;
        }
    }
}
