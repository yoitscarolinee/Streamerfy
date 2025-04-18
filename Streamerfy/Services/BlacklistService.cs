﻿using Newtonsoft.Json;
using Streamerfy.Data.Internal.Json;
using Streamerfy.Windows;
using System.IO;
using System.Net.Http;
using System.Windows.Media;

namespace Streamerfy.Services
{
    public class BlacklistService
    {
        public HashSet<string> Tracks { get; private set; } = new();
        public HashSet<string> Artists { get; private set; } = new();
        public HashSet<string> Users { get; private set; } = new();
        public HashSet<string> Global { get; private set; } = new();

        public BlacklistService()
            => Load();

        #region Tracks
        public bool IsTrackBlacklisted(string id) => Tracks.Contains(id) || Global.Contains(id);

        public void AddTrack(string id)
        {
            Tracks.Add(id);
            Save();
        }

        public void RemoveTrack(string id)
        {
            Tracks.Remove(id);
            Save();
        }
        #endregion

        #region Artists
        public bool IsArtistBlacklisted(string id) => Artists.Contains(id);

        public void AddArtist(string id)
        {
            Artists.Add(id);
            Save();
        }

        public void RemoveArtist(string id)
        {
            Artists.Remove(id);
            Save();
        }
        #endregion

        #region Users
        public bool IsUserBlacklisted(string username) => Users.Contains(username);

        public void AddUser(string username)
        {
            Users.Add(username);
            Save();
        }

        public void RemoveUser(string username)
        {
            Users.Remove(username);
            Save();
        }
        #endregion

        #region Internal Methods
        private void Save()
        {
            File.WriteAllText(App.SongBlacklistFile, JsonConvert.SerializeObject(Tracks, Formatting.Indented));
            File.WriteAllText(App.ArtistBlacklistFile, JsonConvert.SerializeObject(Artists, Formatting.Indented));
            File.WriteAllText(App.UserBlacklistFile, JsonConvert.SerializeObject(Users, Formatting.Indented));
        }

        private void Load()
        {
            // Load Blacklisted Tracks File
            if (!File.Exists(App.SongBlacklistFile))
            {
                File.Create(App.SongBlacklistFile).Close();
                File.WriteAllText(App.SongBlacklistFile, JsonConvert.SerializeObject(Tracks, Formatting.Indented));
            }
            else
            {
                var json = File.ReadAllText(App.SongBlacklistFile);
                Tracks = JsonConvert.DeserializeObject<HashSet<string>>(json) ?? new();
            }

            // Load Blacklisted Artists File
            if (!File.Exists(App.ArtistBlacklistFile))
            {
                File.Create(App.ArtistBlacklistFile).Close();
                File.WriteAllText(App.ArtistBlacklistFile, JsonConvert.SerializeObject(Artists, Formatting.Indented));
            }
            else
            {
                var json = File.ReadAllText(App.ArtistBlacklistFile);
                Artists = JsonConvert.DeserializeObject<HashSet<string>>(json) ?? new();
            }

            // Load Blacklisted Users File
            if (!File.Exists(App.UserBlacklistFile))
            {
                File.Create(App.UserBlacklistFile).Close();
                File.WriteAllText(App.UserBlacklistFile, JsonConvert.SerializeObject(Users, Formatting.Indented));
            }
            else
            {
                var json = File.ReadAllText(App.UserBlacklistFile);
                Users = JsonConvert.DeserializeObject<HashSet<string>>(json) ?? new();
            }

            // Fetch Global Track Blacklist
            Task.Run(async () =>
            {
                try
                {
                    await LanguageService.WaitUntilReadyAsync();

                    Global = await FetchGlobalBlacklistAsync();
                    MainWindow.Instance.AddLog(LanguageService.Translate("Message_Global_Blacklist_Success"), Colors.LimeGreen);
                }
                catch (Exception ex)
                {
                    await LanguageService.WaitUntilReadyAsync();
                    MainWindow.Instance.AddLog(LanguageService.Translate("Message_Global_Blacklist_Failure", new { ERROR = ex.Message }), Colors.Red);
                }
            });

        }

        private async Task<HashSet<string>> FetchGlobalBlacklistAsync()
        {
            using var http = new HttpClient();
            var response = await http.GetStringAsync("https://raw.githubusercontent.com/WTFBlaze/Streamerfy/refs/heads/master/GlobalBlacklist.txt");
            var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new HashSet<string>(lines);
        }
        #endregion
    }
}
