﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Exceptions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Module.ProfileEditor.Dialogs;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem
{
    public abstract class TreeItemViewModel : PropertyChangedBase
    {
        private readonly IDialogService _dialogService;
        private readonly IFolderViewModelFactory _folderViewModelFactory;
        private readonly ILayerService _layerService;
        private readonly ILayerViewModelFactory _layerViewModelFactory;
        private readonly IProfileEditorService _profileEditorService;

        protected TreeItemViewModel(TreeItemViewModel parent,
            ProfileElement profileElement,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            ILayerService layerService,
            IFolderViewModelFactory folderViewModelFactory,
            ILayerViewModelFactory layerViewModelFactory)
        {
            _profileEditorService = profileEditorService;
            _dialogService = dialogService;
            _layerService = layerService;
            _folderViewModelFactory = folderViewModelFactory;
            _layerViewModelFactory = layerViewModelFactory;

            Parent = parent;
            ProfileElement = profileElement;

            Children = new BindableCollection<TreeItemViewModel>();
            UpdateProfileElements();
        }

        public TreeItemViewModel Parent { get; set; }
        public ProfileElement ProfileElement { get; set; }

        public abstract bool SupportsChildren { get; }
        public BindableCollection<TreeItemViewModel> Children { get; set; }

        public List<TreeItemViewModel> GetAllChildren()
        {
            var children = new List<TreeItemViewModel>();
            foreach (var childFolder in Children)
            {
                // Add all children in this element
                children.Add(childFolder);
                // Add all children of children inside this element
                children.AddRange(childFolder.GetAllChildren());
            }

            return children;
        }

        public void SetElementInFront(TreeItemViewModel source)
        {
            if (source.Parent != Parent)
            {
                source.Parent.RemoveExistingElement(source);
                Parent.AddExistingElement(source);
            }

            Parent.ProfileElement.RemoveChild(source.ProfileElement);
            Parent.ProfileElement.AddChild(source.ProfileElement, ProfileElement.Order);
            Parent.UpdateProfileElements();
        }

        public void SetElementBehind(TreeItemViewModel source)
        {
            if (source.Parent != Parent)
            {
                source.Parent.RemoveExistingElement(source);
                Parent.AddExistingElement(source);
            }

            Parent.ProfileElement.RemoveChild(source.ProfileElement);
            Parent.ProfileElement.AddChild(source.ProfileElement, ProfileElement.Order + 1);
            Parent.UpdateProfileElements();
        }

        public void RemoveExistingElement(TreeItemViewModel treeItem)
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot remove a child from a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.RemoveChild(treeItem.ProfileElement);
            Children.Remove(treeItem);
            treeItem.Parent = null;
        }

        public void AddExistingElement(TreeItemViewModel treeItem)
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a child to a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.AddChild(treeItem.ProfileElement);
            Children.Add(treeItem);
            treeItem.Parent = this;
        }

        public void AddFolder()
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a folder to a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.AddChild(new Folder(ProfileElement.Profile, ProfileElement, "New folder"));
            UpdateProfileElements();
            _profileEditorService.UpdateSelectedProfile();
        }

        public void AddLayer()
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a layer to a profile element of type " + ProfileElement.GetType().Name);

            var layer = new Layer(ProfileElement.Profile, ProfileElement, "New layer");
            foreach (var baseLayerProperty in layer.Properties)
                _layerService.InstantiateKeyframeEngine(baseLayerProperty);
            ProfileElement.AddChild(layer);
            UpdateProfileElements();
            _profileEditorService.UpdateSelectedProfile();
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public async Task RenameElement()
        {
            var result = await _dialogService.ShowDialog<ProfileElementRenameViewModel>(
                new Dictionary<string, object> {{"profileElement", ProfileElement}}
            );
            if (result is string newName)
            {
                ProfileElement.Name = newName;
                _profileEditorService.UpdateSelectedProfile();
            }
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public async Task DeleteElement()
        {
            var result = await _dialogService.ShowConfirmDialog(
                "Delete profile element",
                "Are you sure you want to delete this element? This cannot be undone."
            );

            if (!result)
                return;

            // Farewell, cruel world
            var parent = Parent;
            ProfileElement.Parent.RemoveChild(ProfileElement);
            parent.RemoveExistingElement(this);
            parent.UpdateProfileElements();

            _profileEditorService.UpdateSelectedProfile();
        }

        public void UpdateProfileElements()
        {
            // Order the children
            var vmsList = Children.OrderBy(v => v.ProfileElement.Order).ToList();
            for (var index = 0; index < vmsList.Count; index++)
            {
                var profileElementViewModel = vmsList[index];
                Children.Move(Children.IndexOf(profileElementViewModel), index);
            }

            // Ensure every child element has an up-to-date VM
            if (ProfileElement.Children != null)
            {
                foreach (var profileElement in ProfileElement.Children.OrderBy(c => c.Order))
                {
                    TreeItemViewModel existing = null;
                    if (profileElement is Folder folder)
                    {
                        existing = Children.FirstOrDefault(p => p is FolderViewModel vm && vm.ProfileElement == folder);
                        if (existing == null)
                            Children.Add(_folderViewModelFactory.Create((FolderViewModel) this, folder));
                    }
                    else if (profileElement is Layer layer)
                    {
                        existing = Children.FirstOrDefault(p => p is LayerViewModel vm && vm.ProfileElement == layer);
                        if (existing == null)
                            Children.Add(_layerViewModelFactory.Create((FolderViewModel) this, layer));
                    }

                    existing?.UpdateProfileElements();
                }
            }
        }
    }
}