﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OFPSEngine;
using OFPSEngine.Rendering;
using OFPSEngine.ResourceManagement;
using SharpDX;
using Matrix = SharpDX.Matrix;
using Point = System.Drawing.Point;

namespace OFPSGame
{
    public partial class FormEditor : Form
    {
        ControlViewport control;
        private Stopwatch gameLoopWatch;
        private Timer gameTimer;
        private Model3DResource model;
        private Model3DResource floor;
        private bool mdown;
        private Point mpos;
        private List<Keys> pressedKeys = new List<Keys>(); 
        private Camera camera = new Camera();
        private Texture2DResource[] texture = new Texture2DResource[4];
        private Texture2DResource[] normalmap = new Texture2DResource[4];        
        private Texture2DResource[] metallicmap = new Texture2DResource[4];
        private Texture2DResource[] roughnessmap = new Texture2DResource[4];
        private Texture2DResource cubemap;
        private DateTime start = DateTime.Now;

        public FormEditor()
        {
            InitializeComponent();
        }

        private void FormGame_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            var resolver = new GenericFileResolver("GameContent");
            var resourceManager = new ResourceManager(resolver);
            resourceManager.RegisterLoader(new Model3DLoader(), ".fbx");
            resourceManager.RegisterLoader(new Texture2DLoader(), ".png;.jpg;.bmp;.tga;.dds");
            
            model = resourceManager.Load<Model3DResource>("material_sample.fbx");

            floor = resourceManager.Load<Model3DResource>("floor.fbx");

            texture[0] = resourceManager.Load<Texture2DResource>("DefaultMaterial_Base_Color.png");
            normalmap[0] = resourceManager.Load<Texture2DResource>("DefaultMaterial_Normal_DirectX.png");
            metallicmap[0] = resourceManager.Load<Texture2DResource>("DefaultMaterial_Metallic.png");
            roughnessmap[0] = resourceManager.Load<Texture2DResource>("DefaultMaterial_Roughness.png");

            texture[1] = resourceManager.Load<Texture2DResource>("t1_c.png");
            normalmap[1] = resourceManager.Load<Texture2DResource>("t1_n.png");
            metallicmap[1] = resourceManager.Load<Texture2DResource>("t1_m.png");
            roughnessmap[1] = resourceManager.Load<Texture2DResource>("t1_r.png");

            texture[2] = resourceManager.Load<Texture2DResource>("t2_c.png");
            normalmap[2] = resourceManager.Load<Texture2DResource>("t2_n.png");
            metallicmap[2] = resourceManager.Load<Texture2DResource>("t2_m.png");
            roughnessmap[2] = resourceManager.Load<Texture2DResource>("t2_r.png");

            texture[3] = resourceManager.Load<Texture2DResource>("floor_c.png");
            normalmap[3] = resourceManager.Load<Texture2DResource>("floor_n.png");
            metallicmap[3] = resourceManager.Load<Texture2DResource>("floor_m.png");
            roughnessmap[3] = resourceManager.Load<Texture2DResource>("floor_r.png");

            cubemap = resourceManager.Load<Texture2DResource>("yoko.dds");

            control = new ControlViewport() {Dock = DockStyle.Fill};
            control.CustomRender += ControlOnCustomRender;
            Controls.Add(control);

            control.MouseMove += ControlOnMouseMove;
            control.KeyDown += ControlOnKeyDown;
            control.KeyUp += ControlOnKeyUp;

            gameLoopWatch = Stopwatch.StartNew();
            gameTimer = new Timer();
            gameTimer.Interval = 15;
            gameTimer.Tick += GameTimerOnTick;
            gameTimer.Start();

            camera.Position = Vector3.ForwardLH*10f;
        }

        private void ControlOnKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            pressedKeys.Clear();
        }

        private void ControlOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            pressedKeys.Add(keyEventArgs.KeyCode);
        }

        private void ControlOnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                if (!mdown)
                {
                    mdown = true;
                }
                else
                {
                    camera.Angle -= new Vector2(e.X - mpos.X, -(e.Y - mpos.Y))/200f;
                }
                mpos = e.Location;
            }
            else mdown = false;
        }

        private void ControlOnCustomRender()
        {
            var info = new DrawInfo();

            for (int i = 0; i < 3; i++)
            {

                info.World = Matrix.RotationY((float) (DateTime.Now - start).TotalSeconds*0.2f)*
                             Matrix.Translation(i*3-3, 0, 0);
                info.Projection = camera.Projection;
                info.View = camera.View;
                info.DiffuseMap = texture[i];
                info.NormalMap = normalmap[i];
                info.MetallicMap = metallicmap[i];
                info.RoughnessMap = roughnessmap[i];
                info.CameraPosition = camera.Position;
                info.CubeMap = cubemap;

                Renderer.Current.DrawModel3D(model, info);
            }
            info.World = Matrix.Scaling(10)*Matrix.Translation(0, -1.7f, 0);
            info.DiffuseMap = texture[3];
            info.NormalMap = normalmap[3];
            info.MetallicMap = metallicmap[3];
            info.RoughnessMap = roughnessmap[3];
            Renderer.Current.DrawModel3D(floor, info);
        }

        private void GameTimerOnTick(object sender, EventArgs eventArgs)
        {
            camera.UpdateOrientation();
            Vector3 delta = Vector3.Zero;

            if (pressedKeys.Contains(Keys.W)) delta += camera.Orientation.Forward;
            if (pressedKeys.Contains(Keys.S)) delta += camera.Orientation.Backward;
            if (pressedKeys.Contains(Keys.A)) delta += camera.Orientation.Left;
            if (pressedKeys.Contains(Keys.D)) delta += camera.Orientation.Right;

            camera.Position += delta*0.2f;

            camera.UpdateViewProjection(control.ClientSize.Width, control.ClientSize.Height);
            control.Render();
        }

        private void FormGame_Shown(object sender, EventArgs e)
        {
            if (DesignMode) return;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            //control.Render();
            Logger.Info("Game started");
        }
    }
}
