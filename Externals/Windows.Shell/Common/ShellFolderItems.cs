﻿//Copyright (c) Microsoft Corporation.  All rights reserved.

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Windows.Internal;

#endregion

namespace Microsoft.Windows.Shell
{
    internal class ShellFolderItems : IEnumerator<ShellObject>
    {
        #region Private Fields

        private IEnumIDList nativeEnumIdList;
        private ShellContainer nativeShellFolder;

        #endregion

        #region Internal Constructor

        internal ShellFolderItems(ShellContainer nativeShellFolder)
        {
            this.nativeShellFolder = nativeShellFolder;

            HRESULT hr = nativeShellFolder.NativeShellFolder.EnumObjects(IntPtr.Zero, ShellNativeMethods.SHCONT.SHCONTF_FOLDERS | ShellNativeMethods.SHCONT.SHCONTF_NONFOLDERS, out nativeEnumIdList);


            if (CoreErrorHelper.Succeeded((int) hr))
                return;
            if (hr == HRESULT.E_ERROR_CANCELLED)
                throw new FileNotFoundException();
            Marshal.ThrowExceptionForHR((int) hr);
        }

        #endregion

        #region IEnumerator<ShellObject> Members

        public ShellObject Current { get; private set; }

        public void Dispose()
        {
            if (nativeEnumIdList != null)
            {
                Marshal.ReleaseComObject(nativeEnumIdList);
                nativeEnumIdList = null;
            }
        }

        object IEnumerator.Current { get { return Current; } }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (nativeEnumIdList == null)
                return false;

            IntPtr item;
            uint numItemsReturned;
            const uint itemsRequested = 1;
            HRESULT hr = nativeEnumIdList.Next(itemsRequested, out item, out numItemsReturned);

            if (numItemsReturned < itemsRequested || hr != HRESULT.S_OK)
                return false;

            Current = ShellObjectFactory.Create(item, nativeShellFolder);

            return true;
        }

        /// <summary>
        /// </summary>
        public void Reset()
        {
            if (nativeEnumIdList != null)
                nativeEnumIdList.Reset();
        }

        #endregion
    }
}