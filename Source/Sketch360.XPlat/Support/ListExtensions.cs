// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sketch360.XPlat.Support
{
    /// <summary>
    ///     List extensions
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        ///     Add a range of items to a collection
        /// </summary>
        /// <typeparam name="T">the type of item in the list</typeparam>
        /// <param name="list">the destination list</param>
        /// <param name="itemsToAdd">the items to add</param>
        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> itemsToAdd)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (itemsToAdd == null) return;

            foreach (var item in itemsToAdd) list.Add(item);
        }

        /// <summary>
        ///     Set the list to a range of items
        /// </summary>
        /// <typeparam name="T">the type the list contains</typeparam>
        /// <param name="list">the destination list</param>
        /// <param name="itemsToAdd">the source list</param>
        public static void SetRange<T>(this ICollection<T> list, IEnumerable<T> itemsToAdd)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            list.Clear();

            list.AddRange(itemsToAdd);
        }

        /// <summary>
        ///     Insert a range into the list at a certain index
        /// </summary>
        /// <typeparam name="T">the type of item the list contains</typeparam>
        /// <param name="list">the destination list</param>
        /// <param name="index">the index to insert at</param>
        /// <param name="itemsToAdd">the items to add</param>
        public static void InsertRange<T>(this Collection<T> list, int index, IEnumerable<T> itemsToAdd)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (itemsToAdd == null) return;

            foreach (var item in itemsToAdd)
            {
                list.Insert(index, item);

                index++;
            }
        }
    }
}