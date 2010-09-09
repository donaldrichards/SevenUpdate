#region GNU Public License Version 3

// Copyright 2007-2010 Robert Baker, Seven Software.
// This file is part of Seven Update.
//   
//      Seven Update is free software: you can redistribute it and/or modify
//      it under the terms of the GNU General Public License as published by
//      the Free Software Foundation, either version 3 of the License, or
//      (at your option) any later version.
//  
//      Seven Update is distributed in the hope that it will be useful,
//      but WITHOUT ANY WARRANTY; without even the implied warranty of
//      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//      GNU General Public License for more details.
//   
//      You should have received a copy of the GNU General Public License
//      along with Seven Update.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using File = System.IO.File;

#endregion

namespace SevenUpdate
{
    /// <summary>
    ///   Class containing methods to install updates
    /// </summary>
    public static class Install
    {
        #region Fields

        /// <summary>
        ///   Gets an int that indicates to move a file on reboot
        /// </summary>
        private const int MoveOnReboot = 5;

        /// <summary>
        ///   The localized name of the current update being installed
        /// </summary>
        private static string currentUpdateName;

        /// <summary>
        ///   The index position of the current update being installed
        /// </summary>
        private static int updateIndex;

        /// <summary>
        ///   The total number of updates being installed
        /// </summary>
        private static int updateCount;

        private static bool errorOccurred;

        #endregion

        #region Events

        /// <summary>
        ///   Occurs when the installation completed.
        /// </summary>
        public static event EventHandler<InstallCompletedEventArgs> InstallCompleted;

        /// <summary>
        ///   Occurs when the installation progress changed
        /// </summary>
        public static event EventHandler<InstallProgressChangedEventArgs> InstallProgressChanged;

        #endregion

        /// <summary>
        ///   Moves or deletes a file on reboot
        /// </summary>
        /// <param name = "lpExistingFileName">The source filename</param>
        /// <param name = "lpNewFileName">The destination filename</param>
        /// <param name = "dwFlags">A int indicating the move operation to perform</param>
        /// <returns><c>true</c> if successful, otherwise <c>false</c></returns>
        [DllImport("kernel32.dll")]
        private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);

        private static void ReportProgress(int installProgress)
        {
            if (InstallProgressChanged != null)
                InstallProgressChanged(null, new InstallProgressChangedEventArgs(currentUpdateName, installProgress, updateIndex, updateCount));
        }

        #region Update Installation

        /// <summary>
        ///   Installs updates
        /// </summary>
        public static void InstallUpdates(Collection<Sui> apps)
        {
            if (apps == null)
                return;

            if (apps.Count < 1)
                return;

            #region variables

            updateCount = apps.Sum(t => t.Updates.Count);
            int completedUpdates = 0, failedUpdates = 0;

            #endregion

            ReportProgress(0);

            for (var x = 0; x < apps.Count; x++)
            {
                for (var y = 0; y < apps[x].Updates.Count; y++)
                {
                    errorOccurred = false;
                    if (File.Exists(Base.AllUserStore + @"abort.lock"))
                    {
                        File.Delete(Base.AllUserStore + @"abort.lock");
                        return;
                    }

                    currentUpdateName = Base.GetLocaleString(apps[x].Updates[y].Name);
                    if (apps[x].AppInfo.Directory == Base.ConvertPath(@"%PROGRAMFILES%\Seven Software\Seven Update", true, apps[x].AppInfo.Is64Bit))
                    {
                        try
                        {
                            Process.GetProcessesByName(@"SevenUpdate.Helper")[0].Kill();
                        }
                        catch
                        {
                        }
                    }

                    ReportProgress(5);

                    #region Registry

                    SetRegistryItems(apps[x].Updates[y].RegistryItems);

                    #endregion

                    ReportProgress(25);

                    #region Files

                    UpdateFiles(apps[x].Updates[y].Files, Base.AllUserStore + @"downloads\" + currentUpdateName + @"\");

                    #endregion

                    ReportProgress(75);

                    #region Shortcuts

                    SetShortcuts(apps[x].Updates[y].Shortcuts, apps[x].AppInfo.Directory, apps[x].AppInfo.Is64Bit);

                    #endregion

                    ReportProgress(95);

                    if (errorOccurred)
                    {
                        failedUpdates++;
                        AddHistory(apps[x], apps[x].Updates[y]);
                    }
                    else
                    {
                        completedUpdates++;
                        AddHistory(apps[x], apps[x].Updates[y]);
                    }

                    #region If Seven Update

                    if (apps[x].AppInfo.Directory == Base.ConvertPath(@"%PROGRAMFILES%\Seven Software\Seven Update", true, true) && Base.RebootNeeded)
                    {
                        foreach (var t in apps[x].Updates[y].Files)
                        {
                            switch (t.Action)
                            {
                                case FileAction.Delete:
                                case FileAction.UnregisterThenDelete:
                                    try
                                    {
                                        File.Delete(t.Destination);
                                    }
                                    catch
                                    {
                                        MoveFileEx(t.Destination, null, MoveOnReboot);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        Base.StartProcess(Base.AppDir + "SevenUpdate.Helper.exe", "\"" + currentUpdateName + "\"");

                        return;
                    }

                    #endregion

                    ReportProgress(100);

                    updateIndex++;
                }
            }

            ReportProgress(100);

            if (Base.RebootNeeded)
            {
                MoveFileEx(Base.AllUserStore + "reboot.lock", null, MoveOnReboot);

                if (Directory.Exists(Base.AllUserStore + "downloads"))
                    MoveFileEx(Base.AllUserStore + "downloads", null, MoveOnReboot);
            }
            else
            {
                // Delete the downloads directory if no errors were found and no reboot is needed
                if (!errorOccurred)
                {
                    if (Directory.Exists(Base.AllUserStore + "downloads"))
                    {
                        try
                        {
                            Directory.Delete(Base.AllUserStore + "downloads", true);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (InstallCompleted != null)
                InstallCompleted(null, new InstallCompletedEventArgs(completedUpdates, failedUpdates));

            return;
        }

        /// <summary>
        ///   Sets the registry items of an update
        /// </summary>
        /// <param name = "regItems">The registry changes to install on the system</param>
        /// <returns>a bool value indicating if an error occurred</returns>
        private static void SetRegistryItems(IList<RegistryItem> regItems)
        {
            RegistryKey key;
            if (regItems == null)
                return;

            for (var x = 0; x < regItems.Count; x++)
            {
                switch (regItems[x].Hive)
                {
                    case RegistryHive.ClassesRoot:
                        key = Registry.ClassesRoot;
                        break;
                    case RegistryHive.CurrentUser:
                        key = Registry.CurrentUser;
                        break;
                    case RegistryHive.LocalMachine:
                        key = Registry.LocalMachine;
                        break;
                    case RegistryHive.Users:
                        key = Registry.Users;
                        break;
                    default:
                        key = Registry.CurrentUser;
                        break;
                }
                switch (regItems[x].Action)
                {
                    case RegistryAction.Add:
                        try
                        {
                            Registry.SetValue(regItems[x].Key, regItems[x].KeyValue, regItems[x].Data, regItems[x].ValueKind);
                        }
                        catch (Exception e)
                        {
                            Base.ReportError(e, Base.AllUserStore);
                            errorOccurred = true;
                        }
                        break;
                    case RegistryAction.DeleteKey:
                        try
                        {
                            if (key != null)
// ReSharper disable PossibleNullReferenceException
                                key.OpenSubKey(regItems[x].Key, true).DeleteSubKeyTree(regItems[x].Key);
// ReSharper restore PossibleNullReferenceException
                        }
                        catch (Exception e)
                        {
                            Base.ReportError(e, Base.AllUserStore);
                        }
                        break;
                    case RegistryAction.DeleteValue:
                        try
                        {
                            // ReSharper disable PossibleNullReferenceException
                            key.OpenSubKey(regItems[x].Key, true).DeleteValue(regItems[x].KeyValue, false);
                            // ReSharper restore PossibleNullReferenceException
                        }
                        catch (Exception e)
                        {
                            Base.ReportError(e, Base.AllUserStore);
                        }
                        break;
                }

                #region Report Progress

                var installProgress = (x*100)/regItems.Count;
                if (installProgress > 30)
                    installProgress -= 10;

                ReportProgress(installProgress);

                #endregion
            }
        }

        /// <summary>
        ///   Installs the shortcuts of an update
        /// </summary>
        /// <param name = "shortcuts">the shortcuts to install on the system</param>
        /// <param name = "appDirectory">the directory where the application is installed</param>
        /// <param name = "is64Bit"><c>true</c> if the application is 64 bit, otherwise <c>false</c></param>
        private static void SetShortcuts(IList<Shortcut> shortcuts, string appDirectory, bool is64Bit)
        {
            if (shortcuts == null)
                return;

            var ws = new WshShell();
            // Choose the path for the shortcut
            for (var x = 0; x < shortcuts.Count; x++)
            {
                try
                {
                    var linkLocation = Base.ConvertPath(shortcuts[x].Location, appDirectory, is64Bit);
                    var linkName = Base.GetLocaleString(shortcuts[x].Name);

                    if (!linkLocation.EndsWith(@"\"))
                        linkLocation = linkLocation + @"\";

                    if (shortcuts[x].Action == ShortcutAction.Add || (shortcuts[x].Action == ShortcutAction.Update && File.Exists(linkLocation)))
                    {
                        // ReSharper disable AssignNullToNotNullAttribute
                        if (!Directory.Exists(linkLocation))
                            Directory.CreateDirectory(linkLocation);
                        File.Delete(linkLocation + linkName);
                        // ReSharper restore AssignNullToNotNullAttribute
                        var shortcut = (IWshShortcut) ws.CreateShortcut(linkLocation + linkName);
                        // Where the shortcut should point to
                        shortcut.TargetPath = Base.ConvertPath(shortcuts[x].Target, appDirectory, is64Bit);
                        // Description for the shortcut
                        shortcut.Description = Base.GetLocaleString(shortcuts[x].Description);
                        // Location for the shortcut's icon
                        shortcut.IconLocation = Base.ConvertPath(shortcuts[x].Icon, appDirectory, is64Bit);
                        // The arguments to be used for the shortcut
                        shortcut.Arguments = shortcuts[x].Arguments;
                        // The working directory to be used for the shortcut
                        shortcut.WorkingDirectory = appDirectory;
                        // Create the shortcut at the given path
                        shortcut.Save();
                    }

                    if (shortcuts[x].Action == ShortcutAction.Delete)
                        File.Delete(linkLocation);
                }
                catch (Exception e)
                {
                    Base.ReportError(e, Base.AllUserStore);
                }

                #region Report Progress

                var installProgress = (x*100)/shortcuts.Count;
                if (installProgress > 90)
                    installProgress -= 15;

                ReportProgress(installProgress);

                #endregion
            }
        }

        private static void UpdateFile(UpdateFile file)
        {
            switch (file.Action)
            {
                    #region Delete file

                case FileAction.ExecuteThenDelete:
                case FileAction.UnregisterThenDelete:
                case FileAction.Delete:
                    if (file.Action == FileAction.ExecuteThenDelete)
                    {
                        if (File.Exists(file.Source))
                            Base.StartProcess(file.Source, file.Args, true);
                    }

                    if (file.Action == FileAction.UnregisterThenDelete)
                        Base.StartProcess("regsvr32", "/u /s" + file.Destination);

                    try
                    {
                        File.Delete(file.Destination);
                    }
                    catch
                    {
                        MoveFileEx(file.Destination, null, MoveOnReboot);
                    }

                    break;

                    #endregion

                case FileAction.Execute:
                    try
                    {
                        Base.StartProcess(file.Destination, file.Args);
                    }
                    catch (Exception e)
                    {
                        Base.ReportError(e + file.Source, Base.AllUserStore);
                        errorOccurred = true;
                    }
                    break;

                    #region Update file

                case FileAction.Update:
                case FileAction.UpdateIfExist:
                case FileAction.UpdateThenExecute:
                case FileAction.UpdateThenRegister:
                    if (File.Exists(file.Source))
                    {
                        try
                        {
                            if (File.Exists(file.Destination))
                            {
                                File.Copy(file.Destination, file.Destination + ".bak", true);
                                File.Delete(file.Destination);
                            }
                            File.Move(file.Source, file.Destination);

                            if (File.Exists(file.Destination + ".bak"))
                                File.Delete(file.Destination + ".bak");
                        }
                        catch
                        {
                            if (!File.Exists(Base.AllUserStore + "reboot.lock"))
                                File.Create(Base.AllUserStore + "reboot.lock").WriteByte(0);

                            MoveFileEx(file.Source, file.Destination, MoveOnReboot);
                            File.Delete(file.Destination + ".bak");
                        }
                    }
                    else
                    {
                        Base.ReportError("FileNotFound: " + file.Source, Base.AllUserStore);
                        errorOccurred = true;
                    }

                    if (file.Action == FileAction.UpdateThenExecute)
                    {
                        try
                        {
                            Base.StartProcess(file.Destination, file.Args);
                        }
                        catch (Exception e)
                        {
                            Base.ReportError(e + file.Source, Base.AllUserStore);
                            errorOccurred = true;
                        }
                    }

                    if (file.Action == FileAction.UpdateThenRegister)
                        Base.StartProcess("regsvr32", "/s" + file.Destination);
                    break;

                    #endregion
            }
        }

        /// <summary>
        ///   Installs the files in the update
        /// </summary>
        /// <param name = "files">the collection of files to update</param>
        /// <param name = "downloadDirectory">the path to the download folder where the update files are located</param>
        /// <returns><c>true</c> if updated all files without errors, otherwise <c>false</c></returns>
        private static void UpdateFiles(IList<UpdateFile> files, string downloadDirectory)
        {
            for (var x = 0; x < files.Count; x++)
            {
                files[x].Source = downloadDirectory + Path.GetFileName(files[x].Destination);
                try
                {
// ReSharper disable AssignNullToNotNullAttribute
                    Directory.CreateDirectory(Path.GetDirectoryName(files[x].Destination));
// ReSharper restore AssignNullToNotNullAttribute
                }
                catch (Exception e)
                {
                    Base.ReportError(e, Base.AllUserStore);
                    errorOccurred = true;
                }


                var x1 = x;
                var x2 = x;
                Task.Factory.StartNew(() => UpdateFile(files[x1])).ContinueWith(delegate
                                                                                    {
                                                                                        #region Report Progress

                                                                                        var installProgress = (x2*100)/files.Count;
                                                                                        if (installProgress > 70)
                                                                                            installProgress -= 15;

                                                                                        ReportProgress(installProgress);
                                                                                    });

                #endregion
            }
        }

        #endregion

        #region Update History

        /// <summary>
        ///   Adds an update to the update history
        /// </summary>
        /// <param name = "updateInfo">the update information</param>
        /// <param name = "failed"><c>true</c> if the update failed, otherwise <c>false</c></param>
        /// <param name = "appInfo">the application information</param>
        private static void AddHistory(Sui appInfo, Update updateInfo, bool failed = false)
        {
            var history = Base.Deserialize<Collection<Suh>>(Base.HistoryFile) ?? new Collection<Suh>();
            var hist = new Suh
                           {
                               HelpUrl = appInfo.AppInfo.HelpUrl,
                               Publisher = appInfo.AppInfo.Publisher,
                               PublisherUrl = appInfo.AppInfo.AppUrl,
                               Description = updateInfo.Description,
                               Status = failed == false ? UpdateStatus.Successful : UpdateStatus.Failed,
                               InfoUrl = updateInfo.InfoUrl,
                               InstallDate = DateTime.Now.ToShortDateString(),
                               ReleaseDate = updateInfo.ReleaseDate,
                               Importance = updateInfo.Importance,
                               Name = updateInfo.Name
                           };


            history.Add(hist);

            Base.Serialize(history, Base.HistoryFile);
        }

        #endregion
    }
}