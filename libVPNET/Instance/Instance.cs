﻿using System;
using VP.Native;

namespace VP
{
    /// <summary>
    /// Represents a Virtual Paradise bot that can interact with a universe and world,
    /// complete with all applicable methods and events provided by the SDK.
    /// 
    /// Also contains helper methods and properties, such as the current
    /// <see cref="Position"/> of the instance.
    /// </summary>
    public partial class Instance : IDisposable
    {
        #region Member containers
        /// <summary>
        /// Methods, events and properties related to user or world list data
        /// </summary>
        public readonly DataContainer Data;
        /// <summary>
        /// Methods, events and properties related to users and avatars
        /// </summary>
        public readonly AvatarsContainer Avatars;
        /// <summary>
        /// Methods, events and properties related to property and object handling,
        /// including queries
        /// </summary>
        public readonly PropertyContainer Property;
        /// <summary>
        /// Methods, events and properties related to terrain modificaton and queries
        /// </summary>
        public readonly TerrainContainer Terrain;
        #endregion

        #region Constructor, setup & deconstructor
        internal bool   Disposed;
        internal IntPtr Pointer;

        object mutex = new object();
        internal object Mutex
        {
            get
            {
                if (Disposed)
                    throw new ObjectDisposedException( this.ToString() );
                else
                    return mutex;
            }
        }


        /// <summary>
        /// Creates a bot instance, initializing the SDK automatically
        /// </summary>
        public Instance()
        {
            Pointer = SDK.CreateInstance();

            this.Data     = new DataContainer(this);
            this.Avatars  = new AvatarsContainer(this);
            this.Property = new PropertyContainer(this);
            this.Terrain  = new TerrainContainer(this);
            setupEvents();
        }

        /// <summary>
        /// Automatically disposes this instance on finalize
        /// </summary>
        ~Instance()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes of the bot by destorying it natively, then disposes of all
        /// containers.
        /// </summary>
        public void Dispose()
        {
            lock (Mutex)
            {
                Disposed = true;

                if (Pointer != IntPtr.Zero)
                    Functions.Call( () => Functions.vp_destroy(Pointer) );

                Data.Dispose();
                Avatars.Dispose();
                Property.Dispose();
                Terrain.Dispose();
                disposeEvents();
                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
