﻿using System;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Events
{
    public class PluginEventArgs : EventArgs
    {
        public PluginEventArgs()
        {
        }

        public PluginEventArgs(PluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo;
        }

        public PluginInfo PluginInfo { get; }
    }
}