﻿// Special thanks to Sergio Pedri for the basis of this design

using DiscordAPI.Models;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Quarrel.Models.Bindables.Abstract;
using Quarrel.Services;
using Quarrel.Converters.Discord;

namespace Quarrel.Models.Bindables
{
    public class BindableUser : BindableModelBase<GuildMember>, IEquatable<BindableUser>, IComparable<BindableUser>
    {
        public BindableUser([NotNull] GuildMember model) : base(model) { }

        public string GuildId { get; set; }

        // TODO: Store, as variable, not property
        public List<Role> Roles
        {
            get
            {
                if (Model.Roles == null)
                    return null;

                List<Role> Roles = new List<Role>();
                foreach (var role in Model.Roles)
                {
                    Roles.Add(ServicesManager.Cache.Runtime.TryGetValue<Role>(Quarrel.Helpers.Constants.Cache.Keys.GuildRole, GuildId + role));
                }
                return Roles;
            }
        }

        public Role TopRole
        {
            get
            {
                if (Roles != null)
                {
                    return Roles.OrderByDescending(x => x.Position).FirstOrDefault() ?? Role.Everyone;
                }
                return Role.Everyone;
            }
        }

        public Role TopHoistRole
        {
            get
            {
                if (Presence.Status == "offline")
                {
                    return Role.Offline;
                }

                if (Roles != null)
                {
                    return Roles.Where(x => x.Hoist).OrderByDescending(x => x.Position).FirstOrDefault() ?? Role.Everyone;
                }

                return Role.Everyone;
            }
        }

        public Presence Presence => ServicesManager.Cache.Runtime.TryGetValue<Presence>(Quarrel.Helpers.Constants.Cache.Keys.Presence, Model.User.Id) ?? new Presence() { Status = "offline", User = Model.User };

        #region Display 

        public string DisplayName => Model.Nick ?? Model.User.Username;


        public bool HasNickname => !string.IsNullOrEmpty(Model.Nick);

        public string Note => ServicesManager.Cache.Runtime.TryGetValue<string>(Quarrel.Helpers.Constants.Cache.Keys.Note, Model.User.Id);

        #endregion

        #region Friend Actions



        #endregion

        #region Interfaces

        /// <inheritdoc/>
        public bool Equals(BindableUser other) => Model.User.Id.Equals(other.Model.User.Id);

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is BindableUser other && Equals(other);

        }

        public override int GetHashCode()
        {
            return Model.User?.Id?.GetHashCode() ?? 0;
        }

        /// <inheritdoc/>
        public int CompareTo(BindableUser other)
        {
            return TopHoistRole.CompareTo(other.TopHoistRole);
        }

        #endregion
    }
}
