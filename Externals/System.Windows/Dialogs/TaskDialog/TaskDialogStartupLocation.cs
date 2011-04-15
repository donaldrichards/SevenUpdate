// ***********************************************************************
// <copyright file="TaskDialogStartupLocation.cs"
//            project="System.Windows"
//            assembly="System.Windows"
//            solution="SevenUpdate"
//            company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// <license href="http://code.msdn.microsoft.com/WindowsAPICodePack/Project/License.aspx">Microsoft Software License</license>
// ***********************************************************************

namespace System.Windows.Dialogs
{
    /// <summary>Specifies the initial display location for a task dialog.</summary>
    public enum TaskDialogStartupLocation
    {
        /// <summary>The window placed in the center of the screen.</summary>
        CenterScreen,

        /// <summary>The window centered relative to the window that launched the dialog.</summary>
        CenterOwner
    }
}