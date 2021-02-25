using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Flashback_Game
{
    class Camera
    {
        private Vector3 position;
        private Vector3 lookAt;
        private Matrix viewMatrix;
        private Matrix projectionMatrix;
        private Matrix rotationMatrix;
        private float aspectRatio;

        public Vector3 thirdPersonReference = new Vector3(0, -8000, 3000);

        public Camera(Viewport viewport)
        {
            this.aspectRatio = ((float)viewport.Width) / ((float)viewport.Height);
            this.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), this.aspectRatio, 1.0f, 46000.0f);
        }

        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        public Vector3 LookAt
        {
            get { return this.lookAt; }
            set { this.lookAt = value; }
        }

        public float RotateCamera
        {

            set
            {
                rotationMatrix = Matrix.CreateRotationZ(value);

                // Create a vector pointing the direction the camera is facing.
                Vector3 transformedReference = Vector3.Transform(thirdPersonReference, rotationMatrix);

                //transformedReference.Z += MathHelper.PiOver2;

                // Calculate the position the camera is looking from.
                position = transformedReference + lookAt;

            }

        }

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
        }

        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        public void Update()
        {
            this.viewMatrix = Matrix.CreateLookAt(this.position, this.lookAt, Vector3.Backward);
        }


    }
}
