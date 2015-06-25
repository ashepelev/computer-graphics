using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;

namespace CG_Task4
{
    public class MeshObj
    {
        public Material[] meshMaterials;
        public Texture[] meshTextures;
        public Mesh mesh;
        public int scalex;
        public int scaley;
        public int scalez;
        public float radius;

        public float distanceZ;
        public float distanceX;
        public float distanceY;

        public MeshObj(Material[] m, Texture[] t, Mesh me)
        {
            this.meshMaterials = m;
            this.meshTextures = t;
            this.mesh = me;
        }

        public MeshObj() { }

        public MeshObj(MeshObj m)
        {
            this.meshMaterials = m.meshMaterials;
            this.meshTextures = m.meshTextures;
            this.mesh = m.mesh;
            this.radius = m.radius;
            this.distanceZ = m.distanceZ;
            this.distanceX = m.distanceX;
            this.distanceY = m.distanceY;
        }

        public MeshObj(Mesh m)
        {
            this.mesh = m;
        }
    }
}
