﻿using BepuPhysics;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGamers.Camera;
using MonoGamers.PowerUps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NumericVector3 = System.Numerics.Vector3;
using static MonoGamers.Utilities.Utils;
using BepuPhysics.Constraints.Contact;
using MonoGamers.Utilities;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace MonoGamers.Geometries
{
    internal class MonoSphere
    {

        //Tipo de esfera
        enum Type {
            Common,
            Stone,
            Metal,
            Gum
        }

        Type SphereType;
        private const float StandardSideSpeed = 500f;
        private const float standardJumpSpeed = 55000f;
        private const float Friction = 0.5f;
        //private const float maxSpeed = 150f;


        public float SphereSideSpeed;
        public float SphereJumpSpeed;

        public bool rushed;

        public SpherePrimitive SpherePrimitive { get; set; }

        // Sphere internal matrices and vectors
        public Matrix SphereRotation { get; set; }
        public Vector3 SpherePosition { get; set; }
        public Vector3 SphereVelocity { get; set; }
        public Vector3 SphereAcceleration { get; set; }
        public Vector3 SphereFrontDirection { get; set; }
        public Vector3 SphereLateralDirection { get; set; }
        public Matrix SphereWorld { get; set; }


        // A boolean indicating if the Sphere is on the ground
        public bool OnGround { get; set; }

        // Textures
        public Texture2D SphereCommonTexture { get; set; }
        public Texture2D SphereStoneTexture { get; set; }
        public Texture2D SphereMetalTexture { get; set; }
        public Texture2D SphereGumTexture { get; set; }

        // Effect
        public Effect SphereEffect { get; set; }
        

        //handler
        public BodyHandle SphereHandle { get; set; }

        public Sphere SphereShape { get; set; }

        public float velocidadAngularYAnt;
        public float velocidadLinearYAnt;

        private float SphereSideTypeSpeed;
        private float SphereJumpTypeSpeed;
        
        private bool godMode = false;

        public MonoSphere(Vector3 InitialPosition, float Gravity, Simulation Simulation)
        {
            OnGround = true;

            SphereWorld = new Matrix();
            SphereSideSpeed = StandardSideSpeed;
            SphereJumpSpeed = standardJumpSpeed;

            SpherePosition = InitialPosition;
            SphereRotation = Matrix.Identity;
            SphereFrontDirection = Vector3.Backward;
            SphereLateralDirection = Vector3.Right;
            // Set the Acceleration (which in this case won't change) to the Gravity pointing down
            SphereAcceleration = Vector3.Down * Gravity;

            SphereHandle = new BodyHandle();
            
            SphereShape = new Sphere(10f);
            var position = Utils.ToNumericVector3(SpherePosition);
            var initialVelocity = new BodyVelocity(new NumericVector3((float)0f, 0f, 0f));
            var mass = SphereShape.Radius * SphereShape.Radius * SphereShape.Radius ;
            var bodyDescription = BodyDescription.CreateConvexDynamic(position, initialVelocity, mass, Simulation.Shapes, SphereShape);
            SphereHandle = Simulation.Bodies.Add(bodyDescription);

            // Initialize the Velocity as zero
            SphereVelocity = Vector3.Zero;
        }

        public void Update(Simulation Simulation, TargetCamera Camera, KeyboardState KeyboardState)
        {
            var sphereBody = Simulation.Bodies.GetBodyReference(SphereHandle);
            sphereBody.Awake = true;
            SphereRotation = Camera.CameraRotation;
            SphereFrontDirection = Vector3.Transform(Vector3.Backward, SphereRotation);
            SphereLateralDirection = Vector3.Transform(Vector3.Right, SphereRotation);
            
            
            
            
            if(SphereType == Type.Common){
                SphereSideTypeSpeed = 1f;
                SphereJumpTypeSpeed = 1f;
            }

            if(SphereType == Type.Metal){
                SphereSideTypeSpeed = 2f;
                SphereJumpTypeSpeed = 1f;
            }

            if(SphereType == Type.Gum){
                SphereSideTypeSpeed = 1f;
                SphereJumpTypeSpeed = 2f;
            }

            if(SphereType == Type.Stone){
                SphereSideTypeSpeed = 0.5f;
                SphereJumpTypeSpeed = 0.75f;
            }

            //Cambio de tipo de esfera manualmente
            
            if (KeyboardState.IsKeyUp(Keys.G)) {
                if (!godMode) godMode = true;
                else godMode = false;
            }
            
            if (KeyboardState.IsKeyDown(Keys.T) && godMode){
                SphereType = Type.Common;
            }
            if (KeyboardState.IsKeyDown(Keys.Y) && godMode){
                SphereType = Type.Metal;
            }
            if (KeyboardState.IsKeyDown(Keys.U) && godMode){
                SphereType = Type.Gum;
            }
            if (KeyboardState.IsKeyDown(Keys.I) && godMode){
                SphereType = Type.Stone;
            }
            
            if (KeyboardState.IsKeyDown(Keys.D1) && godMode) {
                sphereBody.Pose = new NumericVector3(100f, 10f, 160f);
            }            
            if (KeyboardState.IsKeyDown(Keys.D2) && godMode) {
                sphereBody.Pose = new NumericVector3(100f, 20f, 4580f);
            }
            if (KeyboardState.IsKeyDown(Keys.D3) && godMode) {
                sphereBody.Pose = new NumericVector3(2090f, 150f, 6744f);
            }
            if (KeyboardState.IsKeyDown(Keys.D4) && godMode) {
                sphereBody.Pose = new NumericVector3(3400f, 343f, 6790f);
            } 

            if (KeyboardState.IsKeyDown(Keys.D)) LateralMove( -1f);
            if (KeyboardState.IsKeyDown(Keys.A)) LateralMove(1f);
            if (KeyboardState.IsKeyDown(Keys.W)) FrontalMove( 1f);
            if (KeyboardState.IsKeyDown(Keys.S)) FrontalMove( -1f);

            if (MathHelper.Distance(sphereBody.Velocity.Linear.Y, velocidadLinearYAnt) < 0.1
                    && MathHelper.Distance(sphereBody.Velocity.Angular.Y, velocidadAngularYAnt) < 0.1)
                OnGround = true; // Se revisa que la velocidad lineal como la angular de la esfera en Y, su distancia se menor a 0,1 con respecto a la velocidad anterior

            if (KeyboardState.IsKeyDown(Keys.Space) && OnGround) Jump();

            /*if (KeyboardState.GetPressedKeys().Length > 0) ApplyImpulse(ref sphereBody, 2f);
            ApplyStop(ref sphereBody);*/
            ApplyImpulse(ref sphereBody, 2f);

            if (rushed) ApplyRush(ref sphereBody);

            velocidadAngularYAnt = sphereBody.Velocity.Angular.Y;
            velocidadLinearYAnt = sphereBody.Velocity.Linear.Y;

            var pose = Simulation.Bodies.GetBodyReference(SphereHandle).Pose;
            SpherePosition = pose.Position;
            SphereWorld = Matrix.CreateScale(SphereShape.Radius*2) *
                Matrix.CreateFromQuaternion(pose.Orientation) * Matrix.CreateTranslation(SpherePosition);

            SphereVelocity = Vector3.Zero;
        }
        

        private void LateralMove( float Sense)
        {
            //var speedOntoDirectionSense = (ActualSpeed * Sense * SphereLateralDirection * SphereLateralDirection / SphereLateralDirection.Length()).Length();

            if (OnGround)
            {
                SphereVelocity = Sense * SphereLateralDirection * SphereSideSpeed * SphereSideTypeSpeed;
            }
            else
            {
                SphereVelocity = Sense * SphereLateralDirection * SphereSideSpeed * SphereSideTypeSpeed *0.2f; 
            }
        }

        private void FrontalMove(float Sense)
        {
            /*var speedOntoDirection = (sphereBody.Velocity.Linear * Sense * SphereFrontDirection * SphereFrontDirection / (SphereFrontDirection.Length() * SphereFrontDirection.Length())).Length();*/

            if (OnGround)
            {
                SphereVelocity = Sense * SphereFrontDirection * SphereSideSpeed * SphereSideTypeSpeed;
            }
            else
            {
                SphereVelocity = Sense * SphereFrontDirection * SphereSideSpeed * SphereSideTypeSpeed * 0.2f; 
            }
                
            /*if ( MathHelper.ToDegrees((float)Math.Acos(Vector2.Dot(ActualSpeedV2, SphereFrontDirectionV2) / (ActualSpeedV2.Length() * SphereFrontDirectionV2.Length() ))) < 180f){
                
                ApplyImpulse(ref sphereBody, 2f);
            }
            else if(MathHelper.ToDegrees((float)Math.Acos(Vector2.Dot(ActualSpeedV2, SphereFrontDirectionV2) / (ActualSpeedV2.Length() * SphereFrontDirectionV2.Length() ))) >= 180f)
            {
                ApplyImpulse(ref sphereBody, 100f);
            }
                
            }*/

            
        }

        public void Jump()
        {
            SphereVelocity += Vector3.Up * SphereJumpSpeed * SphereJumpTypeSpeed;
            OnGround = false;
        }

        public void ApplyImpulse(ref BodyReference sphereBody, float intensity)
        {
            sphereBody.ApplyLinearImpulse(new NumericVector3(SphereVelocity.X * intensity,
                SphereVelocity.Y * intensity,
                SphereVelocity.Z * intensity));
        }
        public void ApplyStop(ref BodyReference sphereBody)
        {
            if (sphereBody.MotionState.Velocity.Linear.LengthSquared() > 0.0001f)
                sphereBody.ApplyLinearImpulse(-sphereBody.MotionState.Velocity.Linear * Friction);
        } 
        
        public void ApplyRush(ref BodyReference sphereBody)
        {
            ApplyImpulse(ref sphereBody, 10f);
            rushed = false;
        }


        public bool SphereFalling(float LimitY)
        {
            return SpherePosition.Y < LimitY;
        }

        public void Draw(Camera.Camera camera){
            
            SphereEffect.CurrentTechnique = SphereEffect.Techniques["BasicColorDrawing"];
            SphereEffect.Parameters["View"].SetValue(camera.View);
            SphereEffect.Parameters["Projection"].SetValue(camera.Projection);
            SphereEffect.Parameters["World"].SetValue(SphereWorld);
            
            if(SphereType == Type.Common) {
                SphereEffect.Parameters["ModelTexture"].SetValue(SphereCommonTexture);
            }
            if(SphereType == Type.Gum) {
                SphereEffect.Parameters["ModelTexture"].SetValue(SphereGumTexture);
            }
            if(SphereType == Type.Metal) {
                SphereEffect.Parameters["ModelTexture"].SetValue(SphereMetalTexture);
            }
            if(SphereType == Type.Stone) {
                SphereEffect.Parameters["ModelTexture"].SetValue(SphereStoneTexture);
            }
            
            SpherePrimitive.Draw(SphereEffect);
        }
    }


}