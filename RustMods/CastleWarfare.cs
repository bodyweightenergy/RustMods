using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Rust;
using Newtonsoft.Json.Linq;

namespace Oxide.Plugins 
{
    [Info("CastleWarfare", "BodyweightEnergy", "0.0.1", ResourceId = 0)]
    class CastleWarfare : RustPlugin
    {
        #region Plugin Constructor

        public CastleWarfare ()
        {
            players = new Dictionary<ulong, Team>();
            playerCredits = new Dictionary<ulong, int>();
            itemPrice = new Dictionary<string, int>();
        }

        #endregion

        #region Plugin Methods



        #endregion

        #region Hooks

        [HookMethod("OnServerInitialized")]
        private void OnServerInitialized()
        {
            try
            {
                saveData();
                loadData();
            }
            catch (Exception ex)
            {
                Error("OnServerInitialized failed", ex);
            }
        }

        #endregion

        #region Event Management

        #endregion

        #region UI Management



        #endregion

        #region Data Management

        private string dataFileName = "CastleWarfareData";  // filename in /data directory
        private Dictionary<ulong, Team> players;            // maps player ID to Team, volatile so doesn't need persistance
        private Dictionary<ulong, int> playerCredits;       // maps player ID to accumulative credits/tokens amount, requires persistance
        private Dictionary<string, int> itemPrice;          // maps item to configured price, always loaded from config file in /config directory

        private void loadData()
        {
            // Load Player Credits Data
            playerCredits.Clear();
            var data = Interface.GetMod().DataFileSystem.GetDatafile(dataFileName);
            if (data["playerCredits"] != null)
            {
                var playerCreditsData = (Dictionary<string, object>)data["playerCredits"];
                foreach (var playerData in playerCreditsData)
                {
                    playerCredits.Add(UInt64.Parse(playerData.Key), (int)playerData.Value);
                }
            }

            // Load Item Prices from Config file
            if(Config["ItemPrices"] == null)
            {
                LoadDefaultConfig();
            }
            var ItemPricesConfig = (Dictionary<string, object>) Config["ItemPrices"];
            foreach(var itemData in ItemPricesConfig)
            {
                itemPrice.Add((string) itemData.Key, (int) itemData.Value);
            }
        }

        private void saveData()
        {
            // Save Player Credits Data
            var data = Interface.GetMod().DataFileSystem.GetDatafile(dataFileName);
            var playerCreditsData = new Dictionary<ulong, int>();
            foreach (var player in playerCredits)
            { 
                playerCreditsData.Add(player.Key, player.Value);
            }
            data["playerCredits"] = playerCreditsData;
            Interface.GetMod().DataFileSystem.SaveDatafile(dataFileName);
        }

        private void ChangeCredit(BasePlayer player, int amount)
        {
            ulong playerID = player.userID;
            if(players.ContainsKey(playerID))
            {
                playerCredits[playerID] += amount;  // Positive amount = give credit, Negative amount = take credit
            }
            else 
            {
                Error("Cannot give credit to Player \"" + player.displayName + "\": Cannot find in player list.");
            }
        }

        #region Default Config
        private void LoadDefaultConfig()
        {
            // Write default config
            Config.Set("ItemPrices", "AK-47", 20);
            Config.Set("ItemPrices", "RifleAmmo", 10);
        }
        #endregion

        #endregion

        #region Utility Methods

        private void Log(string message)
        {
            Interface.Oxide.LogInfo("{0}: {1}", Title, message);
        }

        private void Warn(string message)
        {
            Interface.Oxide.LogWarning("{0}: {1}", Title, message);
        }

        private void Error(string message, Exception ex = null)
        {
            if (ex != null)
                Interface.Oxide.LogException(string.Format("{0}: {1}", Title, message), ex);
            else
                Interface.Oxide.LogError("{0}: {1}", Title, message);
        }

        #endregion

        #region Enumartions
        enum Team
        {
            ONE = 1,
            TWO,
            SPECTATOR
        }
        #endregion
    }
}
