﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGamers.Camera;
using MonoGamers.Collisions;
using MonoGamers.Geometries.Textures;
using MonoGamers.Pistas;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using MonoGamers.Geometries;
using MonoGamers.Physics;
using TGC.MonoGame.Samples.Physics.Bepu;
using MonoGamers.Checkpoints;
using MonoGamers.Utilities;
using NumericVector3 = System.Numerics.Vector3;
using MonoGamers.PowerUps;
using MonoGamers.SkyBoxes;
using Microsoft.Xna.Framework.Media;
using MonoGamers.Audio;
using System.Diagnostics;
using MonoGamers.Menu;
using System.Reflection.Metadata;
using TGC.MonoGame.Samples.Geometries;
using System.Threading.Tasks;

namespace MonoGamers
{
    /// <summary>
    ///     Esta es la clase principal del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        
        private const float Gravity = 350f;
        private const float yMinimo = -450f;


        

        //Esfera lógica
        MonoSphere MonoSphere;

        // Camera to draw the scene
        private TargetCamera Camera { get; set; }
        private TargetLightCamera TargetLightCamera { get; set; }
        
        private readonly float LightCameraFarPlaneDistance = 3000f;
        private readonly float LightCameraNearPlaneDistance = 5f;
        private Vector3 SunPosition  = new Vector3(-500f, 1400f, -300f);
        
        private GraphicsDeviceManager Graphics { get; }

        // Geometries
        private QuadPrimitive Floor { get; set; }
        
        
        // Simulation
        private MonoSimulation MonoSimulation { get; set; }
        private Simulation Simulation { get; set; }
        
        


        // World matrices
        private Matrix FloorWorld { get; set; }

        //Texturas
        private Texture2D FloorTexture { get; set; }
        private Texture2D FloorNormalTexture { get; set; }
        

        // Effects

        // Basic Shader Effect
        private Effect LightEffect { get; set; }
        
        
        // Pistas
        Pista1 Pista1 { get; set; }
        Pista2 Pista2 { get; set; }
        Pista3 Pista3 {get; set;}
        Pista4 Pista4 {get; set;}

        // Checkpoints
        private Checkpoint[] Checkpoints { get; set; }

        // Skybox
        private SkyBox SkyBox { get; set; }

        private PowerUp[] PowerUps { get; set; }
        private int CurrentCheckpoint { get; set; }

        //Stopwatch
        Stopwatch stopwatchInitialize = new Stopwatch();
        Stopwatch stopwatchLoad = new Stopwatch();
        Stopwatch stopwatchUpdate = new Stopwatch();
        Stopwatch stopwatchDraw= new Stopwatch();

        bool hasMeasuredLoadContent = false;
        bool hasMeasuredUpdate = false;
        bool hasMeasuredDraw = false;

        
        //SpriteBatch
        private SpriteBatch SpriteBatch { get; set; }
        private SpriteFont SpriteFont { get; set; }
        
        // AudioController 
        private AudioController AudioController { get; set; }

        //Menu

        private Menu.Menu Menu { get; set; }


        private MouseState PreviousMouseState { get; set; }
        
        // Shadow map
        private RenderTarget2D ShadowMapRenderTarget { get; set; }
        private const int ShadowmapSize = 4096;
        
        // Environment Map
        private RenderTargetCube EnvironmentMapRenderTarget { get; set; }
        private const int EnvironmentmapSize = 100;
        private StaticCamera CubeMapCamera { get; set; }

        //Blur

        private FullScreenQuad FullScreenQuad;

        private RenderTarget2D MainRenderTarget;
        private RenderTarget2D HorizontalRenderTarget;

        private Effect BlurEffect;

        public bool BlurON = false;

        
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }


        /// <inheritdoc />
        protected override void Initialize()
        {
            stopwatchInitialize.Start();

            // Enciendo Back-Face culling.
            // Configuro Blend State a Opaco.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Configuro las dimensiones de la pantalla.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Graphics.ApplyChanges();
            
            PreviousMouseState = Mouse.GetState();

            // Creo una camara para seguir a la esfera.
            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 100f, Vector3.Zero, GraphicsDevice.Viewport);


            // Creo los checkpoints
            Checkpoints = new Checkpoint[]
            {
                new Checkpoint(new Vector3(100f, 25f, 60f)),
                new Checkpoint(new Vector3(100f, 25f, 160f), new Vector3(100f, 50f, 25f),Content, GraphicsDevice),
                new Checkpoint(new Vector3(100f, 37.5f, 4394f), new Vector3(300f, 75f, 25f),Content, GraphicsDevice),
                new Checkpoint(new Vector3(2500f, 150f, 7144f), new Vector3(70f, 25f, 70f),Content, GraphicsDevice),
                new Checkpoint(new Vector3(4100f, 360f, 7200f), new Vector3(25f, 50f, 148f),Content, GraphicsDevice),
                new Checkpoint(new Vector3(3775f, 565f, 9315f), new Vector3(100f, 50f, 25f),Content, GraphicsDevice, true),

            };
            CurrentCheckpoint = 0;

            // PowerUps
            PowerUps = new PowerUp[]
            {
                new JumpPowerUp(new Vector3(100f, 10f, 500f)),
                new FastPowerUp(new Vector3(100f, 10f, 4550f)),
                new JumpPowerUp(new Vector3(2000f, 150f, 7144f)),
            };

            
            // Empezar Simulacion
            MonoSimulation = new MonoSimulation();
            Simulation = MonoSimulation.Init();
            
            // Inicializar pistas
            Pista1 = new Pista1(Content, GraphicsDevice, 100f, -3f, 450f, Simulation);
            Pista2 = new Pista2(Content, GraphicsDevice, 100f, -3f, 4594f, Simulation);
            Pista3 = new Pista3(Content, GraphicsDevice, 2500f, 137f, 7144f, Simulation);
            Pista4 = new Pista4(Content, GraphicsDevice, 4000f, 330f, 7200f, Simulation);
            
            AudioController = new AudioController(Content);

            MonoSphere = new MonoSphere(Checkpoints[CurrentCheckpoint].Position, Gravity, Simulation);

            //Menu
            Menu = new Menu.Menu(Content, GraphicsDevice, this);

            
            // raget Light Camera
            TargetLightCamera = new TargetLightCamera(1f, 
                new Vector3(MonoSphere.SpherePosition.X + SunPosition.X ,MonoSphere.SpherePosition.Y + SunPosition.Y, MonoSphere.SpherePosition.Z + SunPosition.Z),
                MonoSphere.SpherePosition ,LightCameraNearPlaneDistance,LightCameraFarPlaneDistance);
            TargetLightCamera.BuildProjection(1f, LightCameraNearPlaneDistance, LightCameraFarPlaneDistance,
                MathHelper.PiOver2);
            TargetLightCamera.BuildView();
            
            // Environment map Camera
            CubeMapCamera = new StaticCamera(1f, MonoSphere.SpherePosition, Vector3.UnitX, Vector3.Up);
            CubeMapCamera.BuildProjection(1f, 1f, 3000f, MathHelper.PiOver2);

            //Blur
            loadBlur();

            base.Initialize();
            stopwatchInitialize.Stop();
        }
        

        /// <inheritdoc />
        protected override void LoadContent()
        {
            if (!hasMeasuredLoadContent)
            {
                stopwatchLoad.Start();
                hasMeasuredLoadContent = true;
            }
            
            MonoSphere.SpherePrimitive = new SpherePrimitive(GraphicsDevice);
            

            //Contenido de HUD
                SpriteBatch = new SpriteBatch(GraphicsDevice);
                SpriteFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");

                foreach(var powerUp in PowerUps)
                {
                    powerUp.LoadContent(Content);
                }

            // Load Textures
                
                FloorTexture = Content.Load<Texture2D>(ContentFolderTextures + "floor/concrete");
                FloorNormalTexture = Content.Load<Texture2D>(ContentFolderTextures + "floor/concrete-normal");
                
                
                MonoSphere.SphereModel = Content.Load<Model>(ContentFolder3D + "geometries/sphere");

                MonoSphere.SphereCommonTexture = Content.Load<Texture2D>(ContentFolderTextures + "pbr/marble/color");
                MonoSphere.SphereCommonNormalTexture = Content.Load<Texture2D>(ContentFolderTextures + "pbr/marble/normal");                
                MonoSphere.SphereStoneTexture = Content.Load<Texture2D>(ContentFolderTextures + "rock/color");
                MonoSphere.SphereStoneNormalTexture = Content.Load<Texture2D>(ContentFolderTextures + "rock/normal");
                MonoSphere.SphereMetalTexture = Content.Load<Texture2D>(ContentFolderTextures + "pbr/metal/color");
                MonoSphere.SphereMetalNormalTexture = Content.Load<Texture2D>(ContentFolderTextures + "pbr/metal/normal");
                MonoSphere.SphereGumTexture = Content.Load<Texture2D>(ContentFolderTextures + "gum");
                MonoSphere.SphereGumNormalTexture = Content.Load<Texture2D>(ContentFolderTextures + "pbr/marble/normal");


            // Load our LightEffect
                LightEffect = Content.Load<Effect>(ContentFolderEffects + "LightEffect");
                LightEffect.Parameters["lightPosition"].SetValue(TargetLightCamera.Position);
                LightEffect.Parameters["ambientColor"].SetValue((Color.LightGoldenrodYellow).ToVector3());
                LightEffect.Parameters["diffuseColor"].SetValue((Color.LightGoldenrodYellow).ToVector3());
                LightEffect.Parameters["specularColor"].SetValue((Color.White).ToVector3());
                
                
                
                MonoSphere.SphereEffect = LightEffect;
                Pista1.Effect = LightEffect;
                Pista2.Effect = LightEffect;
                Pista3.Effect = LightEffect;
                Pista4.Effect = LightEffect;
                
                

            // Create our Quad (to draw the Floor) and add it to Simulation
                Floor = new QuadPrimitive(GraphicsDevice);
                FloorWorld = Matrix.CreateScale(200f, 0.001f, 200f);
                Simulation.Statics.Add(new StaticDescription(new NumericVector3(0f, 0f, 0f),
                    Simulation.Shapes.Add(new Box(400f, 0.002f, 400f))));

                var skyBox = Content.Load<Model>(ContentFolder3D + "skybox/cube");
                var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "skyboxes/sunset/sunset");
                var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
                SkyBox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect);
                
                //Menu
                Menu.LoadContent();
                
                
                // Create a shadow map. It stores depth from the light position
                ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, ShadowmapSize, ShadowmapSize, false,
                    SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
                

                EnvironmentMapRenderTarget = new RenderTargetCube(GraphicsDevice, EnvironmentmapSize, false,
                    SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            base.LoadContent();
            if (stopwatchLoad.IsRunning)
            {
                stopwatchLoad.Stop();
            }       
        }
        
        

        /// <inheritdoc />
        protected override void Update(GameTime gameTime)
        {
            if (!hasMeasuredUpdate) {
                stopwatchUpdate.Start();
                hasMeasuredUpdate = true;
                
            }
            
            // The time that passed between the last loop
            var deltaTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            
            var keyboardState = Keyboard.GetState();
            var MouseState = Mouse.GetState();
            
            // Check for key presses and rotate accordingly
            // We can stack rotations in a given axis by multiplying our past matrix
            // By a new matrix containing a new rotation to apply
            // Also, recalculate the Front Direction

             if (Menu.OnMenu)
            {
                Camera.UpdateCamera(gameTime, new Vector3(100f, 450f, 500f), Menu.OnMenu);
                Menu.Update(PreviousMouseState,MouseState);

            } else {
                 
                 IsMouseVisible = false;
                
                MonoSphere.Update(Simulation, Camera, keyboardState);

                Array.ForEach(PowerUps, PowerUp => PowerUp.Update());

                CheckpointManager();
                Array.ForEach(PowerUps, PowerUp => PowerUp.ActivateIfBounding(Simulation, MonoSphere));
                
                
                
                Pista1.Update();
                Pista2.Update();

                Camera.UpdateCamera(gameTime, MonoSphere.SpherePosition, Menu.OnMenu);
            }
    
            PreviousMouseState = MouseState;

            MonoSimulation.Update();

            TargetLightCamera.Position = new Vector3(MonoSphere.SpherePosition.X + SunPosition.X,
                MonoSphere.SpherePosition.Y + SunPosition.Y, MonoSphere.SpherePosition.Z + SunPosition.Z);
            TargetLightCamera.TargetPosition =  MonoSphere.SpherePosition;

            TargetLightCamera.BuildView();
            
            
            CubeMapCamera.Position = MonoSphere.SpherePosition;
            
            if (keyboardState.IsKeyDown(Keys.Escape)) Exit();

            base.Update(gameTime);
            if (stopwatchUpdate.IsRunning)
            {
                stopwatchUpdate.Stop();
            }  
        }

        private void CheckpointManager()
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(MonoSphere.SphereHandle);

            if(MonoSphere.SphereFalling(yMinimo))
            {
                AudioController.PlayRise();
                bodyRef.Pose.Position = MonoGamers.Utilities.Utils.ToNumericVector3(Checkpoints[CurrentCheckpoint].Position);
                bodyRef.Velocity.Linear = NumericVector3.Zero;
                bodyRef.Velocity.Angular = NumericVector3.Zero;
                
                return;
            }
            for(int i = CurrentCheckpoint; i < Checkpoints.Length; i++)
            {
                bool alreadyPassed = Checkpoints[i].alreadyPassed; 
                if(Checkpoints[i].IsWithinBounds(bodyRef.Pose.Position))
                {
                    CurrentCheckpoint = i;
                    if (Checkpoints[i].FinalCheckpoint && !alreadyPassed) ActivateBlur();
                    return;
                }
            }
        }

        /// <inheritdoc />
        protected override void Draw(GameTime gameTime)
        {
            if (!hasMeasuredDraw)
            {
                stopwatchDraw.Start();
                hasMeasuredDraw = true;
            }
            
                        
            // Limpio la pantalla.
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Calculate the ViewProjection matrix
            //var viewProjection = FollowCamera.View * FollowCamera.Projection;
            var viewProjection = Camera.View * Camera.Projection;

            // Environment Map if metal
            if (MonoSphere.SphereType == MonoSphere.Type.Metal)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                // Draw to our cubemap from the robot position
                for (var face = CubeMapFace.PositiveX; face <= CubeMapFace.NegativeZ; face++)
                {
                    // Set the render target as our cubemap face, we are drawing the scene in this texture
                    GraphicsDevice.SetRenderTarget(EnvironmentMapRenderTarget, face);
                    GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

                    SetCubemapCameraForOrientation(face);
                    CubeMapCamera.BuildView();

                    // Draw our scene. Do not draw our tank as it would be occluded by itself 
                    // (if it has backface culling on)
                    
                    
                    // Dibujamos el skybox
                    DrawSkybox(CubeMapCamera);
                    
                    LightEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * CubeMapCamera.View * CubeMapCamera.Projection);
                    Floor.Draw(LightEffect);
                
                    // Dibujamos las pistas
                    Pista1.Draw(CubeMapCamera);
                    Pista2.Draw(CubeMapCamera);
                    Pista3.Draw(CubeMapCamera);
                    Pista4.Draw(CubeMapCamera);
                }
            }
            
            #region Pass 1

                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                // Set the render target as our shadow map, we are drawing the depth into this texture
                GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1f, 0);

                LightEffect.CurrentTechnique = LightEffect.Techniques["DepthPass"];

                // Sphere drawinga
                MonoSphere.Draw(TargetLightCamera);
                
                // Floor drawing
                
                LightEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * TargetLightCamera.View * TargetLightCamera.Projection);
                Floor.Draw(LightEffect);
                
                // Dibujamos las pistas
                Pista1.Draw(TargetLightCamera);
                Pista2.Draw(TargetLightCamera);
                Pista3.Draw(TargetLightCamera);
                Pista4.Draw(TargetLightCamera);


                //powerups Drawing
                foreach (var powerup in PowerUps)
                {
                    powerup.drawOnlyFloatingSphere = false;
                    
                    powerup.Draw(Camera, gameTime); 
                }



            #endregion

            #region Pass 2


                if (BlurON)
                {
                    GraphicsDevice.SetRenderTarget(MainRenderTarget);
                }
                else
                {
                    // Set the render target as null, we are drawing on the screen!
                    GraphicsDevice.SetRenderTarget(null);
                }
                
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

                LightEffect.CurrentTechnique = LightEffect.Techniques["Draw_NM_SM"];
                LightEffect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
                LightEffect.Parameters["lightPosition"].SetValue(TargetLightCamera.Position);
                LightEffect.Parameters["shadowMapSize"].SetValue(Vector2.One * ShadowmapSize);
                LightEffect.Parameters["LightViewProjection"].SetValue(TargetLightCamera.View * TargetLightCamera.Projection);
                    
      
                
                // Floor drawing
                
                    
                LightEffect.Parameters["eyePosition"].SetValue(Camera.Position);
                LightEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));
                LightEffect.Parameters["ModelTexture"].SetValue(FloorTexture);
                LightEffect.Parameters["NormalTexture"].SetValue(FloorNormalTexture);
                    
                LightEffect.Parameters["KAmbient"]?.SetValue(0.4f);
                LightEffect.Parameters["KDiffuse"].SetValue(0.5f);
                LightEffect.Parameters["shininess"].SetValue(20.0f);
                LightEffect.Parameters["KSpecular"].SetValue(0.5f);

                LightEffect.Parameters["World"].SetValue(FloorWorld);
                LightEffect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(FloorWorld)));
                LightEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * viewProjection);
                    
                Floor.Draw(LightEffect);
                
                // Dibujamos las pistas
                Pista1.Draw(Camera);
                Pista2.Draw(Camera);
                Pista3.Draw(Camera);
                Pista4.Draw(Camera);


                
                // Sphere drawing
                if (MonoSphere.SphereType != MonoSphere.Type.Metal)
                {
                    MonoSphere.Draw(Camera);
                }
                else
                {
                    LightEffect.CurrentTechnique = LightEffect.Techniques["EnvironmentMapSphere"];
                    LightEffect.Parameters["environmentMap"].SetValue(EnvironmentMapRenderTarget);
                    LightEffect.Parameters["eyePosition"].SetValue(Camera.Position);

                    var sphereWorld = Matrix.CreateTranslation(MonoSphere.SpherePosition);

                    // World is used to transform from model space to world space
                    LightEffect.Parameters["World"].SetValue(sphereWorld);
                    // InverseTransposeWorld is used to rotate normals
                    LightEffect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Transpose(Matrix.Invert(sphereWorld)));
                    // WorldViewProjection is used to transform from model space to clip space
                    LightEffect.Parameters["WorldViewProjection"].SetValue(sphereWorld * Camera.View * Camera.Projection);
                    MonoSphere.Draw(Camera);
                }

                //powerups Drawing
                foreach (var powerup in PowerUps)
                {
                    powerup.drawOnlyFloatingSphere = false;
                    
                    powerup.Draw(Camera, gameTime); 
                }

                // Dibujamos el skybox
                DrawSkybox(Camera);
            
                //powerups Drawing -- Se dibuja despues del skybox para que aparezca en la esfera
                foreach (var powerup in PowerUps)
                {
                    powerup.drawOnlyFloatingSphere = true;
                    
                    powerup.Draw(Camera, gameTime); 
                }
            
                foreach (var checkpoint in Checkpoints)
                {

                    checkpoint.Draw(Camera); 
                }

            #endregion

            
            if(BlurON) drawBlur();

            DrawUI(gameTime);

            base.Draw(gameTime);
            
            if (stopwatchDraw.IsRunning)
                stopwatchDraw.Stop();
        }
        
        private void DrawSkybox(Camera.Camera camera)
        {
            var originalRasterizerState = GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Graphics.GraphicsDevice.RasterizerState = rasterizerState;
            SkyBox.Draw(camera.View, camera.Projection, MonoSphere.SpherePosition);
            GraphicsDevice.RasterizerState = originalRasterizerState;
        }

        private void DrawUI(GameTime gameTime)
        {
            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            var color = new Color();
            //Hud 
            switch (MonoSphere.Material)
            {
                case "Common":
                    color = Color.LightGreen;
                    break;
                case "Metal":
                    color = Color.LightGray;
                    break;
                case "Gum":
                    color = Color.Pink;
                    break;
                case "Stone":
                    color = Color.DarkGray;
                    break;
                    
            }
            if (!Menu.OnMenu)
            {
                var gm = MonoSphere.godMode;
                var Height = GraphicsDevice.Viewport.Height;
                var Width = GraphicsDevice.Viewport.Width;
                var fps = MathF.Round(1/Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds), 1);
                var tiempoTotal = stopwatchInitialize.Elapsed + stopwatchLoad.Elapsed + stopwatchUpdate.Elapsed + stopwatchDraw.Elapsed;
                var position = new Vector3(MathF.Round(MonoSphere.SpherePosition.X, 1), MathF.Round(MonoSphere.SpherePosition.Y, 1), MathF.Round(MonoSphere.SpherePosition.Z, 1));
                SpriteBatch.DrawString(SpriteFont, "Checkpoints: " + CurrentCheckpoint.ToString() +"/5 ", new Vector2(Width*0.02f, Height*0.01F), color);
                SpriteBatch.DrawString(SpriteFont, "GODMODE (G) :" + (gm ? "ON" : "OFF"), new Vector2(GraphicsDevice.Viewport.Width/4, Height*0.01F), color);
                SpriteBatch.DrawString(SpriteFont, "Position:" + position.ToString(), new Vector2(Width - 500, Height*0.01F), color);
                SpriteBatch.DrawString(SpriteFont, "Material:" + MonoSphere.Material, new Vector2(Width - 400, Height*0.06f), color);
                SpriteBatch.DrawString(SpriteFont, "USE THE T,Y,U,I KEYS TO CHANGE MATERIALS", new Vector2(Width/3, Height*0.85F), color);
                if (gm) SpriteBatch.DrawString(SpriteFont, "PRESS THE 1,2,3,4,5 KEYS TO MOVE TO THE NEXT CHECKPOINT", new Vector2(Width/3, Height*0.9F), color);
                if (gm) SpriteBatch.DrawString(SpriteFont, "FPS: " + fps.ToString(), new Vector2(Width*0.01f, Height*0.05F), color);
                if (gm) SpriteBatch.DrawString(SpriteFont, "Tiempo Initialize:" + stopwatchInitialize.Elapsed, new Vector2(Width*0.01f, Height*0.15F), color);
                if (gm)SpriteBatch.DrawString(SpriteFont, "Tiempo Load:" + stopwatchLoad.Elapsed, new Vector2(Width*0.01f, Height*0.20F), color);
                if (gm) SpriteBatch.DrawString(SpriteFont, "Tiempo Update:" + stopwatchUpdate.Elapsed, new Vector2(Width*0.01f, Height*0.25F), color);
                if (gm) SpriteBatch.DrawString(SpriteFont, "Tiempo Draw:" + stopwatchDraw.Elapsed, new Vector2(Width*0.01f, Height*0.30F), color);
                if (gm) SpriteBatch.DrawString(SpriteFont, "Tiempo total:" + tiempoTotal, new Vector2(Width*0.01f, Height*0.35F), color);
                if (gm) SpriteBatch.DrawString(SpriteFont, "Side Speed: " + (MonoSphere.SphereSideSpeed * MonoSphere.SphereSideTypeSpeed).ToString(),
                                        new Vector2(Width*0.01f, Height*0.45F), color);
                if (gm) SpriteBatch.DrawString(SpriteFont, "Jump Speed: " + (MonoSphere.SphereJumpSpeed * MonoSphere.SphereJumpTypeSpeed).ToString(),
                                        new Vector2(Width*0.01f, Height*0.50F), color);
            }

            //Menu
            //draw the start menu
            if (Menu.OnMenu)
            {
                Menu.Draw(SpriteBatch);
            }

            
            SpriteBatch.End();
        }
        
        /// <summary>
        ///     Sets the camera orientation based on the cubemap face.
        /// </summary>
        private void SetCubemapCameraForOrientation(CubeMapFace face)
        {
            switch (face)
            {
                default:
                case CubeMapFace.PositiveX:
                    CubeMapCamera.FrontDirection = -Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeX:
                    CubeMapCamera.FrontDirection = Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.PositiveY:
                    CubeMapCamera.FrontDirection = Vector3.Down;
                    CubeMapCamera.UpDirection = Vector3.UnitZ;
                    break;

                case CubeMapFace.NegativeY:
                    CubeMapCamera.FrontDirection = Vector3.Up;
                    CubeMapCamera.UpDirection = -Vector3.UnitZ;
                    break;

                case CubeMapFace.PositiveZ:
                    CubeMapCamera.FrontDirection = -Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeZ:
                    CubeMapCamera.FrontDirection = Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;
            }
        }

        private void loadBlur(){
            // Load the post-processing effect
            BlurEffect = Content.Load<Effect>(ContentFolderEffects + "GaussianBlur");

            // Create a full screen quad to post-process
            FullScreenQuad = new FullScreenQuad(GraphicsDevice);

            // Create render targets. One can be used for simple gaussian blur
            // mainRenderTarget is also used as a render target in the separated filter
            // horizontalRenderTarget is used as the horizontal render target in the separated filter
            MainRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);
            
            HorizontalRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.DiscardContents);

            BlurEffect.Parameters["screenSize"]
                .SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

        }
        
        private void drawBlur(){

            #region Pass 1

            // Set the depth configuration as none, as we don't use depth in this pass
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // Set the render target as horizontalRenderTarget, 
            // we are drawing a horizontal blur into this texture
            GraphicsDevice.SetRenderTarget(HorizontalRenderTarget);
            GraphicsDevice.Clear(Color.Black);

            // Set the technique to our blur technique
            // Then draw a texture into a full-screen quad
            // using our rendertarget as texture

            BlurEffect.CurrentTechnique = BlurEffect.Techniques["BlurHorizontalTechnique"];
            BlurEffect.Parameters["baseTexture"].SetValue(MainRenderTarget);
            FullScreenQuad.Draw(BlurEffect);

            #endregion

            #region Pass 2

            // Now we are drawing into the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Set the technique to our blur technique
            // Then draw a texture into a full-screen quad
            // using our rendertarget as texture

            BlurEffect.CurrentTechnique = BlurEffect.Techniques["BlurVerticalTechnique"];
            BlurEffect.Parameters["baseTexture"].SetValue(HorizontalRenderTarget);
            FullScreenQuad.Draw(BlurEffect);

            #endregion
        }
        
        private async void ActivateBlur()
        {
            AudioController.PlayCheer();
            BlurON = true;
            await Task.Delay(2000);
            BlurON = false;
        }
        
        
        
    }
    

}