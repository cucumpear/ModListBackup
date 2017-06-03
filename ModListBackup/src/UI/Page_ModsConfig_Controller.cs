﻿using ExtraWidgets;
using ModListBackup.Handlers;
using ModListBackup.Handlers.Settings;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ModListBackup.UI {

    /// <summary>
    /// Class to Handle our code injection
    /// </summary>
    internal static class Page_ModsConfig_Controller {
        public const int STATUS_DELAY_TICKS_LONG = 500;
        public const int STATUS_DELAY_TICKS_SHORT = 250;

        private static float BottomHeight = 40f;
        private static float BottomLeftContentOffset = (350f - BottomLeftContentWidth) / 2;
        private static float BottomLeftContentWidth = 330;
        private static float BottomRightContentOffset = 109f;
        private static float ButtonBigWidth = 110f;
        private static float ButtonSmallWidth = 50f;
        private static float LabelStatusHeight = 18f;
        private static float LabelStatusWidth = 100f;
        private static float LabelWidth = 20f;
        private static float Padding = 5f;
        /// <summary>
        /// Holds the currently selected save state, default to 1
        /// </summary>
        private static int selectedState = 1;

        /// <summary>
        /// Enum for easily setting status message delay
        /// </summary>
        internal enum Status_Delay { longDelay, shortDelay }

        /// <summary>
        /// Holds the status message to display
        /// </summary>
        private static string StatusMessage { get; set; }

        /// <summary>
        /// Holds how long the status message should stay visible in ticks
        /// </summary>
        private static int StatusMessageDelay { get; set; }

        /// <summary>
        /// HugsLib window injection that loads AFTER the Page_ModsConfig DoWindowContents method is called
        /// </summary>
        /// <param name="window">Page_ModsConfig's Window</param>
        /// <param name="rect">Page_ModConfig's Rect</param>
        internal static void DoWindowContents(Rect inRect) {
            DoBottomLeftWindowContents(inRect);
            DoBottomRightWindowContents(inRect);
        }

        /// <summary>
        /// Sets a status message to be displayed
        /// </summary>
        /// <param name="message">The messge to display</param>
        /// <param name="delay">How long the message should stay visable (Default:longDelay)</param>
        internal static void SetStatus(string message, Status_Delay delay = Status_Delay.longDelay) {
            StatusMessage = message;
            if (delay == Status_Delay.longDelay)
                StatusMessageDelay = STATUS_DELAY_TICKS_LONG;
            else
                StatusMessageDelay = STATUS_DELAY_TICKS_SHORT;
        }

        /// <summary>
        /// Calls the ModHandler SaveState function
        /// </summary>
        private static void BackupModList() {
            ModsConfigHandler.SaveState(selectedState);
            SetStatus("Status_Message_Backup".Translate());
        }

        /// <summary>
        /// Clears the status message
        /// </summary>
        private static void ClearStatus() {
            StatusMessage = "";
            StatusMessageDelay = -1;
        }

        /// <summary>
        /// Fills the bottom left corner of the window
        /// </summary>
        /// <param name="rect">The rect to draw into</param>
        private static void DoBottomLeftWindowContents(Rect rect) {
            // Backup Button
            Rect BackupRect = new Rect((rect.xMin - 1) + BottomLeftContentOffset, rect.yMax - 37f, ButtonBigWidth, BottomHeight);
            TooltipHandler.TipRegion(BackupRect, "Button_Backup_Tooltip".Translate());
            if (Widgets.ButtonText(BackupRect, "Button_Backup_Text".Translate()))
                BackupModList();

            // '>>' Label
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect toRect = new Rect(BackupRect.xMax + Padding, BackupRect.y, LabelWidth, BottomHeight);
            Widgets.Label(toRect, ">>");

            // State button and Float menu
            Rect StateRect = new Rect(toRect.xMax + Padding, BackupRect.y, ButtonSmallWidth, BottomHeight);
            TooltipHandler.TipRegion(StateRect, "Button_State_Select_Tooltip".Translate());
            if (Widgets.ButtonText(StateRect, string.Format("{0}{1}", selectedState.ToString(), (ModsConfigHandler.StateIsSet(selectedState)) ? null : "*"))) {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                for (int i = 1; i <= SettingsHandler.STATE_LIMIT; i++) {
                    //set a new variable here, otherwise the selected state and button text will change when int i next iterates
                    int n = i;
                    options.Add(new FloatMenuOption(GetStateName(i), (Action)(() => { selectedState = n; }), MenuOptionPriority.Default, (Action)null, (Thing)null, 0.0f, (Func<Rect, bool>)null, (WorldObject)null));
                }

                options.Add(new FloatMenuOption("Edit Names...", (Action)(() => { Find.WindowStack.Add(new Dialogs.Dialog_EditNames()); }), MenuOptionPriority.Default, (Action)null, (Thing)null, 0f, null, null));

                Find.WindowStack.Add((Window)new FloatMenu(options));
            }

            // '<<' Label
            Rect fromRect = new Rect(StateRect.xMax + Padding, StateRect.y, LabelWidth, BottomHeight);
            Widgets.Label(fromRect, "<<");

            // Restore Button
            Rect RestoreRect = new Rect(fromRect.xMax + Padding, StateRect.y, ButtonBigWidth, BottomHeight);
            TooltipHandler.TipRegion(RestoreRect, "Button_Restore_Tooltip".Translate());
            if (Widgets.ButtonText(RestoreRect, "Button_Restore_Text".Translate()))
                RestoreModList();

            // Undo Button
            Rect UndoRect = new Rect(RestoreRect.xMax + Padding, RestoreRect.y, ButtonSmallWidth, BottomHeight);
            TooltipHandler.TipRegion(UndoRect, "Button_Undo_Tooltip".Translate());
            if (ButtonWidgets.ButtonImage(UndoRect, Textures.Undo))
                if (ModsConfigHandler.CanUndo)
                    if (ModsConfigHandler.DoUndoAction())
                        SetStatus("Status_Message_Undone".Translate());

            // Status Label
            UpdateStatus();
            Text.Font = GameFont.Tiny;
            Rect StatusRect = new Rect(StateRect.x - 25f, StateRect.yMax - 58f, LabelStatusWidth, LabelStatusHeight);
            Widgets.Label(StatusRect, StatusMessage);

            //Reset text
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// Fills the bottom right corner of the window
        /// </summary>
        /// <param name="rect">The rect to draw into</param>
        private static void DoBottomRightWindowContents(Rect rect) {
            // Import Button
            Rect ImportRect = new Rect(rect.xMax - BottomRightContentOffset, rect.yMax - 37f, ButtonBigWidth, BottomHeight);
            TooltipHandler.TipRegion(ImportRect, "Button_Import_Tooltip".Translate());
            if (Widgets.ButtonText(ImportRect, "Button_Import_Text".Translate())) {
                Dialogs.Dialog_Import importWindow = new Dialogs.Dialog_Import();
                Find.WindowStack.Add(importWindow);
            }
        }

        /// <summary>
        /// Gets the formatted name of a state
        /// </summary>
        /// <param name="state">The state to get</param>
        /// <returns>The name of the state</returns>
        private static string GetStateName(int state) {
            return String.Format("{0}. {1}{2}", state, ModsConfigHandler.GetStateNamePretty(state), (ModsConfigHandler.StateIsSet(state)) ? null : " [*]");
        }
        /// <summary>
        /// Calls the ModHandler LoadState function
        /// </summary>
        private static void RestoreModList() {
            if (!ModsConfigHandler.StateIsSet(selectedState)) {
                Main.Log.Message("state not set");
                SetStatus("Status_Message_Null".Translate());
            }
            else {
                ModsConfigHandler.LoadState(selectedState);
                SetStatus("Status_Message_Restore".Translate());
            }
        }
        /// <summary>
        /// Updates status message delay ticks
        /// </summary>
        private static void UpdateStatus() {
            if (StatusMessageDelay > 0)
                StatusMessageDelay--;
            if (StatusMessageDelay == 0)
                ClearStatus();
        }
    }
}