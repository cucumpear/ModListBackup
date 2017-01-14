﻿using System.Collections.Generic;
using Verse;

namespace ModListBackup.Handlers
{
    /// <summary>
    /// Class for loading mod ids from a save file
    /// </summary>
    internal static class SaveFileHandler
    {
        /// <summary>
        /// Holds a list of modIds when a file is read from
        /// </summary>
        private static List<string> importList;

        /// <summary>
        /// Import mods from a save file into the current game
        /// </summary>
        /// <param name="filename"></param>
        internal static void ImportMods(string filename)
        {
            Read(GenFilePaths.FilePathForSavedGame(filename));

            ModsConfigHandler.SetActiveMods(importList);
        }

        /// <summary>
        /// Expose the modIds
        /// </summary>
        private static void ExposeData()
        {
            Scribe_Collections.LookList<string>(ref importList, "modIds", LookMode.Undefined);
        }

        /// <summary>
        /// Read a save file
        /// </summary>
        /// <param name="filepath">The path to the savefile</param>
        private static void Read(string filepath)
        {
            Scribe.InitLoading(filepath);
            if (Scribe.EnterNode("meta"))
            {
                ExposeData();
                Scribe.ExitNode();
            }
            Scribe.FinalizeLoading();
        }
    }
}
