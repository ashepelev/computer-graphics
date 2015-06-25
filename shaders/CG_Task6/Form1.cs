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


namespace CG_Task6
{
    public partial class Form1 : Form
    {
        private Microsoft.DirectX.Direct3D.Device device = null;
        public Microsoft.DirectX.DirectInput.Device keyb;
        public Microsoft.DirectX.Direct3D.Effect effect = null;
        public Material mat = new Material();
        public Mesh mesh = null;
        public Mesh light = null;
        public float angle = 0.0f;

        public Color commonColor = Color.RosyBrown;

        public void InitializeKeyboard()
        {
            keyb = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
            keyb.SetCooperativeLevel(this, CooperativeLevelFlags.Background | CooperativeLevelFlags.NonExclusive);
            keyb.Acquire();
        }


        public void InitializeGraphics()
        {
            InitializeKeyboard();
            
            PresentParameters presentParams = new PresentParameters();
            presentParams.Windowed = true; presentParams.SwapEffect = SwapEffect.Discard;

            presentParams.EnableAutoDepthStencil = true;
            presentParams.AutoDepthStencilFormat = Microsoft.DirectX.Direct3D.DepthFormat.D16;
            device = new Microsoft.DirectX.Direct3D.Device(0, Microsoft.DirectX.Direct3D.DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);

      //      initLights();

            effect = Microsoft.DirectX.Direct3D.Effect.FromFile(device, @"..\..\hotspot.fx", null, ShaderFlags.None, null);
            effect.Technique = "Hotspot";

            mesh = Mesh.Teapot(device);
            light = Mesh.Sphere(device, 1.0f, 30, 30);
            effect.SetValue("vec_light", new Vector4(20.0f, 20.0f, 0.0f, 0.0f));
            effect.SetValue("vec_eye", new Vector4(0.0f, 20.0f, 80.0f, 0.0f));
            effect.SetValue("vec_view_pos", new Vector4(0, 0, 0, 0));
            effect.SetValue("specular_color", ColorValue.FromColor(commonColor));
            effect.SetValue("specular_intensity", new Vector4(0.1f, 0.1f, 0.1f, 0.1f));

            Material boxMaterial = new Material();
            boxMaterial.Ambient = Color.White;
            boxMaterial.Diffuse = Color.White;
            device.Material = boxMaterial;


        }

        private void initLights()
        {
            device.Lights[0].Type = LightType.Point;
            device.Lights[0].Position = new Vector3(20.0f, 20.0f, 50.0f);
            device.Lights[0].Diffuse = commonColor;
            device.Lights[0].Attenuation0 = 1000.0f;
            device.Lights[0].Range = 100000.0f;
            device.Lights[0].Enabled = true;
        }

       
        protected override void OnPaint(PaintEventArgs e)
        {
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.CornflowerBlue, 1.0f, 0);
            SetupCamera();
            
         //   ReadKeyboard();
            device.BeginScene();
            DrawMesh(0, 0, 0, 20.0f, 20.0f, 0.0f, light);
            angle += 0.01f;
            int numPass = effect.Begin(0);
            for (int i = 0; i < numPass; i++)
            {
                effect.BeginPass(i);
                DrawSimpleMesh(angle/(float)Math.PI, angle/(float)Math.PI/2, 0, 0, 0, 0, mesh);
                effect.EndPass();
            }
            effect.End();
            //DrawSimpleMesh(angle / (float)Math.PI, angle / (float)Math.PI / 2, 0, 0, 0, 0, mesh);
            device.EndScene();
            device.Present();
            this.Invalidate();
        }

        private void SetupCamera()
        {
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,
                this.Width > this.Height ? this.Width / this.Height : this.Height / this.Width, 1.0f, 30000.0f);

            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 20.0f, 80.0f),
                new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0, 1, 0));

            device.RenderState.Lighting = true;
            device.RenderState.Ambient = Color.White;
            
        }

        private void DrawSimpleMesh(float yaw, float pitch, float roll, float x, float y, float z, Mesh obj)
        {
            device.Transform.World = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Matrix.Scaling(10.0f,10.0f,10.0f);// *Matrix.Translation(x, y, z);
            //effect1.SetValue("time", timeColor);
            effect.SetValue("WorldViewProj", device.Transform.World * device.Transform.View * device.Transform.Projection);
            obj.DrawSubset(0);
        }

        private void DrawMesh(float yaw, float pitch, float roll, float x, float y, float z, Mesh obj)
        {
            device.Transform.World = Matrix.RotationYawPitchRoll(yaw, pitch, roll) * Matrix.Translation(x, y, z); ;// *Matrix.Translation(x, y, z);
            obj.DrawSubset(0);
        }

        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
        }


    }
}
