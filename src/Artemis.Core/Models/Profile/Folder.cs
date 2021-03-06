using System;
using System.Linq;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public sealed class Folder : ProfileElement
    {
        public Folder(Profile profile, ProfileElement parent, string name)
        {
            FolderEntity = new FolderEntity();
            EntityId = Guid.NewGuid();

            Profile = profile;
            Parent = parent;
            Name = name;
        }

        internal Folder(Profile profile, ProfileElement parent, FolderEntity folderEntity)
        {
            FolderEntity = folderEntity;
            EntityId = folderEntity.Id;

            Profile = profile;
            Parent = parent;
            Name = folderEntity.Name;
            Order = folderEntity.Order;

            // TODO: Load conditions

            // Load child folders
            foreach (var childFolder in Profile.ProfileEntity.Folders.Where(f => f.ParentId == EntityId))
                _children.Add(new Folder(profile, this, childFolder));
            // Load child layers
            foreach (var childLayer in Profile.ProfileEntity.Layers.Where(f => f.ParentId == EntityId))
                _children.Add(new Layer(profile, this, childLayer));

            // Ensure order integrity, should be unnecessary but no one is perfect specially me
            _children = _children.OrderBy(c => c.Order).ToList();
            for (var index = 0; index < _children.Count; index++)
            {
                var profileElement = _children[index];
                profileElement.Order = index + 1;
            }
        }

        internal FolderEntity FolderEntity { get; set; }

        public override void Update(double deltaTime)
        {
            // Folders don't update but their children do
            foreach (var profileElement in Children)
                profileElement.Update(deltaTime);
        }

        public override void Render(double deltaTime, SKCanvas canvas)
        {
            // Folders don't render but their children do
            foreach (var profileElement in Children)
                profileElement.Render(deltaTime, canvas);
        }

        public Folder AddFolder(string name)
        {
            var folder = new Folder(Profile, this, name) {Order = Children.LastOrDefault()?.Order ?? 1};
            AddChild(folder);
            return folder;
        }

        public Layer AddLayer(string name)
        {
            var layer = new Layer(Profile, this, name) {Order = Children.LastOrDefault()?.Order ?? 1};
            layer.LayerShape = new Fill(layer);
            AddChild(layer);
            return layer;
        }

        internal override void ApplyToEntity()
        {
            FolderEntity.Id = EntityId;
            FolderEntity.ParentId = Parent?.EntityId ?? new Guid();

            FolderEntity.Order = Order;
            FolderEntity.Name = Name;

            FolderEntity.ProfileId = Profile.EntityId;

            // TODO: conditions
        }

        public override string ToString()
        {
            return $"[Folder] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
        }
    }
}