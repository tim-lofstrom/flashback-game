using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Flashback_Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KeyboardState currentKeyState;

        int score;
        SpriteFont kootenay;
        Vector2 scorePosition = new Vector2(100, 50);

        Texture2D stars;

        //Camera/View information
        Camera camera;

        //Audio Components
        SoundEffect soundEngine;
        SoundEffectInstance soundEngineInstance;
        SoundEffect soundHyperspaceActivation;
        SoundEffect soundExplosion2;
        SoundEffect soundExplosion3;
        SoundEffect soundWeaponsFire;

        //Visual components
        Ship ship = new Ship();

        Model bulletModel;
        Matrix[] bulletTransforms;
        Bullet[] bulletList = new Bullet[GameConstants.NumBullets];

        Model asteroidModel;
        Matrix[] asteroidTransforms;
        Asteroid[] asteroidList = new Asteroid[GameConstants.NumAsteroids];
        Random random = new Random();

        KeyboardState oldState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();

            camera = new Camera(graphics.GraphicsDevice.Viewport);

            ResetAsteroids();

            base.Initialize();
        }

        private void ResetAsteroids()
        {
            float xStart;
            float yStart;
            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                asteroidList[i].isActive = true;
                if (random.Next(2) == 0)
                {
                    xStart = (float)-GameConstants.PlayfieldSizeX;
                }
                else
                {
                    xStart = (float)GameConstants.PlayfieldSizeX;
                }
                yStart =
                    (float)random.NextDouble() * GameConstants.PlayfieldSizeY;
                asteroidList[i].position = new Vector3(xStart, yStart, 0.0f);
                double angle = random.NextDouble() * 2 * Math.PI;
                asteroidList[i].direction.X = -(float)Math.Sin(angle);
                asteroidList[i].direction.Y = (float)Math.Cos(angle);
                asteroidList[i].speed = GameConstants.AsteroidMinSpeed +
                   (float)random.NextDouble() * GameConstants.AsteroidMaxSpeed;
            }
        }

        private Matrix[] SetupEffectDefaults(Model myModel)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    camera.LookAt = ship.Position;
                    camera.RotateCamera = ship.Rotation;
                    camera.Update();

                    effect.EnableDefaultLighting();
                    effect.Projection = camera.ProjectionMatrix;
                    effect.View = camera.ViewMatrix;
                }
            }
            return absoluteTransforms;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ship.Model = Content.Load<Model>("Models/p1_wedge");
            ship.Transforms = SetupEffectDefaults(ship.Model);
            soundEngine = Content.Load<SoundEffect>("Audio/Waves/engine_2");
            soundEngineInstance = soundEngine.CreateInstance();
            soundHyperspaceActivation = Content.Load<SoundEffect>("Audio/Waves/hyperspace_activate");
            asteroidModel = Content.Load<Model>("Models/asteroid1");
            asteroidTransforms = SetupEffectDefaults(asteroidModel);
            soundExplosion2 = Content.Load<SoundEffect>("Audio/Waves/explosion2");
            soundExplosion3 = Content.Load<SoundEffect>("Audio/Waves/explosion3");
            soundWeaponsFire = Content.Load<SoundEffect>("Audio/Waves/tx0_fire1");
            bulletModel = Content.Load<Model>("Models/pea_proj");
            bulletTransforms = SetupEffectDefaults(bulletModel);
            kootenay = Content.Load<SpriteFont>("Fonts/Lucida Console");
            stars = Content.Load<Texture2D>("Textures/B1_stars");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();


            // Get some input.
            UpdateInput();

            // Add velocity to the current position.
            ship.Position += ship.Velocity;

            // Bleed off velocity over time.
            ship.Velocity *= 0.95f;

            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                asteroidList[i].Update(timeDelta);
            }

            //bullet-asteroid collision check
            for (int i = 0; i < asteroidList.Length; i++)
            {
                if (asteroidList[i].isActive)
                {
                    BoundingSphere asteroidSphere =
                      new BoundingSphere(asteroidList[i].position,
                               asteroidModel.Meshes[0].BoundingSphere.Radius *
                                     GameConstants.AsteroidBoundingSphereScale);
                    for (int j = 0; j < bulletList.Length; j++)
                    {
                        if (bulletList[j].isActive)
                        {
                            BoundingSphere bulletSphere = new BoundingSphere(
                              bulletList[j].position,
                              bulletModel.Meshes[0].BoundingSphere.Radius);
                            if (asteroidSphere.Intersects(bulletSphere))
                            {
                                soundExplosion2.Play();
                                asteroidList[i].isActive = false;
                                bulletList[j].isActive = false;
                                score += GameConstants.KillBonus;
                                break; //no need to check other bullets
                            }
                        }
                    }
                }
            }

            //ship-asteroid collision check
            if (ship.isActive)
            {
                BoundingSphere shipSphere = new BoundingSphere(
                ship.Position, ship.Model.Meshes[0].BoundingSphere.Radius *
                GameConstants.ShipBoundingSphereScale);
                for (int i = 0; i < asteroidList.Length; i++)
                {
                    if(asteroidList[i].isActive)
                    {
                        BoundingSphere b = new BoundingSphere(asteroidList[i].position, asteroidModel.Meshes[0].BoundingSphere.Radius * GameConstants.AsteroidBoundingSphereScale);
                        if (b.Intersects(shipSphere))
                        {
                            //blow up ship
                            soundExplosion3.Play();
                            ship.isActive = false;
                            asteroidList[i].isActive = false;
                            score -= GameConstants.DeathPenalty;
                            break; //exit the loop
                        }
                    }

                } 
            }

            for (int i = 0; i < GameConstants.NumBullets; i++)
            {
                if (bulletList[i].isActive)
                {
                    bulletList[i].Update(timeDelta);
                }
            }

            base.Update(gameTime);
        }

        protected void UpdateInput()
        {
            // Get the game pad state.

            currentKeyState = Keyboard.GetState();

            ship.Update(currentKeyState);

            if((currentKeyState.IsKeyDown(Keys.R) && oldState.IsKeyUp(Keys.R)))
            {
                ResetAsteroids();
            }

            //Play engine sound only when the engine is on.
            if (currentKeyState.IsKeyDown(Keys.W))
            {
                if (!(ship.speed > 1.5f))
                {
                    ship.speed++;
                }
                

                if (soundEngineInstance.State == SoundState.Stopped)
                {
                    soundEngineInstance.Volume = 1.0f;
                    soundEngineInstance.IsLooped = true;
                    soundEngineInstance.Play();
                }
                else
                    soundEngineInstance.Resume();
            }
            else if (!currentKeyState.IsKeyDown(Keys.W))
            {
                if (!(ship.speed < 1))
                {
                    ship.speed--;
                }

                if (soundEngineInstance.State == SoundState.Playing)
                    soundEngineInstance.Pause();
            }



            if(currentKeyState.IsKeyDown(Keys.LeftShift))
            {
                ship.burner = 2;
            }
            else if (!currentKeyState.IsKeyDown(Keys.LeftShift))
            {
                ship.burner = 1;
            }

            //are we shooting?
            if (ship.isActive && currentKeyState.IsKeyDown(Keys.Space))
            {
                //add another bullet.  Find an inactive bullet slot and use it
                //if all bullets slots are used, ignore the user input
                for (int i = 0; i < GameConstants.NumBullets; i++)
                {
                    if (!bulletList[i].isActive)
                    {
                        bulletList[i].direction = ship.RotationMatrix.Forward;
                        if (ship.speed > 1) { bulletList[i].speed = GameConstants.BulletSpeedAdjustment * ship.speed; }
                        if (ship.speed == 0) { bulletList[i].speed = GameConstants.BulletSpeedAdjustment; }
                        bulletList[i].position = ship.Position + (200 * bulletList[i].direction);
                        bulletList[i].isActive = true;
                        soundWeaponsFire.Play();
                        score -= GameConstants.ShotPenalty;
                        break; //exit the loop
                    }
                }
            }



            // In case you get lost, press A to warp back to the center.
            if ((currentKeyState.IsKeyDown(Keys.Enter)) && oldState.IsKeyUp(Keys.Enter))
            {
                ship.Position = Vector3.Zero;
                ship.Velocity = Vector3.Zero;
                ship.Rotation = 0.0f;
                ship.isActive = true;
                score -= GameConstants.WarpPenalty;
                soundHyperspaceActivation.Play();
            }

            oldState = currentKeyState;
            
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            //spriteBatch.Draw(stars, new Rectangle(0, 0, 1200, 800), Color.White);
            spriteBatch.DrawString(kootenay, "Score: " + score, scorePosition, Color.LightGreen);
            spriteBatch.End();

            if (ship.isActive)
            {
                Matrix shipTransformMatrix = ship.RotationMatrix
                * Matrix.CreateTranslation(ship.Position);
                ship.Transforms = SetupEffectDefaults(ship.Model);
                DrawModel(ship.Model, shipTransformMatrix, ship.Transforms);
            }

            for (int i = 0; i < GameConstants.NumAsteroids; i++)
            {
                if(asteroidList[i].isActive)
                {
                    Matrix asteroidTransform =
                    Matrix.CreateTranslation(asteroidList[i].position);
                    asteroidTransforms = SetupEffectDefaults(asteroidModel);
                    DrawModel(asteroidModel, asteroidTransform, asteroidTransforms);
                }
            }

            for (int i = 0; i < GameConstants.NumBullets; i++)
            {
                if (bulletList[i].isActive)
                {
                    Matrix bulletTransform =
                      Matrix.CreateTranslation(bulletList[i].position);
                    bulletTransforms = SetupEffectDefaults(bulletModel);
                    DrawModel(bulletModel, bulletTransform, bulletTransforms);
                }
            }

            base.Draw(gameTime);
        }

        public static void DrawModel(Model model, Matrix modelTransform, Matrix[] absoluteBoneTransforms)
        {

            //Draw the model, a model can have multiple meshes, so loop
            foreach (ModelMesh mesh in model.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World =
                        absoluteBoneTransforms[mesh.ParentBone.Index] *
                        modelTransform;
                }
                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }
    }
}
