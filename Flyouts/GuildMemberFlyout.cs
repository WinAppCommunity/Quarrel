﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Discord_UWP.Managers;
using Discord_UWP.LocalModels;
using Discord_UWP.SharedModels;

namespace Discord_UWP.Flyouts
{
    partial class FlyoutCreator
    {
        public static MenuFlyout MakeGuildMemberMenu(GuildMember member)
        {

            MenuFlyout menu = new MenuFlyout();
            menu.MenuFlyoutPresenterStyle = (Style)App.Current.Resources["MenuFlyoutPresenterStyle1"];
            MenuFlyoutItem profile = new MenuFlyoutItem()
            {
                Text = App.GetString("/Flyouts/Profile"),
                //Text = "Profile",
                Tag = member.User,
                Icon = new SymbolIcon(Symbol.ContactInfo)
            };
            profile.Click += FlyoutManager.OpenProfile;
            menu.Items.Add(profile);
            MenuFlyoutItem message = new MenuFlyoutItem()
            {
                Text = App.GetString("/Flyouts/Message"),
                //Text = "Message",
                Tag = member.User.Id,
                Icon = new SymbolIcon(Symbol.Message)
            };
            message.Click += FlyoutManager.MessageUser;
            menu.Items.Add(message);
            MenuFlyoutSeparator sep1 = new MenuFlyoutSeparator();
            menu.Items.Add(sep1);
            if (member.User.Id != LocalState.CurrentUser.Id)
            {
                MenuFlyoutSubItem InviteToServer = new MenuFlyoutSubItem()
                {
                    Text = App.GetString("/Flyouts/InviteToServer")
                    //Text = "Invite To Server"
                    //Tag = member.Raw.User.Id,
                    //Icon = new SymbolIcon(Symbol.)
                };
                foreach (KeyValuePair<string, LocalModels.Guild> guild in LocalState.Guilds)
                {
                    if (guild.Value.permissions.Administrator || guild.Value.permissions.CreateInstantInvite)
                    {
                        MenuFlyoutItem item = new MenuFlyoutItem() { Text = guild.Value.Raw.Name, Tag = new Tuple<string, string>(guild.Value.channels.FirstOrDefault().Value.raw.Id, member.User.Id) };
                        item.Click += FlyoutManager.InviteToServer;
                        InviteToServer.Items.Add(item);
                    }

                }
                menu.Items.Add(InviteToServer);
            }
            MenuFlyoutItem addFriend = new MenuFlyoutItem()
            {
                Text = App.GetString("/Flyouts/AddFriend"),
                //Text = "Add Friend",
                Tag = member.User.Id,
                Icon = new SymbolIcon(Symbol.AddFriend)
            };
            addFriend.Click += FlyoutManager.AddFriend;
            MenuFlyoutItem removeFriend = new MenuFlyoutItem()
            {
                Text = App.GetString("/Flyouts/RemoveFriend"),
                //Text = "Remove Friend",
                Tag = member.User.Id,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 71, 71)),
                Icon = new SymbolIcon(Symbol.ContactPresence)
            };
            removeFriend.Click += FlyoutManager.RemoveFriend;
            MenuFlyoutItem block = new MenuFlyoutItem()
            {
                Text = App.GetString("/Flyouts/Block"),
                //Text = "Block",
                Tag = member.User.Id,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 71, 71)),
                Icon = new SymbolIcon(Symbol.BlockContact)
            };
            block.Click += FlyoutManager.BlockUser;
            MenuFlyoutItem unBlock = new MenuFlyoutItem()
            {
                Text = App.GetString("/Flyouts/Unblock"),
                //Text = "Unblock",
                Tag = member.User.Id,
                Icon = new SymbolIcon(Symbol.ContactPresence)
            };
            unBlock.Click += FlyoutManager.RemoveFriend;
            MenuFlyoutItem acceptFriendRequest = new MenuFlyoutItem()
            {
                Text = App.GetString("/Flyouts/AcceptFriendRequest"),
                //Text = "Accept Friend Request",
                Tag = member.User.Id,
                Icon = new SymbolIcon(Symbol.AddFriend)
            };
            acceptFriendRequest.Click += FlyoutManager.AddFriend;
            if (LocalState.Friends.ContainsKey(member.User.Id))
            {
                switch (LocalState.Friends[member.User.Id].Type)
                {
                    case 1:
                        menu.Items.Add(removeFriend);
                        menu.Items.Add(block);
                        break;
                    case 2:
                        menu.Items.Add(unBlock);
                        break;
                    case 3:
                        menu.Items.Add(acceptFriendRequest);
                        menu.Items.Add(block);
                        break;
                    case 4:
                        menu.Items.Add(block);
                        break;
                }
            }
            else if (member.User.Id == LocalState.CurrentUser.Id)
            {
                //None
            }
            else
            {
                menu.Items.Add(addFriend);
                menu.Items.Add(block);
            }
            if (member.User.Id != LocalState.CurrentUser.Id)
            {
                MenuFlyoutSeparator sep2 = new MenuFlyoutSeparator();
                menu.Items.Add(sep2);
            }
            if ((member.User.Id == LocalState.CurrentUser.Id && LocalState.Guilds[App.CurrentGuildId].permissions.ChangeNickname) || (LocalState.Guilds[App.CurrentGuildId].permissions.ManageNicknames || LocalState.Guilds[App.CurrentGuildId].permissions.Administrator && LocalState.Guilds[App.CurrentGuildId].GetHighestRole(member.Roles).Position <= LocalState.Guilds[App.CurrentGuildId].GetHighestRole(LocalState.Guilds[App.CurrentGuildId].members[LocalState.CurrentUser.Id].Roles).Position) || LocalState.Guilds[App.CurrentGuildId].Raw.OwnerId == LocalState.CurrentUser.Id)
            {
                MenuFlyoutItem changeNickname = new MenuFlyoutItem()
                {
                    Text = App.GetString("/Flyouts/ChangeNickname"),
                    //Text = "Change Nickname",
                    Tag = member.User.Id,
                    Icon = new SymbolIcon(Symbol.Edit)
                };
                changeNickname.Click += FlyoutManager.ChangeNickname;
                menu.Items.Add(changeNickname);
            }
            if (LocalState.Guilds[App.CurrentGuildId].permissions.Administrator || LocalState.Guilds[App.CurrentGuildId].permissions.ManageRoles)
            {
                MenuFlyoutSubItem roles = new MenuFlyoutSubItem()
                {
                    Text = App.GetString("/Flyouts/Roles")
                    //Text = "Roles"
                    //Tag = member.Raw.User.Id,
                    //Icon = new SymbolIcon(Symbol.)
                };

                foreach (SharedModels.Role role in LocalState.Guilds[App.CurrentGuildId].roles.Values.OrderByDescending(x => x.Position))
                {
                    ToggleMenuFlyoutItem roleItem = new ToggleMenuFlyoutItem()
                    {
                        Text = role.Name,
                        Tag = new Tuple<string, string>(role.Id, member.User.Id),
                        Foreground = Common.IntToColor(role.Color),
                        IsChecked = LocalState.Guilds[App.CurrentGuildId].members[member.User.Id].Roles.Contains(role.Id),
                        //Style = (Style)App.Current.Resources["ToggleOnlyCheckbox"],
                        IsEnabled = (role.Position < LocalState.Guilds[App.CurrentGuildId].roles[LocalState.Guilds[App.CurrentGuildId].members[LocalState.CurrentUser.Id].Roles.First()].Position || LocalState.Guilds[App.CurrentGuildId].Raw.OwnerId == LocalState.CurrentUser.Id)  //TODO: Double check role system
                    };
                    roleItem.Click += FlyoutManager.AddRole;
                    if (role.Name != "@everyone")
                    {
                        roles.Items.Add(roleItem);
                    }
                }
                menu.Items.Add(roles);
            }
            if ((LocalState.Guilds[App.CurrentGuildId].permissions.Administrator || LocalState.Guilds[App.CurrentGuildId].permissions.KickMembers) && LocalState.Guilds[App.CurrentGuildId].GetHighestRole(member.Roles).Position < LocalState.Guilds[App.CurrentGuildId].GetHighestRole(LocalState.Guilds[App.CurrentGuildId].members[LocalState.CurrentUser.Id].Roles).Position || LocalState.Guilds[App.CurrentGuildId].Raw.OwnerId == LocalState.CurrentUser.Id && member.User.Id != LocalState.CurrentUser.Id)
            {
                MenuFlyoutItem kickMember = new MenuFlyoutItem()
                {
                    Text = App.GetString("/Flyouts/KickMember"),
                    //Text = "Kick Member",
                    Tag = member.User.Id,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 71, 71)),
                    Icon = new SymbolIcon(Symbol.BlockContact)
                };
                kickMember.Click += FlyoutManager.KickMember;
                menu.Items.Add(kickMember);
            }
            else if (member.User.Id == LocalState.CurrentUser.Id && LocalState.Guilds[App.CurrentGuildId].Raw.OwnerId != LocalState.CurrentUser.Id)
            {
                MenuFlyoutItem leaveServer = new MenuFlyoutItem()
                {
                    Text = App.GetString("/Flyouts/LeaveServer"),
                    //Text = "Leave Server",
                    Tag = member.User.Id,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 71, 71)),
                    Icon = new SymbolIcon(Symbol.Remove)
                };
                leaveServer.Click += FlyoutManager.LeaveServer;
                menu.Items.Add(leaveServer);
            }
            if (((LocalState.Guilds[App.CurrentGuildId].permissions.Administrator || LocalState.Guilds[App.CurrentGuildId].permissions.BanMembers) && LocalState.Guilds[App.CurrentGuildId].GetHighestRole(member.Roles).Position < LocalState.Guilds[App.CurrentGuildId].GetHighestRole(LocalState.Guilds[App.CurrentGuildId].members[LocalState.CurrentUser.Id].Roles).Position) || LocalState.Guilds[App.CurrentGuildId].Raw.OwnerId == LocalState.CurrentUser.Id && member.User.Id != LocalState.CurrentUser.Id)
            {
                MenuFlyoutItem banMember = new MenuFlyoutItem()
                {
                    Text = "Ban Member", //TODO: Translate
                    Tag = member.User.Id,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 71, 71)),
                    Icon = new SymbolIcon(Symbol.BlockContact)
                };
                banMember.Click += FlyoutManager.BanMember;
                menu.Items.Add(banMember);
            }
            if (false)
            {
                //TODO: style ToggleMenuFlyoutItem to have a checkbox on the right side
                ToggleMenuFlyoutItem mute = new ToggleMenuFlyoutItem()
                {
                    Text = App.GetString("/Flyouts/Mute"),
                    //Text = "Mute",
                    Icon = new SymbolIcon(Symbol.Mute)
                };


                ToggleMenuFlyoutItem deafen = new ToggleMenuFlyoutItem()
                {
                    Text = App.GetString("/Flyouts/Deafen"),
                    //Text = "Deafen",
                    Icon = new SymbolIcon(Symbol.Mute)
                };

                MenuFlyoutSubItem moveChannel = new MenuFlyoutSubItem()
                {
                    Text = App.GetString("/Flyouts/MoveChannel")
                    //Text = "Move Channel"
                };
            }
            return menu;
        }
    }
}
