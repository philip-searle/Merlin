using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace HoverRenderer
{
    public struct FpsCamera
    {
        Vector3 position;
        Vector3 angles;

        public Matrix ViewMatrix
        {
            get
            {
                var rotateMatrix = Matrix.RotationYawPitchRoll(DegToRad(angles.X), DegToRad(angles.Y), DegToRad(angles.Z));
                var translateMatrix = Matrix.Translation(position);
                return Matrix.Multiply(rotateMatrix, translateMatrix);
            }
        }

        private static float DegToRad(float deg)
        {
            var fov = deg * (float)System.Math.PI / 180;
            return fov;
        }

        public void Turn(int degrees)
        {
            angles.Z += degrees;
            if (angles.Z > 180) angles.Z -= 360;
            if (angles.Z < -180) angles.Z += 360;
        }

        public void LookUp(int degrees)
        {
            angles.X += degrees;
        }

        public void MoveForward(int distance)
        {
            position += Vector3.Multiply(this.ForwardVector, distance);
        }

        public void MoveLeft(int distance)
        {
            position += Vector3.Multiply(this.LeftVector, distance);
        }

        public void MoveUp(int distance)
        {
            position.Z += distance;
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        private Vector3 ForwardVector
        {
            get
            {
                var rotationMatrix = Matrix.RotationZ(DegToRad(angles.Z));
                var vector = Vector3.TransformNormal(Vector3.UnitX, rotationMatrix);
                return vector;
            }
        }

        private Vector3 LeftVector
        {
            get
            {
                var rotationMatrix = Matrix.RotationZ(DegToRad(angles.Z));
                var vector = Vector3.TransformNormal(Vector3.UnitY, rotationMatrix);
                return vector;
            }
        }

        public Vector3 UpVector
        {
            get
            {
                var rotationMatrix = Matrix.RotationZ(DegToRad(angles.X));
                var vector = Vector3.TransformNormal(Vector3.UnitZ, rotationMatrix);
                return vector;
            }
        }

        public Vector3 LookAtPosition
        {
            get
            {
                var yawMatrix = Matrix.RotationZ(DegToRad(angles.Z));
                var pitchMatrix = Matrix.RotationX(DegToRad(angles.X));
                var lookAtNormal = Vector3.TransformCoordinate(new Vector3(100, 0, 0), yawMatrix);
                lookAtNormal = Vector3.TransformCoordinate(lookAtNormal, pitchMatrix);
                return Vector3.Add(position, new Vector3(lookAtNormal.X, lookAtNormal.Y, lookAtNormal.Z));
                /*var rotationMatrix = Matrix.RotationZ(DegToRad(angles.Z));
                var lookAtNormal = Vector3.Transform(position, rotationMatrix);
                return new Vector3(lookAtNormal.X, lookAtNormal.Y, lookAtNormal.Z);*/
            }
        }

        public Vector3 Angles
        {
            get { return angles; }
        }
    }
}
