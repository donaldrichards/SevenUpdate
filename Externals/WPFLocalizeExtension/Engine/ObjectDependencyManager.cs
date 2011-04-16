// ***********************************************************************
// <copyright file="ObjectDependencyManager.cs" project="WPFLocalizeExtension" assembly="WPFLocalizeExtension" solution="SevenUpdate" company="Bernhard Millauer">
//     Copyright (c) Bernhard Millauer. All rights reserved.
// </copyright>
// <author username="SeriousM">Bernhard Millauer</author>
// <license href="http://wpflocalizeextension.codeplex.com/license">Microsoft Public License</license>
// ***********************************************************************

namespace WPFLocalizeExtension.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>This class ensures, that a specific object lives as long a associated object is alive.</summary>
    public static class ObjectDependencyManager
    {
        #region Constants and Fields

        /// <summary>This member holds the list of all <see cref="WeakReference" />s and their appropriate objects.</summary>
        private static readonly Dictionary<object, List<WeakReference>> InternalList = new Dictionary<object, List<WeakReference>>();

        #endregion

        #region Public Methods

        /// <summary>This method adds a new object dependency.</summary>
        /// <param name="weakRef">The <see cref="WeakReference" />, which ensures the live cycle of <paramref name="value" />.</param>
        /// <param name="value">The object, which should stay alive as long <paramref name="weakRef" /> is alive.</param>
        /// <returns><see langword="true" />, if the binding was successfully, otherwise <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool AddObjectDependency(WeakReference weakRef, object value)
        {
            // run the clean up to ensure that only objects are watched they are really still alive
            CleanUp();

            if (weakRef == null)
            {
                throw new ArgumentNullException("weakRef");
            }

            // if the objToHold is null, we cannot handle this afterwards.
            if (value == null)
            {
                throw new ArgumentNullException("value", "The objToHold cannot be null");
            }

            // if the objToHold is a weak reference, we cannot handle this type afterwards.
            if (value.GetType() == typeof(WeakReference))
            {
                throw new ArgumentException("value cannot be type of WeakReference", "value");
            }

            // if the target of the weak reference is the objToHold, this would be a cycling play.
            if (weakRef.Target == value)
            {
                throw new InvalidOperationException("The WeakReference.Target cannot be the same as value");
            }

            // holds the status of registration of the object dependency
            var itemRegistered = false;

            // check if the objToHold is contained in the internalList.
            if (!InternalList.ContainsKey(value))
            {
                // add the objToHold to the internal list.
                var lst = new List<WeakReference> { weakRef };

                InternalList.Add(value, lst);

                itemRegistered = true;
            }
            else
            {
                // otherwise, check if the weakRefDp exists and add it if necessary
                var lst = InternalList[value];
                if (!lst.Contains(weakRef))
                {
                    lst.Add(weakRef);

                    itemRegistered = true;
                }
            }

            // return the status of the registration
            return itemRegistered;
        }

        /// <summary>This method cleans up all independent (!<see cref="WeakReference" />.IsAlive) objects or a single object.</summary>
        /// <param name="value">If defined, the associated object dependency will be removed instead of a full CleanUp.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void CleanUp(object value = null)
        {
            // if a particular object is passed, remove it.
            if (value != null)
            {
                // if the key wasn't found, throw an exception.
                if (!InternalList.Remove(value))
                {
                    throw new ArgumentException("Key was not found!");
                }

                // stop here
                return;
            }

            // perform an full clean up

            // this list will hold all keys they has to be removed
            var keysToRemove = new List<object>();

            // step through all object dependencies
            foreach (var kvp in InternalList)
            {
                // step recursive through all weak references
                for (var i = kvp.Value.Count - 1; i >= 0; i--)
                {
                    // if this weak reference is no more alive, remove it
                    if (!kvp.Value[i].IsAlive)
                    {
                        kvp.Value.RemoveAt(i);
                    }
                }

                // if the list of weak references is empty, remove the whole entry
                if (kvp.Value.Count == 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            // step recursive through all keys that have to be remove
            for (var i = keysToRemove.Count - 1; i >= 0; i--)
            {
                // remove the key from the internalList
                InternalList.Remove(keysToRemove[i]);
            }

            // clear up the keysToRemove
            keysToRemove.Clear();
        }

        #endregion
    }
}