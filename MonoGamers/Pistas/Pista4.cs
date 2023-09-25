﻿using System.Linq;
using BepuPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGamers.Geometries.Textures;
using NumericVector3 = System.Numerics.Vector3;
using BepuPhysics.Collidables;
using MonoGamers.Utilities;
using System.Configuration;

namespace MonoGamers.Pistas;

public class Pista4
{

    // ======== DECALRACION DE VARIABES ========

    // ____ Colliders ____

    // Bounding Boxes (for all our models)
    private BoundingBox[] Colliders { get; set; }

    // _____ Geometries _______
    private BoxPrimitive BoxPrimitive { get; set; }

    // ____ Textures ____
    private Texture2D CobbleTexture { get; set; }

    // ____ World matrices ____
    // Plataformas principales
    private Matrix Platform1World { get; set; }
    private Matrix Platform2World { get; set; }

    // Floating Platforms
    private Matrix[] FloatingPlatformsWorld { get; set; }

    // FloatingMovingPlatform (Vertical Movement)
    private Matrix FloatingMovingPlatformWorld { get; set; }

    // GraphicsDevice
    private GraphicsDevice GraphicsDevice { get; set; }
    
    // Simulation           
    private Simulation Simulation { get; set; }


    // ======== Constructor de Pista 4 ========
    public Pista4(ContentManager Content, GraphicsDevice graphicsDevice, float x, float y, float z, Simulation simulation)
    {
        GraphicsDevice = graphicsDevice;
        Simulation = simulation;
        Initialize(x, y, z);

        LoadContent(Content);

    }

    private void Initialize(float x, float y, float z)
    {

        float lastX = x;
        float lastY = y;
        float lastZ = z;
        Vector3 scale;
        Quaternion rot;
        Vector3 translation;

        // Plataforma 1
        Platform1World = Matrix.CreateScale(375f, 6f, 150f) * Matrix.CreateTranslation(lastX += 200f, lastY, lastZ);
        Platform1World.Decompose(out scale, out rot, out translation);
        Simulation.Statics.Add(new StaticDescription(Utils.ToNumericVector3(translation),
            Simulation.Shapes.Add( new Box(scale.X,scale.Y, scale.Z))));
        
        FloatingPlatformsWorld = new Matrix[]
        {
            Matrix.CreateScale(375f, 6f, 150f) * Matrix.CreateTranslation(lastX, lastY += 42f, lastZ += 180f),
            Matrix.CreateScale(300f, 6f, 150f) * Matrix.CreateTranslation(lastX, lastY += 42f, lastZ += 180f),
            Matrix.CreateScale(225f, 6f, 150f) * Matrix.CreateTranslation(lastX, lastY += 42f, lastZ += 180f),
            Matrix.CreateScale(150f, 6f, 150f) * Matrix.CreateTranslation(lastX, lastY += 42f, lastZ += 180f),
            Matrix.CreateScale(75f, 6f, 150f) * Matrix.CreateTranslation(lastX, lastY += 42f, lastZ += 180f),
            Matrix.CreateScale(75f, 6f, 75f) * Matrix.CreateTranslation(lastX, lastY += 30f, lastZ += 120f),
            Matrix.CreateScale(75f, 6f, 75f) * Matrix.CreateTranslation(lastX += 90f, lastY += 30f, lastZ),
            Matrix.CreateScale(75f, 6f, 75f) * Matrix.CreateTranslation(lastX, lastY += 30f, lastZ -= 90f),
            Matrix.CreateScale(75f, 6f, 75f) * Matrix.CreateTranslation(lastX, lastY += 30f, lastZ -= 90f),
            Matrix.CreateScale(75f, 6f, 75f) * Matrix.CreateTranslation(lastX -= 90f, lastY += 30f, lastZ),
            Matrix.CreateScale(75f, 6f, 30f) * Matrix.CreateTranslation(lastX -= 90f, lastY -= 30f, lastZ),
            Matrix.CreateScale(75f, 6f, 30f) * Matrix.CreateTranslation(lastX -= 120f, lastY -= 30f, lastZ),
            Matrix.CreateScale(75f, 6f, 30f) * Matrix.CreateTranslation(lastX -= 90f, lastY -= 30f, lastZ),
            Matrix.CreateScale(75f, 6f, 75f) * Matrix.CreateTranslation(lastX -= 90f, lastY -= 30f, lastZ),
            Matrix.CreateScale(75f, 6f, 30f) * Matrix.CreateTranslation(lastX, lastY -= 30f, lastZ += 120f),
            Matrix.CreateScale(75f, 6f, 30f) * Matrix.CreateTranslation(lastX, lastY -= 30f, lastZ += 120f),
            Matrix.CreateScale(75f, 6f, 30f) * Matrix.CreateTranslation(lastX, lastY -= 30f, lastZ += 120f),
            Matrix.CreateScale(24f, 6f, 24f) * Matrix.CreateTranslation(lastX += 36f, lastY, lastZ += 102f),
            Matrix.CreateScale(24f, 6f, 24f) * Matrix.CreateTranslation(lastX += 36f, lastY, lastZ += 72f),
            Matrix.CreateScale(24f, 6f, 24f) * Matrix.CreateTranslation(lastX -= 36f, lastY, lastZ += 72f),
            Matrix.CreateScale(24f, 6f, 24f) * Matrix.CreateTranslation(lastX -= 36f, lastY, lastZ += 72f),
            Matrix.CreateScale(24f, 6f, 24f) * Matrix.CreateTranslation(lastX -= 36f, lastY, lastZ += 72f),
            Matrix.CreateScale(24f, 6f, 24f) * Matrix.CreateTranslation(lastX, lastY += 18f, lastZ += 72f),
            Matrix.CreateScale(24f, 6f, 24f) * Matrix.CreateTranslation(lastX, lastY += 18f, lastZ += 72f),
            Matrix.CreateScale(15f, 6f, 15f) * Matrix.CreateTranslation(lastX, lastY += 18f, lastZ += 72f),
            Matrix.CreateScale(15f, 6f, 15f) * Matrix.CreateTranslation(lastX, lastY, lastZ += 72f),
            Matrix.CreateScale(15f, 6f, 15f) * Matrix.CreateTranslation(lastX, lastY, lastZ += 72f),
            Matrix.CreateScale(150f, 6f, 150f) * Matrix.CreateTranslation(lastX, lastY, lastZ += 200f)

            
        };
        
        for (int index = 0; index < FloatingPlatformsWorld.Length; index++)
        {
            var matrix = FloatingPlatformsWorld[index];
            matrix.Decompose(out scale, out rot, out translation);
            Simulation.Statics.Add(new StaticDescription(Utils.ToNumericVector3(translation),
                Simulation.Shapes.Add( new Box(scale.X,scale.Y, scale.Z))));

        }

    }

    private void LoadContent(ContentManager Content)
    {
        // Cargar Texturas
        CobbleTexture = Content.Load<Texture2D>(
                ConfigurationManager.AppSettings["ContentFolderTextures"] + "floor/adoquin");

        // Cargar Primitiva de caja con textura
        BoxPrimitive = new BoxPrimitive(GraphicsDevice, Vector3.One, CobbleTexture);/*  */
    }

    public void Draw(Matrix view, Matrix projection)
    {
        // Draw Platform1
        BoxPrimitive.Draw(Platform1World, view, projection);

        // Draw Floating Platforms
        for (int index = 0; index < FloatingPlatformsWorld.Length; index++)
        {
            var matrix = FloatingPlatformsWorld[index];
            BoxPrimitive.Draw(matrix, view, projection);

        }

        // Draw Platform1
        BoxPrimitive.Draw(FloatingMovingPlatformWorld, view, projection);
    }
}

