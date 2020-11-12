// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// <copyright file="TypedEventArgs.cs" company="Michael S. Scherotter">
// Copyright (c) 2016 Michael S. Scherotter All Rights Reserved
// </copyright>
// <author>Michael S. Scherotter</author>
// <email>synergist@charette.com</email>
// <date>2016-11-29</date>
// <summary>Typed event arguments</summary>

using System;

namespace Xamarin.Forms.Inking.Support
{
    /// <summary>
    ///     Typed event arguments
    /// </summary>
    /// <typeparam name="T">the value type</typeparam>
    public class TypedEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the TypedEventArgs class
        /// </summary>
        /// <param name="value">the value</param>
        public TypedEventArgs(T value)
        {
            Value = value;
        }

        /// <summary>
        ///     Gets the value
        /// </summary>
        public T Value { get; }

        /// <summary>
        ///     Returns the string for the value.
        /// </summary>
        /// <returns>the string for the Value</returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}