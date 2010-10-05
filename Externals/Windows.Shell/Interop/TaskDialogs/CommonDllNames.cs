//***********************************************************************
// Assembly         : Windows.Shell
// Author           : sevenalive
// Created          : 09-17-2010
// Last Modified By : sevenalive
// Last Modified On : 10-05-2010
// Description      : 
// Copyright        : (c) Seven Software. All rights reserved.
//***********************************************************************

namespace Microsoft.Windows.Internal
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class to hold string references to common interop DLLs.
    /// </summary>
    public static class CommonDllNames
    {
        #region Constants and Fields

        /// <summary>
        ///   Comctl32.DLL
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ctl")]
        public const string ComCtl32 = "comctl32.dll";

        /// <summary>
        ///   Comdlg32.dll
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dlg")]
        public const string ComDlg32 = "comdlg32.dll";

        /// <summary>
        ///   Kernel32.dll
        /// </summary>
        public const string Kernel32 = "kernel32.dll";

        /// <summary>
        ///   Shell32.dll
        /// </summary>
        public const string Shell32 = "shell32.dll";

        /// <summary>
        ///   User32.dll
        /// </summary>
        public const string User32 = "user32.dll";

        #endregion
    }
}