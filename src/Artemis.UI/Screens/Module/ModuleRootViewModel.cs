﻿using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Module
{
    public class ModuleRootViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly IProfileEditorViewModelFactory _profileEditorViewModelFactory;

        public ModuleRootViewModel(Core.Plugins.Abstract.Module module, IProfileEditorViewModelFactory profileEditorViewModelFactory)
        {
            DisplayName = module?.DisplayName;
            Module = module;

            _profileEditorViewModelFactory = profileEditorViewModelFactory;

            Task.Run(AddTabsAsync);
        }

        public Core.Plugins.Abstract.Module Module { get; }

        private async Task AddTabsAsync()
        {
            // Give the screen a moment to active without freezing the UI thread
            await Task.Delay(400);

            // Create the profile editor and module VMs
            if (Module is ProfileModule profileModule)
            {
                var profileEditor = _profileEditorViewModelFactory.Create(profileModule);
                Items.Add(profileEditor);
            }

            var moduleViewModels = Module.GetViewModels();
            Items.AddRange(moduleViewModels);

            ActiveItem = Items.FirstOrDefault();
        }
    }
}