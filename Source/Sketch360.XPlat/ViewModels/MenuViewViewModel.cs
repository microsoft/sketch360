// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Interfaces;
using System.ComponentModel;

namespace Sketch360.XPlat
{
    public class MenuViewViewModel : INotifyPropertyChanged
    {
        private ISketchData _sketchData;

        public event PropertyChangedEventHandler PropertyChanged;

        public ISketchData SketchData
        {
            get => _sketchData;
            set
            {
                if (value != _sketchData)
                {
                    _sketchData = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SketchData)));
                }
            }
        }
    }
}
