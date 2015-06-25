using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;

namespace CG_Task4
{
    public partial class Form1 : Form
    {
        private Microsoft.DirectX.Direct3D.Device device = null;
        public Microsoft.DirectX.DirectInput.Device keyb;
        public Texture road;
        public Texture grass;
        public Texture horizon;
        public VertexBuffer vb = null;
        public VertexBuffer vbhor = null;
        public float speed = 20.0f;
        public float angle = 0.0f;
        public float mouseX = 0.0f;
        public float carsTimer = 0.0f;
        public float farestCar = 15000.0f;
        public List<float> roadCoords = new List<float>();

        public MeshObj car = new MeshObj();
        public MeshObj house = new MeshObj();
        public MeshObj car1 = new MeshObj();
        public MeshObj car2 = new MeshObj();
        public MeshObj car3 = new MeshObj();
        public MeshObj obstacle1 = new MeshObj();
        public MeshObj obstacle2 = new MeshObj();
        public MeshObj obstacle3 = new MeshObj();
        public MeshObj obstacle4 = new MeshObj();
        public List<MeshObj> obstacles = new List<MeshObj>();
        public Mesh mesh = null;
        public float collisionTimer = 0.0f;
        public bool collision = false;
        public int collisionObstPos = 1;

        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
        }

         public void InitializeKeyboard()
        {
            keyb = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
            keyb.SetCooperativeLevel(this, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            keyb.Acquire();
        }

        public void InitializeGraphics()
        {
            PresentParameters presentParams = new PresentParameters();
            presentParams.Windowed = true; presentParams.SwapEffect = SwapEffect.Discard;
      //      presentParams.MultiSample = MultiSampleType.FourSamples;
      //      presentParams.MultiSampleQuality = 4;

            presentParams.EnableAutoDepthStencil = true;
            presentParams.AutoDepthStencilFormat = DepthFormat.D16;
            device = new Microsoft.DirectX.Direct3D.Device(0, Microsoft.DirectX.Direct3D.DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            device.VertexFormat = CustomVertex.PositionNormalTextured.Format;                      

            vb = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), 36, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionNormalTextured.Format, Pool.Default);
            vb.Created += new EventHandler(this.OnVertexBufferCreate);
            OnVertexBufferCreate(vb, null);
            vbhor = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), 6, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionNormalTextured.Format, Pool.Default);
            OnHorizonVertexBufferCreate(vbhor, null);
            for (int i = 0; i < 50; i++)
                roadCoords.Add(700.0f - 512.0f * i);

            InitializeKeyboard();

            road = new Texture(device, new Bitmap(this.GetType(), @"road2.jpg"), 0, Pool.Managed);
            grass = new Texture(device, new Bitmap(this.GetType(), @"GrassSample.jpg"), 0, Pool.Managed);
            horizon = new Texture(device, new Bitmap(this.GetType(), @"horizon2.bmp"), 0, Pool.Managed);
            LoadMesh(@"..\..\Mesh\cot_dodge.x", car);
            LoadMesh(@"..\..\Mesh\cot_chevy.x", car1);
            LoadMesh(@"..\..\Mesh\cot_toyota.x", car2);
            obstacle1.mesh = Mesh.Box(device, 200, 200, 200);
            obstacle2.mesh = Mesh.Cylinder(device, 100, 100, 200, 30, 30);
            obstacle3.mesh = Mesh.Sphere(device, 100, 30, 30);
            obstacle4.mesh = Mesh.Torus(device, 25, 100, 30, 30);

            car.distanceX = 0.0f;
            device.SamplerState[0].MinFilter = TextureFilter.Anisotropic;
            device.SamplerState[0].MagFilter = TextureFilter.Anisotropic;
            device.SamplerState[0].MaxAnisotropy = 16;
            device.SamplerState[0].SrgbTexture = false;
        }

        private void OnVertexBufferCreate(object sender, EventArgs e)
        {
            VertexBuffer buffer = (VertexBuffer)sender;
            CustomVertex.PositionNormalTextured[] verts_triangle = new CustomVertex.PositionNormalTextured[6];

            verts_triangle[0] = new CustomVertex.PositionNormalTextured(512.0f, 0.0f, 512.0f, 0.0f, 4000.0f, 0.0f, 0.0f, 1.0f);
            verts_triangle[1] = new CustomVertex.PositionNormalTextured(512.0f, 0.0f, -512.0f, 0.0f, 4000.0f, 0.0f, 0.0f, 0.0f);
            verts_triangle[2] = new CustomVertex.PositionNormalTextured(-512.0f, 0.0f, -512.0f, 0.0f, 4000.0f, 0.0f, 1.0f, 0.0f);

            verts_triangle[3] = new CustomVertex.PositionNormalTextured(-512.0f, 0.0f, -512.0f, 0.0f, 4000.0f, 0.0f, 1.0f, 0.0f);
            verts_triangle[4] = new CustomVertex.PositionNormalTextured(-512.0f, 0.0f, 512.0f, 0.0f, 4000.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[5] = new CustomVertex.PositionNormalTextured(512.0f, 0.0f, 512.0f, 0.0f, 4000.0f, 0.0f, 0.0f, 1.0f);
            
            buffer.SetData(verts_triangle, 0, LockFlags.None);
        }

        private void OnHorizonVertexBufferCreate(object sender, EventArgs e)
        {
            VertexBuffer buffer = (VertexBuffer)sender;
            CustomVertex.PositionNormalTextured[] verts_triangle = new CustomVertex.PositionNormalTextured[6];
            verts_triangle[0] = new CustomVertex.PositionNormalTextured(12800.0f, 0.0f, 8368.0f, 0.0f, 4000.0f, 0.0f, 0.0f, 1.0f);
            verts_triangle[1] = new CustomVertex.PositionNormalTextured(12800.0f, 0.0f, -8368.0f, 0.0f, 4000.0f, 0.0f, 0.0f, 0.0f);
            verts_triangle[2] = new CustomVertex.PositionNormalTextured(-12800.0f, 0.0f, -8368.0f, 0.0f, 4000.0f, 0.0f, 1.0f, 0.0f);

            verts_triangle[3] = new CustomVertex.PositionNormalTextured(-12800.0f, 0.0f, -8368.0f, 0.0f, 4000.0f, 0.0f, 1.0f, 0.0f);
            verts_triangle[4] = new CustomVertex.PositionNormalTextured(-12800.0f, 0.0f, 8368.0f, 0.0f, 4000.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[5] = new CustomVertex.PositionNormalTextured(12800.0f, 0.0f, 8368.0f, 0.0f, 4000.0f, 0.0f, 0.0f, 1.0f);
            buffer.SetData(verts_triangle, 0, LockFlags.None);
        }

        private void DrawBox(float yaw, float pitch, float roll, float x, float y, float z, Texture t)
        {
            device.Transform.World = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Matrix.Translation(x, y, z);
            device.SetTexture(0, t);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);
            SetupCamera();
            
          //  dRoad += 5.0f;
            if (speed < 100.0f)
                speed += 0.1f;
            if (collision)
            {
                collisionTimer -= 0.1f;
                if (collisionTimer > 0.0f)
                    angle += 0.01f * collisionObstPos;
                else if (collisionTimer < 0.0f)
                    angle -= 0.01f * collisionObstPos;
                if (collisionTimer <= -10.0f)
                {
                    angle = 0.0f;
                    collision = false;
                    collisionTimer = 0.0f;
                }
            }
            mouseX = 0.0f;
            ReadKeyboard();
            if (car.distanceX + mouseX < 100.0f && car.distanceX + mouseX > -100.0f)
                car.distanceX += mouseX*(60.0f/speed);
            
            carsTimer += 0.1f;
            if (carsTimer >= 20.0f)
            {
                Random rnd = new Random();
                int rndObstacle = rnd.Next(1, 3);
                MeshObj newObstacle = new MeshObj();
                switch (rndObstacle)
                {
                    case 1: { newObstacle = new MeshObj(car1); break; }
                    case 2: { newObstacle = new MeshObj(car2); break; }
                    default: break;
                }
                int zpos = rnd.Next(0, 11);
                newObstacle.distanceZ = -5000.0f -zpos * 256.0f;
                int pos = rnd.Next(0, 2);
                newObstacle.distanceX = 80.0f - 160.0f * pos;
                newObstacle.distanceY = 30.0f;
                obstacles.Add(newObstacle);
                carsTimer = 0.0f;
            }
            obstacles.RemoveAll(removeObst);
            foreach (MeshObj c in obstacles)
            {
                c.distanceZ += 5.0f * ((speed - 20.0f)/20.0f);
                DrawCarMesh(0, -(float)Math.PI / 2, 0, c.distanceX, 30.0f, c.distanceZ, c);
                if (Math.Abs(c.distanceZ+300.0f) < c.radius + car.radius)
                    if (Math.Abs(c.distanceX - car.distanceX) < 80.0f)
                    {
                        collision = true;
                        speed /= 2.0f;
                        collisionTimer = 10.0f;
                        if (c.distanceX > 0)
                        {
                            c.distanceX = 200.0f;
                            collisionObstPos = -1;
                        }
                        else
                        {
                            c.distanceX = -200.0f;
                            collisionObstPos = 1;
                        }
                    }
                    
            }
           
            device.BeginScene();
            device.SetStreamSource(0, vbhor, 0);
            DrawBox(0, (float)Math.PI / 2, 0, -500.0f, 8000.0f, -24900.0f, horizon);
            device.SetStreamSource(0, vb, 0);
            for (int i = 0; i < roadCoords.Count; i++)
            {
                roadCoords[i] += speed;
                DrawBox(0, 0, 0, 0.0f, 0.0f, roadCoords[i], road);
                for (int j = 0; j < 10; j++)
                {
                    DrawBox(0, 0, 0, -1024.0f - 512.0f * j, 0.0f, roadCoords[i], grass);
                }
                for (int j = 0; j < 10; j++)
                {
                    DrawBox(0, 0, 0, 1024.0f + 512.0f * j, 0.0f, roadCoords[i], grass);
                }      
            }
            roadCoords.RemoveAll(RemoveAndAdd);
            if (collision)
                DrawCarMesh(angle/(float)Math.PI*2.0f, -(float)Math.PI / 2, 0, car.distanceX, 30.0f, -300.0f, car);   
            else
                DrawCarMesh(0, -(float)Math.PI / 2, 0, car.distanceX, 30.0f, -300.0f, car);   
       //     DrawMesh(0.0f, (float)Math.PI/2, 0, car.distanceX, 30.0f, -300.0f, car);   
            device.EndScene();
            device.Present();
            this.Invalidate();
        }

        public void putInTheEnd()
        {
            roadCoords.RemoveAt(0);
            roadCoords.Add(roadCoords[roadCoords.Count - 1] - 512.0f);
        }

        public bool removeObst(MeshObj o)
        {
            if (o.distanceZ > 700.0f + 512.0f)
                return true;
            return false;
        }

        public bool RemoveAndAdd(float obj)
        {
            if (obj > 700.0f + 512.0f)
            {
                roadCoords.Add(roadCoords[roadCoords.Count - 1] - 512.0f);
                return true;
            }
            return false;
        }

        private void SetupCamera()
        {
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,
                this.Width > this.Height ? this.Width / this.Height : this.Height / this.Width, 1.0f, 30000.0f);


            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 500.0f, 700.0f),
                new Vector3(0.0f, 0.0f, -2048.0f), new Vector3(0, 1, 0));

            //device.RenderState.CullMode = Cull.None;

            device.RenderState.Lighting = false;
            device.RenderState.Ambient = Color.White;
        }

        private void LoadMesh(string file, MeshObj obj)
        {
            ExtendedMaterial[] mtrl;
            Material[] meshMaterials;
            Texture[] meshTextures;
            mesh = Mesh.FromFile(file, MeshFlags.Managed, device, out mtrl);
            if ((mtrl != null) && (mtrl.Length > 0))
            {
                meshMaterials = new Material[mtrl.Length];
                meshTextures = new Texture[mtrl.Length];
                for (int i = 0; i < mtrl.Length; i++)
                {
                    meshMaterials[i] = mtrl[i].Material3D;
                    if ((mtrl[i].TextureFilename != null) && (mtrl[i].TextureFilename != string.Empty))
                        meshTextures[i] = TextureLoader.FromFile(device, @"..\..\Mesh\" + mtrl[i].TextureFilename);
                }
                VertexBuffer vb = mesh.VertexBuffer;
                float radius;
                try
                {
                    GraphicsStream stm = vb.Lock(0, 0, LockFlags.None);
                    Vector3 center;
                    radius = Geometry.ComputeBoundingSphere(stm, mesh.NumberVertices, mesh.VertexFormat, out center);
                    obj.radius = radius;
                }
                finally
                {
                    vb.Unlock();
                    vb.Dispose();
                }
                obj.mesh = mesh;
                obj.meshMaterials = meshMaterials;
                obj.meshTextures = meshTextures;
            }
        }

        private void DrawCarMesh(float yaw, float pitch, float roll, float x, float y, float z, MeshObj obj)
        {
            device.Transform.World = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Matrix.Translation(x, y, z) * Matrix.Scaling(3.0f,3.0f,3.0f);
            for (int i = 0; i < obj.meshMaterials.Length - 1; i++)
            {
                device.Material = obj.meshMaterials[i];
                device.SetTexture(0, obj.meshTextures[i]);
                obj.mesh.DrawSubset(i);
            }
        }

        private void DrawMesh(float yaw, float pitch, float roll, float x, float y, float z, MeshObj obj)
        {
            device.Transform.World = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Matrix.Translation(x, y, z) * Matrix.Scaling(50.0f, 50.0f, 50.0f);
            for (int i = 0; i < obj.meshMaterials.Length; i++)
            {
                device.Material = obj.meshMaterials[i];
                device.SetTexture(0, obj.meshTextures[i]);
                obj.mesh.DrawSubset(i);
            }
        }

        private void ReadKeyboard()
        {
            KeyboardState keys = keyb.GetCurrentKeyboardState();
            if (keys[Microsoft.DirectX.DirectInput.Key.A])
                mouseX = 1.0f;
            if (keys[Microsoft.DirectX.DirectInput.Key.D])
                mouseX = -1.0f;
        }

        private void DrawSimpleMesh(float yaw, float pitch, float roll, float x, float y, float z, MeshObj obj)
        {
            device.Transform.World = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Matrix.Translation(x, y, z);
            obj.mesh.DrawSubset(0);
        }

    }
}
