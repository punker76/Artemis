﻿using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services.Storage.Interfaces
{
    public interface IProfileService : IArtemisService
    {
        Profile CreateProfile(ProfileModule module, string name);
        List<Profile> GetProfiles(ProfileModule module);
        Profile GetActiveProfile(ProfileModule module);
        void UpdateProfile(Profile profile, bool includeChildren);
        void DeleteProfile(Profile profile);

        /// <summary>
        ///     Activates the profile for the given <see cref="ProfileModule" /> with the currently active surface
        /// </summary>
        /// <param name="module">The module to activate the profile for</param>
        /// <param name="profile">The profile to activate</param>
        void ActivateProfile(ProfileModule module, Profile profile);

        /// <summary>
        ///     Attempts to restore the profile to the state it had before the last <see cref="UpdateProfile" /> call.
        /// </summary>
        /// <param name="selectedProfile"></param>
        /// <param name="module"></param>
        void UndoUpdateProfile(Profile selectedProfile, ProfileModule module);

        /// <summary>
        ///     Attempts to restore the profile to the state it had before the last <see cref="UndoUpdateProfile" /> call.
        /// </summary>
        /// <param name="selectedProfile"></param>
        /// <param name="module"></param>
        void RedoUpdateProfile(Profile selectedProfile, ProfileModule module);
    }
}