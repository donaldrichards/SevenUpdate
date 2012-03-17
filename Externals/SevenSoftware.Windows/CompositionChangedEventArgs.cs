// ***********************************************************************
// <copyright file="CompositionChangedEventArgs.cs" project="SevenSoftware.Windows" assembly="SevenSoftware.Windows" solution="SevenUpdate" company="Seven Software">
//     Copyright (c) Seven Software. All rights reserved.
// </copyright>
// <author username="sevenalive">Robert Baker</author>
// <license href="http://www.gnu.org/licenses/gpl-3.0.txt" name="GNU General Public License 3">
//  This file is part of Seven Update.
//    Seven Update is free software: you can redistribute it and/or modify it under the terms of the GNU General Public
//    License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any
//    later version. Seven Update is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
//    even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details. You should have received a copy of the GNU General Public License
//    along with Seven Update.  If not, see http://www.gnu.org/licenses/.
// </license>
// ***********************************************************************

namespace SevenSoftware.Windows
{
    using System;

    /// <summary>Event argument for The CompositionChanged event.</summary>
    public class CompositionChangedEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="CompositionChangedEventArgs" /> class.</summary>
        /// <param name="isGlassEnabled">If set to <c>True</c> aero glass is enabled.</param>
        internal CompositionChangedEventArgs(bool isGlassEnabled)
        {
            this.IsGlassEnabled = isGlassEnabled;
        }

        #endregion

        #region Public Properties

        /// <summary>Gets a value indicating whether DWM/Glass is currently enabled.</summary>
        /// <value><c>True</c> if this instance is glass enabled; otherwise, <c>False</c>.</value>
        public bool IsGlassEnabled { get; private set; }

        #endregion
    }
}