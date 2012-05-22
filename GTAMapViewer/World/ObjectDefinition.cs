﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GTAMapViewer.Resource;
using GTAMapViewer.Graphics;

namespace GTAMapViewer.World
{
    internal class ObjectDefinition
    {
        public readonly String ModelName;
        public readonly String TextureDictName;

        public Model Model
        {
            get;
            private set;
        }

        public bool Loaded
        {
            get;
            private set;
        }

        public RenderLayer RenderLayer
        {
            get
            {
                return HasFlags( ObjectFlag.Alpha2 ) ?
                    RenderLayer.Alpha2 : HasFlags( ObjectFlag.Alpha1 ) ?
                    RenderLayer.Alpha1 : Graphics.RenderLayer.Base;
            }
        }

        public readonly float DrawDist;
        public readonly float DrawDist2;
        public readonly ObjectFlag Flags;

        public ObjectDefinition( String model, String txd, float drawDist, ObjectFlag flags )
        {
            ModelName = model;
            TextureDictName = txd;
            DrawDist = drawDist;
            DrawDist2 = DrawDist * DrawDist;
            Flags = flags;

            Loaded = false;
        }

        public bool HasFlags( ObjectFlag flag )
        {
            return ( Flags & flag ) == flag;
        }

        public void Load()
        {
            Model = ResourceManager.LoadModel( ModelName, TextureDictName );
            Loaded = true;
        }

        public void Unload()
        {
            ResourceManager.UnloadModel( ModelName, TextureDictName );
            Model = null;
            Loaded = false;
        }
    }
}
