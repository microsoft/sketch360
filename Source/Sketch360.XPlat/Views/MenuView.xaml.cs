// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Sketch360.XPlat.Commands;
using Sketch360.XPlat.Pages;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Sketch360.XPlat.Interfaces;

namespace Sketch360.XPlat.Views
{
    public partial class MenuView : ContentView
    {
        readonly MenuViewViewModel _viewModel = new MenuViewViewModel();
        
        private Page _page;

        public MenuView()
        {
            InitializeComponent();

            BindingContext = _viewModel;

            SettingsMenuItem.Command = new PushModalPageCommand<SettingsPage>();
            AboutSketchMenuItem.Command = new PushModalPageCommand<AboutPage>();
            SketchPropertiesMenuItem.Command = new PushModalPageCommand<DrawingPropertiesPage>();
            HelpMenuItem.Command = new PushModalPageCommand<HelpPage>();

        }

        public Page Page { get => _page;
            set
            {
                if (Resources.TryGetValue("ExportImageCommand", out object commandValue))
                {
                    if (commandValue is IExportImageCommand command)
                    {
                        command.Page = value;
                    }
                }

                _page = value;

                SaveSketchCommand.Page = Page;
            }
        }


        public IExportImageCommand SaveSketchCommand { get => SaveSketchMenuItem.Command as IExportImageCommand; set => SaveSketchMenuItem.Command = value; }

        public ICommand NewSketchCommand { get => NewSketchMenuItem.Command; set => NewSketchMenuItem.Command = value; }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is MenuItem item)
            {
                if (item.Command != null && item.Command.CanExecute(item.CommandParameter))
                {
                    item.Command.Execute(item.CommandParameter);

                }
            }

            MenuListView.SelectedItem = null;

            IsVisible = false;
        }
    }
}