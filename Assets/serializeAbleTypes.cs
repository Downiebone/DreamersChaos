using System;
using UnityEngine;

namespace SerializableTypes
{
    /// <summary> Serializable version of UnityEngine.Vector3. </summary>
    [Serializable]
    public struct SVector3Int
    {
        public int x;
        public int y;
        public int z;

        public SVector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public SVector3Int(Vector3Int vecInt)
        {
            this.x = vecInt.x;
            this.y = vecInt.y;
            this.z = vecInt.z;
        }

        public override string ToString()
            => $"[x, y, z]";

        public static implicit operator Vector3Int(SVector3Int s)
            => new Vector3Int(s.x, s.y, s.z);

        public static implicit operator SVector3Int(Vector3 v)
            => new SVector3Int((int)v.x, (int)v.y, (int)v.z);


        //public static SVector3 operator +(SVector3 a, SVector3 b)
        //    => new SVector3(a.x + b.x, a.y + b.y, a.z + b.z);

        //public static SVector3 operator -(SVector3 a, SVector3 b)
        //    => new SVector3(a.x - b.x, a.y - b.y, a.z - b.z);

        //public static SVector3 operator -(SVector3 a)
        //    => new SVector3(-a.x, -a.y, -a.z);

        //public static SVector3 operator *(SVector3 a, float m)
        //    => new SVector3(a.x * m, a.y * m, a.z * m);

        //public static SVector3 operator *(float m, SVector3 a)
        //    => new SVector3(a.x * m, a.y * m, a.z * m);

        //public static SVector3 operator /(SVector3 a, float d)
        //    => new SVector3(a.x / d, a.y / d, a.z / d);
    }
    [Serializable]
    public struct SVector3
    {
        public float x;
        public float y;
        public float z;

        public SVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
            => $"[x, y, z]";

        public static implicit operator Vector3(SVector3 s)
            => new Vector3(s.x, s.y, s.z);

        public static implicit operator SVector3(Vector3 v)
            => new SVector3(v.x, v.y, v.z);


        //public static SVector3 operator +(SVector3 a, SVector3 b)
        //    => new SVector3(a.x + b.x, a.y + b.y, a.z + b.z);

        //public static SVector3 operator -(SVector3 a, SVector3 b)
        //    => new SVector3(a.x - b.x, a.y - b.y, a.z - b.z);

        //public static SVector3 operator -(SVector3 a)
        //    => new SVector3(-a.x, -a.y, -a.z);

        //public static SVector3 operator *(SVector3 a, float m)
        //    => new SVector3(a.x * m, a.y * m, a.z * m);

        //public static SVector3 operator *(float m, SVector3 a)
        //    => new SVector3(a.x * m, a.y * m, a.z * m);

        //public static SVector3 operator /(SVector3 a, float d)
        //    => new SVector3(a.x / d, a.y / d, a.z / d);
    }
    /// <summary> Serializable version of UnityEngine.Color32 without transparency. </summary>
    [Serializable]
    public struct SColor
    {
        public float r;
        public float g;
        public float b;

        public SColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public SColor(Color c)
        {
            r = c.r;
            g = c.g;
            b = c.b;
        }

        public override string ToString()
            => $"[{r}, {g}, {b}]";

        public static implicit operator Color(SColor rValue)
            => new Color(rValue.r, rValue.g, rValue.b, a: byte.MaxValue);

        public static implicit operator SColor(Color rValue)
            => new SColor(rValue.r, rValue.g, rValue.b);
    }
}