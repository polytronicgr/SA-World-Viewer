﻿using System.Collections.Generic;

using GTAMapViewer.Graphics;

namespace GTAMapViewer.World
{
    internal class Interior : Cell
    {
        private List<Instance> myInstances;

        protected override void OnFinalisePlacements( ICollection<InstPlacement> placements )
        {
            myInstances = new List<Instance>();
            foreach ( InstPlacement p in placements )
                myInstances.Add( new Instance( p ) );
        }

        public override ICollection<Instance> GetInstances()
        {
            return myInstances;
        }

        public override void Render( ModelShader shader )
        {
            foreach ( Instance inst in myInstances )
                inst.Render( shader );
        }
    }
}
