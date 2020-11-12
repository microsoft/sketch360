// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Xamarin.Forms.Inking.Support
{
    /// <summary>
    /// A typed event handler
    /// </summary>
    /// <typeparam name="TSender">the sender type</typeparam>
    /// <typeparam name="TResult">the result type</typeparam>
    /// <param name="sender">the sender</param>
    /// <param name="args">the arguments</param>
    public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);
}
