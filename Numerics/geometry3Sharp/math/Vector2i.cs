﻿using MessagePack;

using System;

namespace RNumerics
{
    [MessagePackObject]
    public struct Vector2i : IComparable<Vector2i>, IEquatable<Vector2i>, IConvertible
    {
        [Key(0)]
        public int x;
        [Key(1)]
        public int y;

        public Vector2i(int f) { x = y = f; }
        public Vector2i(int x, int y) { this.x = x; this.y = y; }
        public Vector2i(int[] v2) { x = v2[0]; y = v2[1]; }
        [IgnoreMember]
		public static readonly Vector2i Zero = new(0, 0);
        [IgnoreMember]
        static public readonly Vector2i One = new(1, 1);
        [IgnoreMember]
        static public readonly Vector2i AxisX = new(1, 0);
        [IgnoreMember]
        static public readonly Vector2i AxisY = new(0, 1);

        [IgnoreMember]
        public int this[int key]
		{
			get { return (key == 0) ? x : y; }
			set { if (key == 0) { x = value; } else
                {
                    y = value;
                }
            }
		}

        [IgnoreMember]
        public int[] Array
		{
			get { return new int[] { x, y }; }
		}

		public void Add(int s) { x += s; y += s; }


        [IgnoreMember]
        public int LengthSquared { get { return (x * x) + (y * y); } }


        public static Vector2i operator -(Vector2i v)
		{
			return new Vector2i(-v.x, -v.y);
		}

		public static Vector2i operator *(int f, Vector2i v)
		{
			return new Vector2i(f * v.x, f * v.y);
		}
		public static Vector2i operator *(Vector2i v, int f)
		{
			return new Vector2i(f * v.x, f * v.y);
		}
		public static Vector2i operator /(Vector2i v, int f)
		{
			return new Vector2i(v.x / f, v.y / f);
		}
		public static Vector2i operator /(int f, Vector2i v)
		{
			return new Vector2i(f / v.x, f / v.y);
		}

		public static Vector2i operator *(Vector2i a, Vector2i b)
		{
			return new Vector2i(a.x * b.x, a.y * b.y);
		}
		public static Vector2i operator /(Vector2i a, Vector2i b)
		{
			return new Vector2i(a.x / b.x, a.y / b.y);
		}


		public static Vector2i operator +(Vector2i v0, Vector2i v1)
		{
			return new Vector2i(v0.x + v1.x, v0.y + v1.y);
		}
		public static Vector2i operator +(Vector2i v0, int f)
		{
			return new Vector2i(v0.x + f, v0.y + f);
		}

		public static Vector2i operator -(Vector2i v0, Vector2i v1)
		{
			return new Vector2i(v0.x - v1.x, v0.y - v1.y);
		}
		public static Vector2i operator -(Vector2i v0, int f)
		{
			return new Vector2i(v0.x - f, v0.y - f);
		}



		public static bool operator ==(Vector2i a, Vector2i b)
		{
			return a.x == b.x && a.y == b.y;
		}
		public static bool operator !=(Vector2i a, Vector2i b)
		{
			return a.x != b.x || a.y != b.y;
		}
		public override bool Equals(object obj)
		{
			return this == (Vector2i)obj;
		}
		public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public int CompareTo(Vector2i other)
		{
			if (x != other.x)
            {
                return x < other.x ? -1 : 1;
            }
            else if (y != other.y)
            {
                return y < other.y ? -1 : 1;
            }

            return 0;
		}
		public bool Equals(Vector2i other)
		{
			return x == other.x && y == other.y;
		}



		public override string ToString()
		{
			return string.Format("{0} {1}", x, y);
		}

		public TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public byte ToByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public short ToInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public int ToInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public long ToInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public float ToSingle(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider)
		{
			return ToString();
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ulong ToUInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}
	}












	public struct Vector2l : IComparable<Vector2l>, IEquatable<Vector2l>
	{
		public long x;
		public long y;

		public Vector2l(long f) { x = y = f; }
		public Vector2l(long x, long y) { this.x = x; this.y = y; }
		public Vector2l(long[] v2) { x = v2[0]; y = v2[1]; }

		static public readonly Vector2l Zero = new(0, 0);
		static public readonly Vector2l One = new(1, 1);
		static public readonly Vector2l AxisX = new(1, 0);
		static public readonly Vector2l AxisY = new(0, 1);

		public long this[long key]
		{
			get { return (key == 0) ? x : y; }
			set { if (key == 0) { x = value; } else
                {
                    y = value;
                }
            }
		}

		public long[] Array
		{
			get { return new long[] { x, y }; }
		}

		public void Add(long s) { x += s; y += s; }




		public static Vector2l operator -(Vector2l v)
		{
			return new Vector2l(-v.x, -v.y);
		}

		public static Vector2l operator *(long f, Vector2l v)
		{
			return new Vector2l(f * v.x, f * v.y);
		}
		public static Vector2l operator *(Vector2l v, long f)
		{
			return new Vector2l(f * v.x, f * v.y);
		}
		public static Vector2l operator /(Vector2l v, long f)
		{
			return new Vector2l(v.x / f, v.y / f);
		}
		public static Vector2l operator /(long f, Vector2l v)
		{
			return new Vector2l(f / v.x, f / v.y);
		}

		public static Vector2l operator *(Vector2l a, Vector2l b)
		{
			return new Vector2l(a.x * b.x, a.y * b.y);
		}
		public static Vector2l operator /(Vector2l a, Vector2l b)
		{
			return new Vector2l(a.x / b.x, a.y / b.y);
		}


		public static Vector2l operator +(Vector2l v0, Vector2l v1)
		{
			return new Vector2l(v0.x + v1.x, v0.y + v1.y);
		}
		public static Vector2l operator +(Vector2l v0, long f)
		{
			return new Vector2l(v0.x + f, v0.y + f);
		}

		public static Vector2l operator -(Vector2l v0, Vector2l v1)
		{
			return new Vector2l(v0.x - v1.x, v0.y - v1.y);
		}
		public static Vector2l operator -(Vector2l v0, long f)
		{
			return new Vector2l(v0.x - f, v0.y - f);
		}



		public static bool operator ==(Vector2l a, Vector2l b)
		{
			return a.x == b.x && a.y == b.y;
		}
		public static bool operator !=(Vector2l a, Vector2l b)
		{
            return a.x != b.x || a.y != b.y;
		}
		public override bool Equals(object obj)
		{
			return this == (Vector2l)obj;
		}
		public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public int CompareTo(Vector2l other)
		{
			if (x != other.x)
            {
                return x < other.x ? -1 : 1;
            }
            else if (y != other.y)
            {
                return y < other.y ? -1 : 1;
            }

            return 0;
		}
		public bool Equals(Vector2l other)
		{
			return x == other.x && y == other.y;
		}



		public override string ToString()
		{
			return string.Format("{0} {1}", x, y);
		}
	}



}
