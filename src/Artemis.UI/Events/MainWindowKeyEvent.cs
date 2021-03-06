﻿using System.Windows.Input;

namespace Artemis.UI.Events
{
    public class MainWindowKeyEvent
    {
        public MainWindowKeyEvent(bool keyDown, KeyEventArgs eventArgs)
        {
            KeyDown = keyDown;
            EventArgs = eventArgs;
        }

        public bool KeyDown { get; }
        public KeyEventArgs EventArgs { get; }
    }
}