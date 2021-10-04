﻿using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Util;

namespace Backend
{
    public class DataContainer : NotifyPropertyChangedBase
    {
        public static DataContainer Instance { get; } = new();
        private DataContainer() { }

        public void Clear()
        {
            User = null;
            Tags = null;
            MetaPlaylists.Clear();
            LikedPlaylists.Clear();
            GeneratedPlaylists.Clear();
        }


        // spotify data
        private PrivateUser user;
        public PrivateUser User
        {
            get => user;
            set => SetProperty(ref user, value, nameof(User));
        }

        #region playlists
        public ObservableCollection<Playlist> MetaPlaylists { get; } = new();
        public ObservableCollection<Playlist> LikedPlaylists { get; } = new();
        public ObservableCollection<Playlist> GeneratedPlaylists { get; } = new();
        public async Task LoadSourcePlaylists()
        {
            Log.Information("Loading source playlists");
            var newLiked = await DatabaseOperations.PlaylistsLiked();
            var newMeta = DatabaseOperations.PlaylistsMeta();
            Utils.SyncLists(LikedPlaylists, newLiked);
            Utils.SyncLists(MetaPlaylists, newMeta);
        }
        public void LoadGeneratedPlaylists()
        {
            Log.Information("Loading generated playlists");
            Utils.SyncLists(GeneratedPlaylists, DatabaseOperations.PlaylistsGenerated());
        }
        #endregion


        #region tags
        private bool isLoadingTags;
        public bool IsLoadingTags
        {
            get => isLoadingTags;
            set => SetProperty(ref isLoadingTags, value, nameof(IsLoadingTags));
        }
        private ObservableCollection<Tag> tags;
        public ObservableCollection<Tag> Tags
        {
            get => tags;
            set => SetProperty(ref tags, value, nameof(Tags));
        }
        public async Task LoadTags()
        {
            if (Tags != null) return;

            Log.Information("Loading tags");
            IsLoadingTags = true;
            var dbTags = await ConnectionManager.Instance.Database.Tags.ToListAsync();
            Tags = new ObservableCollection<Tag>(dbTags);
            IsLoadingTags = false;
        }
        #endregion
    }
}
