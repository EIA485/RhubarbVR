﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Veldrid;
using System.Runtime.CompilerServices;
using Veldrid.Utilities;

namespace RhubarbEngine.Utilities
{
	public unsafe struct BoundingFrustum
	{
		private SixPlane _planes;

		private struct SixPlane
		{
			public Plane Left;
			public Plane Right;
			public Plane Bottom;
			public Plane Top;
			public Plane Near;
			public Plane Far;
		}

		public BoundingFrustum(Matrix4x4 m)
		{
			// Plane computations: http://gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf
			_planes.Left = Plane.Normalize(
				new Plane(
					m.M14 + m.M11,
					m.M24 + m.M21,
					m.M34 + m.M31,
					m.M44 + m.M41));

			_planes.Right = Plane.Normalize(
				new Plane(
					m.M14 - m.M11,
					m.M24 - m.M21,
					m.M34 - m.M31,
					m.M44 - m.M41));

			_planes.Bottom = Plane.Normalize(
				new Plane(
					m.M14 + m.M12,
					m.M24 + m.M22,
					m.M34 + m.M32,
					m.M44 + m.M42));

			_planes.Top = Plane.Normalize(
				new Plane(
					m.M14 - m.M12,
					m.M24 - m.M22,
					m.M34 - m.M32,
					m.M44 - m.M42));

			_planes.Near = Plane.Normalize(
				new Plane(
					m.M13,
					m.M23,
					m.M33,
					m.M43));

			_planes.Far = Plane.Normalize(
				new Plane(
					m.M14 - m.M13,
					m.M24 - m.M23,
					m.M34 - m.M33,
					m.M44 - m.M43));
		}

		public BoundingFrustum(Plane left, Plane right, Plane bottom, Plane top, Plane near, Plane far)
		{
			_planes.Left = left;
			_planes.Right = right;
			_planes.Bottom = bottom;
			_planes.Top = top;
			_planes.Near = near;
			_planes.Far = far;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ContainmentType Contains(Vector3 point)
		{
			var planes = (Plane*)Unsafe.AsPointer(ref _planes); // Is this safe?

			for (var i = 0; i < 6; i++)
			{
				if (Plane.DotCoordinate(planes[i], point) < 0)
				{
					return ContainmentType.Disjoint;
				}
			}

			return ContainmentType.Contains;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ContainmentType Contains(Vector3* point)
		{
			var planes = (Plane*)Unsafe.AsPointer(ref _planes); // Is this safe?

			for (var i = 0; i < 6; i++)
			{
				if (Plane.DotCoordinate(planes[i], *point) < 0)
				{
					return ContainmentType.Disjoint;
				}
			}

			return ContainmentType.Contains;
		}

		public ContainmentType Contains(BoundingSphere sphere)
		{
			var planes = (Plane*)Unsafe.AsPointer(ref _planes);

			var result = ContainmentType.Contains;
			for (var i = 0; i < 6; i++)
			{
				var distance = Plane.DotCoordinate(planes[i], sphere.Center);
				if (distance < -sphere.Radius)
				{
					return ContainmentType.Disjoint;
				}
				else if (distance < sphere.Radius)
				{
					result = ContainmentType.Intersects;
				}
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ContainmentType Contains(BoundingBox box) => Contains(ref box);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ContainmentType Contains(ref BoundingBox box)
		{
			var planes = (Plane*)Unsafe.AsPointer(ref _planes);

			var result = ContainmentType.Contains;
			for (var i = 0; i < 6; i++)
			{
				var plane = planes[i];

				// Approach: http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

				var positive = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
				var negative = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);

				if (plane.Normal.X >= 0)
				{
					positive.X = box.Max.X;
					negative.X = box.Min.X;
				}
				if (plane.Normal.Y >= 0)
				{
					positive.Y = box.Max.Y;
					negative.Y = box.Min.Y;
				}
				if (plane.Normal.Z >= 0)
				{
					positive.Z = box.Max.Z;
					negative.Z = box.Min.Z;
				}

				// If the positive vertex is outside (behind plane), the box is disjoint.
				var positiveDistance = Plane.DotCoordinate(plane, positive);
				if (positiveDistance < 0)
				{
					return ContainmentType.Disjoint;
				}

				// If the negative vertex is outside (behind plane), the box is intersecting.
				// Because the above check failed, the positive vertex is in front of the plane,
				// and the negative vertex is behind. Thus, the box is intersecting this plane.
				var negativeDistance = Plane.DotCoordinate(plane, negative);
				if (negativeDistance < 0)
				{
					result = ContainmentType.Intersects;
				}
			}

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe ContainmentType Contains(ref BoundingFrustum other)
		{
			var pointsContained = 0;
			var corners = other.GetCorners();
			var cornersPtr = (Vector3*)&corners;
			for (var i = 0; i < 8; i++)
			{
				if (Contains(&cornersPtr[i]) != ContainmentType.Disjoint)
				{
					pointsContained++;
				}
			}

            return pointsContained == 8 ? ContainmentType.Contains : pointsContained == 0 ? ContainmentType.Disjoint : ContainmentType.Intersects;
        }

		public FrustumCorners GetCorners()
		{
            GetCorners(out var corners);
            return corners;
		}

		public void GetCorners(out FrustumCorners corners)
		{
			PlaneIntersection(ref _planes.Near, ref _planes.Top, ref _planes.Left, out corners.NearTopLeft);
			PlaneIntersection(ref _planes.Near, ref _planes.Top, ref _planes.Right, out corners.NearTopRight);
			PlaneIntersection(ref _planes.Near, ref _planes.Bottom, ref _planes.Left, out corners.NearBottomLeft);
			PlaneIntersection(ref _planes.Near, ref _planes.Bottom, ref _planes.Right, out corners.NearBottomRight);
			PlaneIntersection(ref _planes.Far, ref _planes.Top, ref _planes.Left, out corners.FarTopLeft);
			PlaneIntersection(ref _planes.Far, ref _planes.Top, ref _planes.Right, out corners.FarTopRight);
			PlaneIntersection(ref _planes.Far, ref _planes.Bottom, ref _planes.Left, out corners.FarBottomLeft);
			PlaneIntersection(ref _planes.Far, ref _planes.Bottom, ref _planes.Right, out corners.FarBottomRight);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void PlaneIntersection(ref Plane p1, ref Plane p2, ref Plane p3, out Vector3 intersection)
		{
			// Formula: http://geomalgorithms.com/a05-_intersect-1.html
			// The formula assumes that there is only a single intersection point.
			// Because of the way the frustum planes are constructed, this should be guaranteed.
			intersection =
				(-(p1.D * Vector3.Cross(p2.Normal, p3.Normal))
				- (p2.D * Vector3.Cross(p3.Normal, p1.Normal))
				- (p3.D * Vector3.Cross(p1.Normal, p2.Normal)))
				/ Vector3.Dot(p1.Normal, Vector3.Cross(p2.Normal, p3.Normal));
		}
	}

}
