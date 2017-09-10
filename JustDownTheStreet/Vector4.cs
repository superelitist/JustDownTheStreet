// Big thanks to Reazer for reminding me of creating this!
// This is a basic template for making scripts for GTA5. I may or may not make a program to do this,
// But since jedijosh920 already made a program to do this, I might not. However, enjoy the template!
//
// The readme in the downloaded zip folder explains how to use this, and how to import into GTA5. Enjoy!
//

using GTA.Math;

namespace JustDownTheStreet
{
    internal struct Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float H { get; set; }

        public Vector4(float x, float y, float z, float h)
        {
            X = x;
            Y = y;
            Z = z;
            H = h;
        }

        public Vector4(Vector3 vector3, float h)
        {
            X = vector3.X;
            Y = vector3.Y;
            Z = vector3.Z;
            H = h;
        }
    }
}
