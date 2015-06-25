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

namespace CG_Task3
{    

    public partial class Form1 : Form
    {
        private Microsoft.DirectX.Direct3D.Device device = null;
        //private VertexBuffer vb = null;
        private float angle = 0;
        private Mesh mesh = null;
        Material mat = new Material();
        public VertexBuffer vb = null;
        public int difficulty = 2;

        public MeshObj mg42 = new MeshObj();
        public MeshObj tiny = new MeshObj();
        public MeshObj sphere = new MeshObj();

        public List<MeshObj> enemies = new List<MeshObj>();

        public Mesh crossfire = null;

        public Microsoft.DirectX.DirectInput.Device keyb;
        public Microsoft.DirectX.DirectInput.Device mouse;

        public float mouseX = 0.0f;
        public float mouseY = 0.0f;
        public float mouseZ = 0.0f;

        public bool fire = false;

        public float shootTimer = 0.0f;
        public float gameTimer = 0.0f;
        public float enemytimer = 0.0f;

        public float farestEnemy = -2000.0f;

        public float camDirectionX = 0.0f;
        public float camDirectionY = 15.0f;

        public Texture block = null;
        CustomVertex.PositionNormalColored[] verts = new CustomVertex.PositionNormalColored[6];
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

            presentParams.EnableAutoDepthStencil = true;
            presentParams.AutoDepthStencilFormat = DepthFormat.D16;
            device = new Microsoft.DirectX.Direct3D.Device(0, Microsoft.DirectX.Direct3D.DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
            Cursor.Position = new Point(this.Width/2,this.Height/2);

            /*
            mat.Diffuse = Color.Red;
            device.Material = mat;
            */
            LoadMesh(@"..\..\..\Mesh\mg42.x", mg42);
            LoadMesh(@"..\..\..\Mesh\tiny.x", tiny);
            sphere = new MeshObj(null, null, Mesh.Sphere(device, tiny.radius, 30, 30));
            block = new Texture(device, new Bitmap(this.GetType(), "crosshair.jpg"), 0, Pool.Managed);

            InitializeKeyboard();

            device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);
            SetupCamera();

            vb = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), 36, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionNormalTextured.Format, Pool.Default);
            vb.Created += new EventHandler(this.OnVertexBufferCreate);
            OnVertexBufferCreate(vb, null);
            
            device.Lights[0].Type = LightType.Point;
            device.Lights[0].Position = new Vector3(0.0f, 0.0f, 0.0f);
            device.Lights[0].Diffuse = System.Drawing.Color.White;
            device.Lights[0].Attenuation0 = 5.0f;
            device.Lights[0].Range = 10000.0f;
            device.Lights[0].Enabled = true;

            ReadKeyboard();

            enemytimer += 0.1f * difficulty;
      //      gameTimer += 0.1f;
            if (enemytimer > 20.0f)
            {
                MeshObj enemy = new MeshObj(sphere);
                enemy.distanceZ = -7000.0f;
                enemy.radius = tiny.radius;
                Random rnd = new Random();
                enemy.distanceX = rnd.Next(-3000, 3000);
                enemies.Add(enemy);
                enemytimer = 0.0f;
            }
            
            device.BeginScene();
            drawLine();
            foreach (MeshObj mo in enemies)
            {
                mo.distanceX += Math.Abs(mo.distanceX/mo.distanceZ)*0.04f;
                mo.distanceZ += Math.Abs(mo.distanceZ/mo.distanceX)*0.04f;
                if (mo.health > 0)
                    DrawSimpleMesh(0, -(float)Math.PI / 2, 0, mo.distanceX, 0.0f, mo.distanceZ, mo);
            }
            enemies.RemoveAll(dead);
            if (enemies.Count == 0)
                gameTimer = 0.0f;
            if (fire)
            {
                shootTimer += 1.0f;
                mouseZ = mouseZ;
                DrawMesh(-(float)Math.PI / 2 - (float)Math.Asin(camDirectionX / 300.0f), 0, -(float)Math.Asin(camDirectionY / 300.0f), 0, 0, 400.0f + (shootTimer % 10 > 5 ? 5.0f : -5.0f), mg42);
                foreach (MeshObj mo in enemies)
                {
                  //  if distance
                    if (IntersectMesh(mo))
                        if (shootTimer % 10 > 5)
                            mo.health -= 0.2f;
                }
            }
            else
            {
                DrawMesh(-(float)Math.PI / 2 - (float)Math.Asin(camDirectionX/310.0f), 0, -(float)Math.Asin(camDirectionY/310.0f), 0, 0, 400.0f, mg42);
            }
            device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
            device.SetStreamSource(0, vb, 0);
            DrawBax(0, 0, 0, camDirectionX, camDirectionY, 100.0f, block);
            DrawBax(0, 0, 0, camDirectionX-2.0f, camDirectionY, 100.0f, block);
            DrawBax(0, 0, 0, camDirectionX+2.0f, camDirectionY, 100.0f, block);
            DrawBax(0, 0, 0, camDirectionX, camDirectionY-2.0f, 100.0f, block);
            DrawBax(0, 0, 0, camDirectionX, camDirectionY+2.0f, 100.0f, block);
            device.EndScene();
            device.Present();
            this.Invalidate();
        }

        private void drawLine()
        {
            float xa = -camDirectionX / 2; // Точка, откуда смотрит камера
            float ya = -camDirectionY / 2;
            float xb = camDirectionX; // Точка, куда смотрит камера
            float yb = camDirectionY;
            float t1 = (10000.0f - 100.0f) / (580.0f - 100.0f);
            float lineX1 = xa * t1 + xb * (1 - t1);
            float lineY1 = ya * t1 + yb * (1 - t1);
            device.Transform.World = Matrix.RotationYawPitchRoll(0, 0, 0) * Matrix.Translation(0.0f, 0.0f, 0.0f);
            verts[1] = new CustomVertex.PositionNormalColored(xa, 0.0f, 580.0f, 0.0f, 0.0f, -5000.0f, Color.Blue.ToArgb());
            verts[0] = new CustomVertex.PositionNormalColored(-lineX1, 50.0f + lineY1, -9000.0f, 0.0f, 0.0f, -5000.0f, Color.Blue.ToArgb());
            device.VertexFormat = CustomVertex.PositionNormalColored.Format;
            device.DrawUserPrimitives(PrimitiveType.LineList, 1, verts);
        }

        private bool IntersectMesh(MeshObj obj)
		{
            float xa = -camDirectionX / 2; // Точка, откуда смотрит камера
            float xb = camDirectionX; // Точка, куда смотрит камера

            float za = 580.0f;
            float zb = 100.0f;
            float zm = obj.distanceZ;
            float t = (zm - zb) / (za - zb);
            float lineX = xa * t + xb * (1 - t);
              
            if (Math.Abs(lineX - obj.distanceX) < obj.radius) // Сравниваем расстояние от неё до Mesh'a с радиусом Mesh'a
                return true;
            else
                return false;

		}

        private void SetupCamera()
        {
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, 
                this.Width > this.Height ? this.Width/this.Height : this.Height/this.Width, 1.0f, 10000.0f);

            camDirectionX += mouseX;
            camDirectionY += mouseY;
            mouseX = 0.0f; mouseY = 0.0f;
            device.Transform.View = Matrix.LookAtLH(new Vector3(-camDirectionX/2, 30-camDirectionY/2, 580.0f), 
                new Vector3(camDirectionX,camDirectionY,100.0f), new Vector3(0, 1, 0));

             //device.RenderState.CullMode = Cull.None;

            device.RenderState.Lighting = false;
            device.RenderState.Ambient = Color.White;            
        }

        private void DrawMesh(float yaw, float pitch, float roll, float x, float y, float z, MeshObj obj)
        {
            angle += 0.01f;
            device.Transform.World = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Matrix.Translation(x, y, z);
            for (int i = 0; i < obj.meshMaterials.Length; i++)
            {
                device.Material = obj.meshMaterials[i];
                device.SetTexture(0, obj.meshTextures[i]);
                obj.mesh.DrawSubset(i);
            }
        }

        private void DrawSimpleMesh(float yaw, float pitch, float roll, float x, float y, float z, MeshObj obj)
        {
            angle += 0.01f;
            device.Transform.World = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Matrix.Translation(x, y, z);
            obj.mesh.DrawSubset(0);
        }

        private void LoadMesh(string file, MeshObj obj)
        {
           ExtendedMaterial[] mtrl;
           Material[] meshMaterials;
           Texture[] meshTextures;
           mesh = Mesh.FromFile(file, MeshFlags.Managed, 	device, out mtrl);
           if ((mtrl != null) && (mtrl.Length > 0))
           {
               meshMaterials = new Material[mtrl.Length];
               meshTextures = new Texture[mtrl.Length];
               for (int i = 0; i < mtrl.Length; i++)
               {
                   meshMaterials[i] = mtrl[i].Material3D;
                   if ((mtrl[i].TextureFilename != null) && (mtrl[i].TextureFilename != string.Empty))
                       meshTextures[i] = TextureLoader.FromFile(device, @"..\..\..\Mesh\" + mtrl[i].TextureFilename);
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

        private void ReadKeyboard()
        {
            KeyboardState keys = keyb.GetCurrentKeyboardState();
            if (keys[Microsoft.DirectX.DirectInput.Key.A])
                mouseX = 1.0f;
            if (keys[Microsoft.DirectX.DirectInput.Key.D])
                mouseX = -1.0f;
       /*     if (keys[Microsoft.DirectX.DirectInput.Key.W])
                mouseY = 0.5f;
            if (keys[Microsoft.DirectX.DirectInput.Key.S])
                mouseY = -0.5f;*/
            if (keys[Microsoft.DirectX.DirectInput.Key.Space])
                fire = true;
            else
                fire = false;
        }


        public bool dead(MeshObj obj)
        {
            return obj.health <= 0;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
           
        }

        private void OnVertexBufferCreate(object sender, EventArgs e)
        {
            VertexBuffer buffer = (VertexBuffer)sender;
            CustomVertex.PositionNormalTextured[] verts_triangle = new CustomVertex.PositionNormalTextured[36];

            // FIRST PLACE
            //front
            verts_triangle[0] = new CustomVertex.PositionNormalTextured(1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 10.0f, 0.0f, 0.0f);
            verts_triangle[1] = new CustomVertex.PositionNormalTextured(-1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 10.0f, 1.0f, 0.0f);
            verts_triangle[2] = new CustomVertex.PositionNormalTextured(-1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 10.0f, 1.0f, 1.0f);
            verts_triangle[3] = new CustomVertex.PositionNormalTextured(-1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 10.0f, 1.0f, 1.0f);
            verts_triangle[4] = new CustomVertex.PositionNormalTextured(1.0f, -1.0f, 1.0f, 0.0f, 0.0f, 10.0f, 0.0f, 1.0f);
            verts_triangle[5] = new CustomVertex.PositionNormalTextured(1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 10.0f, 0.0f, 0.0f);

            //right side
            verts_triangle[6] = new CustomVertex.PositionNormalTextured(-1.0f, 1.0f, 1.0f, -10.0f, 0.0f, 0.0f, 0.0f, 0.0f);
            verts_triangle[7] = new CustomVertex.PositionNormalTextured(-1.0f, 1.0f, -1.0f, -10.0f, 0.0f, 0.0f, 1.0f, 0.0f);
            verts_triangle[8] = new CustomVertex.PositionNormalTextured(-1.0f, -1.0f, -1.0f, -10.0f, 0.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[9] = new CustomVertex.PositionNormalTextured(-1.0f, -1.0f, -1.0f, -10.0f, 0.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[10] = new CustomVertex.PositionNormalTextured(-1.0f, -1.0f, 1.0f, -10.0f, 0.0f, 0.0f, 0.0f, 1.0f);
            verts_triangle[11] = new CustomVertex.PositionNormalTextured(-1.0f, 1.0f, 1.0f, -10.0f, 0.0f, 0.0f, 0.0f, 0.0f);

            //left side            
            verts_triangle[12] = new CustomVertex.PositionNormalTextured(1.0f, 1.0f, -1.0f, 10.0f, 0.0f, 0.0f, 0.0f, 0.0f);
            verts_triangle[13] = new CustomVertex.PositionNormalTextured(1.0f, 1.0f, 1.0f, 10.0f, 0.0f, 0.0f, 1.0f, 0.0f);
            verts_triangle[14] = new CustomVertex.PositionNormalTextured(1.0f, -1.0f, 1.0f, 10.0f, 0.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[15] = new CustomVertex.PositionNormalTextured(1.0f, -1.0f, 1.0f, 10.0f, 0.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[16] = new CustomVertex.PositionNormalTextured(1.0f, -1.0f, -1.0f, 10.0f, 0.0f, 0.0f, 0.0f, 1.0f);
            verts_triangle[17] = new CustomVertex.PositionNormalTextured(1.0f, 1.0f, -1.0f, 10.0f, 0.0f, 0.0f, 0.0f, 0.0f);

            //top
            verts_triangle[18] = new CustomVertex.PositionNormalTextured(1.0f, 1.0f, -1.0f, 0.0f, 10.0f, 0.0f, 0.0f, 0.0f);
            verts_triangle[19] = new CustomVertex.PositionNormalTextured(-1.0f, 1.0f, -1.0f, 0.0f, 10.0f, 0.0f, 1.0f, 0.0f);
            verts_triangle[20] = new CustomVertex.PositionNormalTextured(-1.0f, 1.0f, 1.0f, 0.0f, 10.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[21] = new CustomVertex.PositionNormalTextured(-1.0f, 1.0f, 1.0f, 0.0f, 10.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[22] = new CustomVertex.PositionNormalTextured(1.0f, 1.0f, 1.0f, 0.0f, 10.0f, 0.0f, 0.0f, 1.0f);
            verts_triangle[23] = new CustomVertex.PositionNormalTextured(1.0f, 1.0f, -1.0f, 0.0f, 10.0f, 0.0f, 0.0f, 0.0f);

            //bot            
            verts_triangle[24] = new CustomVertex.PositionNormalTextured(1.0f, -1.0f, 1.0f, 0.0f, -10.0f, 0.0f, 0.0f, 0.0f);
            verts_triangle[25] = new CustomVertex.PositionNormalTextured(-1.0f, -1.0f, 1.0f, 0.0f, -10.0f, 0.0f, 1.0f, 0.0f);
            verts_triangle[26] = new CustomVertex.PositionNormalTextured(-1.0f, -1.0f, -1.0f, 0.0f, -10.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[27] = new CustomVertex.PositionNormalTextured(-1.0f, -1.0f, -1.0f, 0.0f, -10.0f, 0.0f, 1.0f, 1.0f);
            verts_triangle[28] = new CustomVertex.PositionNormalTextured(1.0f, -1.0f, -1.0f, 0.0f, -10.0f, 0.0f, 0.0f, 1.0f);
            verts_triangle[29] = new CustomVertex.PositionNormalTextured(1.0f, -1.0f, 1.0f, 0.0f, -10.0f, 0.0f, 0.0f, 0.0f);

            //behind

            verts_triangle[30] = new CustomVertex.PositionNormalTextured(-1.0f, 1.0f, -1.0f, 0.0f, 0.0f, -10.0f, 0.0f, 0.0f);
            verts_triangle[31] = new CustomVertex.PositionNormalTextured(1.0f, 1.0f, -1.0f, 0.0f, 0.0f, -10.0f, 1.0f, 0.0f);
            verts_triangle[32] = new CustomVertex.PositionNormalTextured(1.0f, -1.0f, -1.0f, 0.0f, 0.0f, -10.0f, 1.0f, 1.0f);
            verts_triangle[33] = new CustomVertex.PositionNormalTextured(1.0f, -1.0f, -1.0f, 0.0f, 0.0f, -10.0f, 1.0f, 1.0f);
            verts_triangle[34] = new CustomVertex.PositionNormalTextured(-1.0f, -1.0f, -1.0f, 0.0f, 0.0f, -10.0f, 0.0f, 1.0f);
            verts_triangle[35] = new CustomVertex.PositionNormalTextured(-1.0f, 1.0f, -1.0f, 0.0f, 0.0f, -10.0f, 0.0f, 0.0f);

            
            buffer.SetData(verts_triangle, 0, LockFlags.None);
        }

        private void DrawBax(float yaw, float pitch, float roll, float x, float y, float z, Texture t)
        {
            angle += 0.001f;
            device.Transform.World = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Matrix.Translation(x, y, z) ;
            //   device.Transform.World = Matrix.RotationX((System.Environment.TickCount / 450.0f) / (float)Math.PI);
            device.SetTexture(0, t);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
        }
            

    }
}

