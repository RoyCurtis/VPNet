﻿using System;
using System.Collections.Generic;
using VP.Native;

namespace VP
{
    public partial class Instance : IDisposable
    {
        #region Native events and callbacks
        Dictionary<Events, EventDelegate>       nativeEvents    = new Dictionary<Events, EventDelegate>();
        Dictionary<Callbacks, CallbackDelegate> nativeCallbacks = new Dictionary<Callbacks, CallbackDelegate>();

        internal void setNativeEvent(Events eventType, EventDelegate eventFunction)
        {
            nativeEvents[eventType] = eventFunction;
            Functions.vp_event_set(pointer, (int) eventType, eventFunction);
        }

        internal void setNativeCallback(Callbacks callbackType, CallbackDelegate callbackFunction)
        {
            nativeCallbacks[callbackType] = callbackFunction;
            Functions.vp_callback_set(pointer, (int) callbackType, callbackFunction);
        }

        void setupEvents()
        {
            setNativeEvent(Events.UniverseDisconnect, OnUniverseDisconnect);
            setNativeEvent(Events.WorldDisconnect, OnWorldDisconnect);
            setNativeEvent(Events.Chat, OnChat);
        }

        void disposeEvents()
        {
            UniverseDisconnect = null;
            WorldDisconnect    = null;
            Chat               = null;
        }
        #endregion

        #region Events
        /// <summary>
        /// Encapsulates a general method that accepts an <see cref="Instance"/> for most
        /// events
        /// </summary>
        public delegate void Event(Instance sender);
        /// <summary>
        /// Encapsulates a general method that accepts an <see cref="Instance"/> and a
        /// <see cref="ReasonCode"/> for most callbacks
        /// </summary>
        public delegate void Callback(Instance sender, ReasonCode result);
        /// <summary>
        /// Encapsulates a method that accepts a source <see cref="Instance"/> and a
        /// <see cref="ChatMessage"/> for the <see cref="Chat"/> event
        /// </summary>
        public delegate void ChatEvent(Instance sender, ChatMessage chat);
        /// <summary>
        /// Encapsulates a method that accepts a source <see cref="Instance"/> and a
        /// <see cref="VP.ConsoleMessage"/> for the <see cref="Console"/> event
        /// </summary>
        public delegate void ConsoleEvent(Instance sender, ConsoleMessage console);

        /// <summary>
        /// Fired when the SDK has been unexpectedly disconnected from the universe
        /// </summary>
        /// <remarks>
        /// Universe connections are independant of world connections. This will not
        /// cause <see cref="WorldDisconnect"/> to fire also.
        /// </remarks>
        public event Event UniverseDisconnect;
        /// <summary>
        /// Fired when the SDK has been unexpectedly disconnected from the world
        /// </summary>
        public event Event WorldDisconnect;
        /// <summary>
        /// Fired when a chat message has been said in the world, providing the message
        /// and source
        /// </summary>
        public event ChatEvent Chat;
        /// <summary>
        /// Fired when a console message has been sent to this instance, providing the
        /// message, its formatting and source
        /// </summary>
        public event ConsoleEvent Console; 
        #endregion

        #region Event handlers
        internal void OnUniverseDisconnect(IntPtr sender)
        {
            if (UniverseDisconnect != null)
                UniverseDisconnect(this);
        }

        internal void OnWorldDisconnect(IntPtr sender)
        {
            if (WorldDisconnect != null)
                WorldDisconnect(this);
        }

        internal void OnChat(IntPtr sender)
        {
            if (Chat == null && Console == null)
                return;

            var type = (ChatType) Functions.vp_int(pointer, IntAttributes.ChatType);

            if      (type == ChatType.Normal && Chat != null)
                Chat(this, new ChatMessage(sender) );
            else if (type != ChatType.Normal && Console != null)
                Console(this, new ConsoleMessage(sender) );
        }
        #endregion
    }
}
