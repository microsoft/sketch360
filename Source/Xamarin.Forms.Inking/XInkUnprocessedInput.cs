// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xamarin.Forms.Inking.Support;

namespace Xamarin.Forms.Inking
{
    /// <summary>
    /// Xamarin InkUnprocessedInput
    /// </summary>
    public sealed class XInkUnprocessedInput
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XInkUnprocessedInput"/> class
        /// </summary>
        /// <param name="inkPresenter"></param>
        internal XInkUnprocessedInput(XInkPresenter inkPresenter)
        {
            InkPresenter = inkPresenter;
        }

        /// <summary>
        /// Gets the ink presenter
        /// </summary>
        public XInkPresenter InkPresenter { get; }

        /// <summary>
        /// Pointer entered event handler
        /// </summary>
        public event TypedEventHandler<XInkUnprocessedInput, XPointerEventArgs> PointerEntered;

        /// <summary>
        /// Pointer exited event handler
        /// </summary>
        public event TypedEventHandler<XInkUnprocessedInput, XPointerEventArgs> PointerExited;

        /// <summary>
        /// Pointer hovered event handler
        /// </summary>
        public event TypedEventHandler<XInkUnprocessedInput, XPointerEventArgs> PointerHovered;

        /// <summary>
        /// Pointer lost event handler
        /// </summary>
        public event TypedEventHandler<XInkUnprocessedInput, XPointerEventArgs> PointerLost;

        /// <summary>
        /// Pointer moved event handler
        /// </summary>
        public event TypedEventHandler<XInkUnprocessedInput, XPointerEventArgs> PointerMoved;
        /// <summary>
        /// Pointer pressed event handler
        /// </summary>
        public event TypedEventHandler<XInkUnprocessedInput, XPointerEventArgs> PointerPressed;

        /// <summary>
        /// Pointer released event handler
        /// </summary>
        public event TypedEventHandler<XInkUnprocessedInput, XPointerEventArgs> PointerReleased;

        internal void ProcessTouch(string actionType, XPointerEventArgs args)
        {
            switch (actionType)
            {
                case "Cancelled":
                    PointerLost?.Invoke(this, args);
                    break;

                case "Entered":
                    PointerEntered?.Invoke(this, args);
                    break;

                case "Exited":
                    PointerExited?.Invoke(this, args);
                    break;

                case "Moved":
                    PointerMoved?.Invoke(this, args);
                    break;

                case "Pressed":
                    PointerPressed?.Invoke(this, args);
                    break;

                case "Released":
                    PointerReleased?.Invoke(this, args);
                    break;
            }
        }
    }
}
