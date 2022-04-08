﻿// Adam Dernis © 2022

using Microsoft.Extensions.DependencyInjection;
using Quarrel.Bindables.Guilds;
using Quarrel.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Quarrel.Controls.Shell.Panels
{
    public sealed partial class GuildPanel : UserControl
    {
        public GuildPanel()
        {
            this.InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<GuildsViewModel>();
        }

        public GuildsViewModel ViewModel => (GuildsViewModel)DataContext;

        private void GuildList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is BindableGuild guild)
            {
                ViewModel.NavigateToGuild(guild);
            }
        }

        private void GuildList_OnItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var container = (TreeViewItem)sender.ContainerFromItem(args.InvokedItem);
            if (args.InvokedItem is BindableGuild guild)
            {
                ViewModel.NavigateToGuild(guild);
            }
            else
            {
                args.Handled = true;
            }
        }
    }
}
