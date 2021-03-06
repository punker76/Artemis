﻿using System;

namespace Artemis.Storage.Entities
{
    public class PluginSettingEntity
    {
        public Guid Id { get; set; }
        public Guid PluginGuid { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}