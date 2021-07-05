using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace g3
{
    [MessagePackObject]
    public struct Vector3b : IComparable<Vector3b>, IEquatable<Vector3b>
    {
        [Key(0)]
        public bool x;
        [Key(1)]
        public bool y;
        [Key(2)]
        public bool z;

        public Vector3b(bool f) { x = y = z = f; }
        public Vector3b(bool x, bool y,bool z) { this.x = x; this.y = y; this.z = z; }
        public Vector3b(bool[] v3) { x = v3[0]; y = v3[1]; z = v3[2]; }


        static public readonly Vector3b True = new Vector3b(true, true,true);
        static public readonly Vector3b False = new Vector3b(false, false,false);

        public bool this[int key]
        {
            get { return (key == 0) ? x : y; }
            set { if (key == 0) x = value; else y = value; }
        }





        public void Set(Vector2b o) {
            x = o.x; y = o.y;
        }
        public void Set(bool fX, bool fY) {
            x = fX; y = fY;
        }



        public static bool operator ==(Vector3b a, Vector3b b)
        {
            return (a.x == b.x && a.y == b.y&& a.z == b.z);
        }
        public static bool operator !=(Vector3b a, Vector3b b)
        {
            return (a.x != b.x || a.y != b.y && a.z != b.z);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector3b)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int) 2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                hash = (hash * 16777619) ^ z.GetHashCode();

                return hash;
            }
        }
        public int CompareTo(Vector3b other)
        {
            return 0;
        }
        public bool Equals(Vector3b other)
        {
            return (x == other.x && y == other.y && z == other.z);
        }


        public override string ToString() {
            return string.Format("{0} {1} {2}", x, y,z);
        }


    }

}