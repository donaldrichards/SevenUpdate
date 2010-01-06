﻿#region GNU Public License v3

// Copyright 2007-2010 Robert Baker, aka Seven ALive.
// This file is part of Seven Update.
//  
//     Seven Update is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     Seven Update is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//  
//    You should have received a copy of the GNU General Public License
//    along with Seven Update.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace SevenUpdate.Sdk.Pages
{
    public sealed partial class UpdateMenu : UserControl
    {
        #region Global Vars

        /// <summary>
        /// Index of the current update
        /// </summary>
        internal int Index = -1;

        #endregion

        public UpdateMenu()
        {
            Font = SystemFonts.MessageBoxFont;

            InitializeComponent();
        }

        #region Methods

        /// <summary>
        /// Updates the UI with the updates created.
        /// </summary>
        internal void UpdateUI()
        {
            lbUpdates.Items.Clear();

            if (SDK.Application.Updates != null)
            {
                if (SDK.Application.Updates.Count > 0)
                {
                    lbUpdates.Visible = true;

                    label1.Visible = true;

                    clEditUpdate.Enabled = true;

                    for (var x = 0; x < SDK.Application.Updates.Count; x++)
                    {
                        if (SDK.Application.Updates[x].Name.Count <= 0)
                            continue;
                        if (SDK.Application.Updates[x].Name[0].Value != null)
                            lbUpdates.Items.Add(SDK.Application.Updates[x].Name[0].Value);
                        else
                            lbUpdates.Items.Add(Program.RM.GetString("Update"));
                    }
                    if (lbUpdates != null)
                    {
                        if (lbUpdates.Items.Count > 0)
                            lbUpdates.SelectedIndex = 0;
                    }
                }
                else
                {
                    removeUpdateToolStripMenuItem.Enabled = false;

                    clEditUpdate.Enabled = false;
                }
            }
            else
            {
                removeUpdateToolStripMenuItem.Enabled = false;

                lbUpdates.Visible = false;

                label1.Visible = false;

                clEditUpdate.Enabled = false;
            }
        }

        #endregion

        #region UI Events

        private void lbUpdates_SelectedIndexChanged(object sender, EventArgs e)
        {
            Index = lbUpdates.SelectedIndex;

            removeUpdateToolStripMenuItem.Enabled = Index >= 0;
        }

        private void removeUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SDK.RemoveUpdate(lbUpdates.SelectedIndex);

            UpdateUI();

            if (Index >= 0)
                return;
            removeUpdateToolStripMenuItem.Enabled = false;
            clEditUpdate.Enabled = false;
        }

        #endregion
    }
}