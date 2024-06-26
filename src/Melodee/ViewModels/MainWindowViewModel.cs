﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using Avalonia.Media.Imaging;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Grids;
using Melodee.Models;
using Melodee.Plugins.Discovery.Directories;
using Melodee.Plugins.Discovery.Releases;
using ReactiveUI;

namespace Melodee.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private Configuration Configuration { get; }

    private IDirectoriesDiscoverer DirectoriesDiscoverer { get; }
    
    public IReleasesDiscoverer ReleasesDiscoverer { get; }
    
    public ObservableCollection<Node> InboundDirectoryInfos { get; }

    public ObservableCollection<ReleaseGrid> ReleaseInfos { get; set; } = [];
    
    public FileSystemDirectoryInfo SelectedFileSystemDirectoryInfo { get; set; }
    
    public WriteableBitmap ReleasePrimaryCoverImage { get; set; }
   
    private bool _showSelectedRelease;

    public bool ShowSelectedRelease
    {
        get => _showSelectedRelease;
        set => this.RaiseAndSetIfChanged(ref _showSelectedRelease, value);
    }

    private ReleaseDetail? _releaseDetail;
    
    public ReleaseDetail? SelectedRelease
    {
        get => _releaseDetail;
        set
        {
            this.RaiseAndSetIfChanged(ref _releaseDetail, value);
            ShowSelectedRelease = _releaseDetail != null;            
        }
    }
    
    public bool IsLoading { get; set; }
    
    public MainWindowViewModel(Configuration configuration, IDirectoriesDiscoverer directoriesDiscoverer, IReleasesDiscoverer releasesDiscoverer)
    {
        Configuration = configuration;
        DirectoriesDiscoverer = directoriesDiscoverer;
        ReleasesDiscoverer = releasesDiscoverer;

        var directories = DirectoriesDiscoverer.DirectoryInfosForDirectory(
            new System.IO.DirectoryInfo(Configuration.InboundDirectory), 
            new PagedRequest());

        var result = new List<Node>();
        var dirData = directories.Data.ToArray();
        var inboundDirectoryDirectoryInfo = dirData.First(x => x.ParentId == 0);
        foreach (var directory in dirData.Where(x => x.ParentId == inboundDirectoryDirectoryInfo.UniqueId && x.ShowInTree))
        {
            var node = new Node(directory, directory.Name);
            var childNodes = NodesForDirectoryInfo(dirData, directory).ToArray();
            if (childNodes.Any())
            {
                node.SubNodes = new ObservableCollection<Node>(childNodes);
            }
            result.Add(node);
        }
        InboundDirectoryInfos = new ObservableCollection<Node>(result);      
    }

    private IEnumerable<Node> NodesForDirectoryInfo(IEnumerable<FileSystemDirectoryInfo> data, FileSystemDirectoryInfo fileSystemDirectoryInfo)
    {
        var directoryInfos = data as FileSystemDirectoryInfo[] ?? data.ToArray();
        var result = directoryInfos
            .Where(x => x.ParentId == fileSystemDirectoryInfo.UniqueId && x.ShowInTree)
            .Select(x => new Node(x, x.Name))
            .ToArray();
        foreach (var node in result)
        {
            var ccChildNodes = NodesForDirectoryInfo(directoryInfos, node.FileSystemDirectoryInfo).ToArray();
            if (ccChildNodes.Length != 0)
            {
                node.SubNodes = new ObservableCollection<Node>(ccChildNodes);
            }
        }        
        return result;
    }
}