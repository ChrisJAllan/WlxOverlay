// Copyright (c) 2007-2022 Juan Linietsky, Ariel Manzur.
// Copyright (c) 2014-2022 Godot Engine contributors (cf. AUTHORS.md).
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Runtime.InteropServices;
using Valve.VR;

namespace WlxOverlay.Numerics;

/// <summary>
/// 2-element structure that can be used to represent positions in 2D space or any other pair of numeric values.
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Vector2 : IEquatable<Vector2>
{
    /// <summary>
    /// Enumerated index values for the axes.
    /// Returned by <see cref="MaxAxisIndex"/> and <see cref="MinAxisIndex"/>.
    /// </summary>
    public enum Axis
    {
        /// <summary>
        /// The vector's X axis.
        /// </summary>
        X = 0,
        /// <summary>
        /// The vector's Y axis.
        /// </summary>
        Y
    }

    /// <summary>
    /// The vector's X component. Also accessible by using the index position <c>[0]</c>.
    /// </summary>
    public float x;

    /// <summary>
    /// The vector's Y component. Also accessible by using the index position <c>[1]</c>.
    /// </summary>
    public float y;

    /// <summary>
    /// Access vector components using their index.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not 0 or 1.
    /// </exception>
    /// <value>
    /// <c>[0]</c> is equivalent to <see cref="x"/>,
    /// <c>[1]</c> is equivalent to <see cref="y"/>.
    /// </value>
    public float this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return x;
                case 1:
                    return y;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                    x = value;
                    return;
                case 1:
                    y = value;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    /// <summary>
    /// Helper method for deconstruction into a tuple.
    /// </summary>
    public void Deconstruct(out float x, out float y)
    {
        x = this.x;
        y = this.y;
    }

    internal void Normalize()
    {
        float lengthsq = LengthSquared();

        if (lengthsq == 0)
        {
            x = y = 0f;
        }
        else
        {
            float length = Mathf.Sqrt(lengthsq);
            x /= length;
            y /= length;
        }
    }

    /// <summary>
    /// Returns a new vector with all components in absolute values (i.e. positive).
    /// </summary>
    /// <returns>A vector with <see cref="Mathf.Abs(float)"/> called on each component.</returns>
    public Vector2 Abs()
    {
        return new Vector2(Mathf.Abs(x), Mathf.Abs(y));
    }

    /// <summary>
    /// Returns this vector's angle with respect to the X axis, or (1, 0) vector, in radians.
    ///
    /// Equivalent to the result of <see cref="Mathf.Atan2(float, float)"/> when
    /// called with the vector's <see cref="y"/> and <see cref="x"/> as parameters: <c>Mathf.Atan2(v.y, v.x)</c>.
    /// </summary>
    /// <returns>The angle of this vector, in radians.</returns>
    public float Angle()
    {
        return Mathf.Atan2(y, x);
    }

    /// <summary>
    /// Returns the angle to the given vector, in radians.
    /// </summary>
    /// <param name="to">The other vector to compare this vector to.</param>
    /// <returns>The angle between the two vectors, in radians.</returns>
    public float AngleTo(Vector2 to)
    {
        return Mathf.Atan2(Cross(to), Dot(to));
    }

    /// <summary>
    /// Returns the angle between the line connecting the two points and the X axis, in radians.
    /// </summary>
    /// <param name="to">The other vector to compare this vector to.</param>
    /// <returns>The angle between the two vectors, in radians.</returns>
    public float AngleToPoint(Vector2 to)
    {
        return Mathf.Atan2(y - to.y, x - to.x);
    }

    /// <summary>
    /// Returns the aspect ratio of this vector, the ratio of <see cref="x"/> to <see cref="y"/>.
    /// </summary>
    /// <returns>The <see cref="x"/> component divided by the <see cref="y"/> component.</returns>
    public float Aspect()
    {
        return x / y;
    }

    /// <summary>
    /// Returns the vector "bounced off" from a plane defined by the given normal.
    /// </summary>
    /// <param name="normal">The normal vector defining the plane to bounce off. Must be normalized.</param>
    /// <returns>The bounced vector.</returns>
    public Vector2 Bounce(Vector2 normal)
    {
        return -Reflect(normal);
    }

    /// <summary>
    /// Returns a new vector with all components rounded up (towards positive infinity).
    /// </summary>
    /// <returns>A vector with <see cref="Mathf.Ceil"/> called on each component.</returns>
    public Vector2 Ceil()
    {
        return new Vector2(Mathf.Ceil(x), Mathf.Ceil(y));
    }

    /// <summary>
    /// Returns a new vector with all components clamped between the
    /// components of <paramref name="min"/> and <paramref name="max"/> using
    /// <see cref="Mathf.Clamp(float, float, float)"/>.
    /// </summary>
    /// <param name="min">The vector with minimum allowed values.</param>
    /// <param name="max">The vector with maximum allowed values.</param>
    /// <returns>The vector with all components clamped.</returns>
    public Vector2 Clamp(Vector2 min, Vector2 max)
    {
        return new Vector2
        (
            Mathf.Clamp(x, min.x, max.x),
            Mathf.Clamp(y, min.y, max.y)
        );
    }

    /// <summary>
    /// Returns the cross product of this vector and <paramref name="with"/>.
    /// </summary>
    /// <param name="with">The other vector.</param>
    /// <returns>The cross product value.</returns>
    public float Cross(Vector2 with)
    {
        return (x * with.y) - (y * with.x);
    }

    /// <summary>
    /// Performs a cubic interpolation between vectors <paramref name="preA"/>, this vector,
    /// <paramref name="b"/>, and <paramref name="postB"/>, by the given amount <paramref name="weight"/>.
    /// </summary>
    /// <param name="b">The destination vector.</param>
    /// <param name="preA">A vector before this vector.</param>
    /// <param name="postB">A vector after <paramref name="b"/>.</param>
    /// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
    /// <returns>The interpolated vector.</returns>
    public Vector2 CubicInterpolate(Vector2 b, Vector2 preA, Vector2 postB, float weight)
    {
        return new Vector2
        (
            Mathf.CubicInterpolate(x, b.x, preA.x, postB.x, weight),
            Mathf.CubicInterpolate(y, b.y, preA.y, postB.y, weight)
        );
    }

    /// <summary>
    /// Performs a cubic interpolation between vectors <paramref name="preA"/>, this vector,
    /// <paramref name="b"/>, and <paramref name="postB"/>, by the given amount <paramref name="weight"/>.
    /// It can perform smoother interpolation than <see cref="CubicInterpolate"/>
    /// by the time values.
    /// </summary>
    /// <param name="b">The destination vector.</param>
    /// <param name="preA">A vector before this vector.</param>
    /// <param name="postB">A vector after <paramref name="b"/>.</param>
    /// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
    /// <param name="t"></param>
    /// <param name="preAT"></param>
    /// <param name="postBT"></param>
    /// <returns>The interpolated vector.</returns>
    public Vector2 CubicInterpolateInTime(Vector2 b, Vector2 preA, Vector2 postB, float weight, float t, float preAT, float postBT)
    {
        return new Vector2
        (
            Mathf.CubicInterpolateInTime(x, b.x, preA.x, postB.x, weight, t, preAT, postBT),
            Mathf.CubicInterpolateInTime(y, b.y, preA.y, postB.y, weight, t, preAT, postBT)
        );
    }

    /// <summary>
    /// Returns the point at the given <paramref name="t"/> on a one-dimensional Bezier curve defined by this vector
    /// and the given <paramref name="control1"/>, <paramref name="control2"/> and <paramref name="end"/> points.
    /// </summary>
    /// <param name="control1">Control point that defines the bezier curve.</param>
    /// <param name="control2">Control point that defines the bezier curve.</param>
    /// <param name="end">The destination vector.</param>
    /// <param name="t">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
    /// <returns>The interpolated vector.</returns>
    public Vector2 BezierInterpolate(Vector2 control1, Vector2 control2, Vector2 end, float t)
    {
        // Formula from Wikipedia article on Bezier curves
        float omt = 1 - t;
        float omt2 = omt * omt;
        float omt3 = omt2 * omt;
        float t2 = t * t;
        float t3 = t2 * t;

        return this * omt3 + control1 * omt2 * t * 3 + control2 * omt * t2 * 3 + end * t3;
    }

    /// <summary>
    /// Returns the normalized vector pointing from this vector to <paramref name="to"/>.
    /// </summary>
    /// <param name="to">The other vector to point towards.</param>
    /// <returns>The direction from this vector to <paramref name="to"/>.</returns>
    public Vector2 DirectionTo(Vector2 to)
    {
        return new Vector2(to.x - x, to.y - y).Normalized();
    }

    /// <summary>
    /// Returns the squared distance between this vector and <paramref name="to"/>.
    /// This method runs faster than <see cref="DistanceTo"/>, so prefer it if
    /// you need to compare vectors or need the squared distance for some formula.
    /// </summary>
    /// <param name="to">The other vector to use.</param>
    /// <returns>The squared distance between the two vectors.</returns>
    public float DistanceSquaredTo(Vector2 to)
    {
        return (x - to.x) * (x - to.x) + (y - to.y) * (y - to.y);
    }

    /// <summary>
    /// Returns the distance between this vector and <paramref name="to"/>.
    /// </summary>
    /// <param name="to">The other vector to use.</param>
    /// <returns>The distance between the two vectors.</returns>
    public float DistanceTo(Vector2 to)
    {
        return Mathf.Sqrt((x - to.x) * (x - to.x) + (y - to.y) * (y - to.y));
    }

    /// <summary>
    /// Returns the dot product of this vector and <paramref name="with"/>.
    /// </summary>
    /// <param name="with">The other vector to use.</param>
    /// <returns>The dot product of the two vectors.</returns>
    public float Dot(Vector2 with)
    {
        return (x * with.x) + (y * with.y);
    }

    /// <summary>
    /// Returns a new vector with all components rounded down (towards negative infinity).
    /// </summary>
    /// <returns>A vector with <see cref="Mathf.Floor"/> called on each component.</returns>
    public Vector2 Floor()
    {
        return new Vector2(Mathf.Floor(x), Mathf.Floor(y));
    }

    /// <summary>
    /// Returns the inverse of this vector. This is the same as <c>new Vector2(1 / v.x, 1 / v.y)</c>.
    /// </summary>
    /// <returns>The inverse of this vector.</returns>
    public Vector2 Inverse()
    {
        return new Vector2(1 / x, 1 / y);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the vector is normalized, and <see langword="false"/> otherwise.
    /// </summary>
    /// <returns>A <see langword="bool"/> indicating whether or not the vector is normalized.</returns>
    public bool IsNormalized()
    {
        return Mathf.Abs(LengthSquared() - 1.0f) < float.Epsilon;
    }

    /// <summary>
    /// Returns the length (magnitude) of this vector.
    /// </summary>
    /// <seealso cref="LengthSquared"/>
    /// <returns>The length of this vector.</returns>
    public float Length()
    {
        return Mathf.Sqrt((x * x) + (y * y));
    }

    /// <summary>
    /// Returns the squared length (squared magnitude) of this vector.
    /// This method runs faster than <see cref="Length"/>, so prefer it if
    /// you need to compare vectors or need the squared length for some formula.
    /// </summary>
    /// <returns>The squared length of this vector.</returns>
    public float LengthSquared()
    {
        return (x * x) + (y * y);
    }

    /// <summary>
    /// Returns the result of the linear interpolation between
    /// this vector and <paramref name="to"/> by amount <paramref name="weight"/>.
    /// </summary>
    /// <param name="to">The destination vector for interpolation.</param>
    /// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
    /// <returns>The resulting vector of the interpolation.</returns>
    public Vector2 Lerp(Vector2 to, float weight)
    {
        return new Vector2
        (
            Mathf.Lerp(x, to.x, weight),
            Mathf.Lerp(y, to.y, weight)
        );
    }

    /// <summary>
    /// Returns the result of the linear interpolation between
    /// this vector and <paramref name="to"/> by the vector amount <paramref name="weight"/>.
    /// </summary>
    /// <param name="to">The destination vector for interpolation.</param>
    /// <param name="weight">
    /// A vector with components on the range of 0.0 to 1.0, representing the amount of interpolation.
    /// </param>
    /// <returns>The resulting vector of the interpolation.</returns>
    public Vector2 Lerp(Vector2 to, Vector2 weight)
    {
        return new Vector2
        (
            Mathf.Lerp(x, to.x, weight.x),
            Mathf.Lerp(y, to.y, weight.y)
        );
    }

    /// <summary>
    /// Returns the vector with a maximum length by limiting its length to <paramref name="length"/>.
    /// </summary>
    /// <param name="length">The length to limit to.</param>
    /// <returns>The vector with its length limited.</returns>
    public Vector2 LimitLength(float length = 1.0f)
    {
        Vector2 v = this;
        float l = Length();

        if (l > 0 && length < l)
        {
            v /= l;
            v *= length;
        }

        return v;
    }

    /// <summary>
    /// Returns the axis of the vector's highest value. See <see cref="Axis"/>.
    /// If both components are equal, this method returns <see cref="Axis.X"/>.
    /// </summary>
    /// <returns>The index of the highest axis.</returns>
    public Axis MaxAxisIndex()
    {
        return x < y ? Axis.Y : Axis.X;
    }

    /// <summary>
    /// Returns the axis of the vector's lowest value. See <see cref="Axis"/>.
    /// If both components are equal, this method returns <see cref="Axis.Y"/>.
    /// </summary>
    /// <returns>The index of the lowest axis.</returns>
    public Axis MinAxisIndex()
    {
        return x < y ? Axis.X : Axis.Y;
    }

    /// <summary>
    /// Moves this vector toward <paramref name="to"/> by the fixed <paramref name="delta"/> amount.
    /// </summary>
    /// <param name="to">The vector to move towards.</param>
    /// <param name="delta">The amount to move towards by.</param>
    /// <returns>The resulting vector.</returns>
    public Vector2 MoveToward(Vector2 to, float delta)
    {
        Vector2 v = this;
        Vector2 vd = to - v;
        float len = vd.Length();
        if (len <= delta || len < float.Epsilon)
            return to;

        return v + (vd / len * delta);
    }

    /// <summary>
    /// Returns the vector scaled to unit length. Equivalent to <c>v / v.Length()</c>.
    /// </summary>
    /// <returns>A normalized version of the vector.</returns>
    public Vector2 Normalized()
    {
        Vector2 v = this;
        v.Normalize();
        return v;
    }

    /// <summary>
    /// Returns a vector composed of the <see cref="Mathf.PosMod(float, float)"/> of this vector's components
    /// and <paramref name="mod"/>.
    /// </summary>
    /// <param name="mod">A value representing the divisor of the operation.</param>
    /// <returns>
    /// A vector with each component <see cref="Mathf.PosMod(float, float)"/> by <paramref name="mod"/>.
    /// </returns>
    public Vector2 PosMod(float mod)
    {
        Vector2 v;
        v.x = Mathf.PosMod(x, mod);
        v.y = Mathf.PosMod(y, mod);
        return v;
    }

    /// <summary>
    /// Returns a vector composed of the <see cref="Mathf.PosMod(float, float)"/> of this vector's components
    /// and <paramref name="modv"/>'s components.
    /// </summary>
    /// <param name="modv">A vector representing the divisors of the operation.</param>
    /// <returns>
    /// A vector with each component <see cref="Mathf.PosMod(float, float)"/> by <paramref name="modv"/>'s components.
    /// </returns>
    public Vector2 PosMod(Vector2 modv)
    {
        Vector2 v;
        v.x = Mathf.PosMod(x, modv.x);
        v.y = Mathf.PosMod(y, modv.y);
        return v;
    }

    /// <summary>
    /// Returns this vector projected onto another vector <paramref name="onNormal"/>.
    /// </summary>
    /// <param name="onNormal">The vector to project onto.</param>
    /// <returns>The projected vector.</returns>
    public Vector2 Project(Vector2 onNormal)
    {
        return onNormal * (Dot(onNormal) / onNormal.LengthSquared());
    }

    /// <summary>
    /// Returns this vector reflected from a plane defined by the given <paramref name="normal"/>.
    /// </summary>
    /// <param name="normal">The normal vector defining the plane to reflect from. Must be normalized.</param>
    /// <returns>The reflected vector.</returns>
    public Vector2 Reflect(Vector2 normal)
    {
#if DEBUG
        if (!normal.IsNormalized())
        {
            throw new ArgumentException("Argument is not normalized.", nameof(normal));
        }
#endif
        return (2 * Dot(normal) * normal) - this;
    }

    /// <summary>
    /// Rotates this vector by <paramref name="angle"/> radians.
    /// </summary>
    /// <param name="angle">The angle to rotate by, in radians.</param>
    /// <returns>The rotated vector.</returns>
    public Vector2 Rotated(float angle)
    {
        float sine = Mathf.Sin(angle);
        float cosi = Mathf.Cos(angle);
        return new Vector2(
            x * cosi - y * sine,
            x * sine + y * cosi);
    }

    /// <summary>
    /// Returns this vector with all components rounded to the nearest integer,
    /// with halfway cases rounded towards the nearest multiple of two.
    /// </summary>
    /// <returns>The rounded vector.</returns>
    public Vector2 Round()
    {
        return new Vector2(Mathf.Round(x), Mathf.Round(y));
    }

    /// <summary>
    /// Returns a vector with each component set to one or negative one, depending
    /// on the signs of this vector's components, or zero if the component is zero,
    /// by calling <see cref="Mathf.Sign(float)"/> on each component.
    /// </summary>
    /// <returns>A vector with all components as either <c>1</c>, <c>-1</c>, or <c>0</c>.</returns>
    public Vector2 Sign()
    {
        Vector2 v;
        v.x = Mathf.Sign(x);
        v.y = Mathf.Sign(y);
        return v;
    }

    /// <summary>
    /// Returns the result of the spherical linear interpolation between
    /// this vector and <paramref name="to"/> by amount <paramref name="weight"/>.
    ///
    /// This method also handles interpolating the lengths if the input vectors
    /// have different lengths. For the special case of one or both input vectors
    /// having zero length, this method behaves like <see cref="Lerp(Vector2, float)"/>.
    /// </summary>
    /// <param name="to">The destination vector for interpolation.</param>
    /// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
    /// <returns>The resulting vector of the interpolation.</returns>
    public Vector2 Slerp(Vector2 to, float weight)
    {
        float startLengthSquared = LengthSquared();
        float endLengthSquared = to.LengthSquared();
        if (startLengthSquared == 0.0 || endLengthSquared == 0.0)
        {
            // Zero length vectors have no angle, so the best we can do is either lerp or throw an error.
            return Lerp(to, weight);
        }
        float startLength = Mathf.Sqrt(startLengthSquared);
        float resultLength = Mathf.Lerp(startLength, Mathf.Sqrt(endLengthSquared), weight);
        float angle = AngleTo(to);
        return Rotated(angle * weight) * (resultLength / startLength);
    }

    /// <summary>
    /// Returns this vector slid along a plane defined by the given <paramref name="normal"/>.
    /// </summary>
    /// <param name="normal">The normal vector defining the plane to slide on.</param>
    /// <returns>The slid vector.</returns>
    public Vector2 Slide(Vector2 normal)
    {
        return this - (normal * Dot(normal));
    }

    /// <summary>
    /// Returns this vector with each component snapped to the nearest multiple of <paramref name="step"/>.
    /// This can also be used to round to an arbitrary number of decimals.
    /// </summary>
    /// <param name="step">A vector value representing the step size to snap to.</param>
    /// <returns>The snapped vector.</returns>
    public Vector2 Snapped(Vector2 step)
    {
        return new Vector2(Mathf.Snapped(x, step.x), Mathf.Snapped(y, step.y));
    }

    /// <summary>
    /// Returns a perpendicular vector rotated 90 degrees counter-clockwise
    /// compared to the original, with the same length.
    /// </summary>
    /// <returns>The perpendicular vector.</returns>
    public Vector2 Orthogonal()
    {
        return new Vector2(y, -x);
    }

    // Constants
    private static readonly Vector2 _zero = new Vector2(0, 0);
    private static readonly Vector2 _one = new Vector2(1, 1);
    private static readonly Vector2 _inf = new Vector2(Mathf.Inf, Mathf.Inf);

    private static readonly Vector2 _up = new Vector2(0, -1);
    private static readonly Vector2 _down = new Vector2(0, 1);
    private static readonly Vector2 _right = new Vector2(1, 0);
    private static readonly Vector2 _left = new Vector2(-1, 0);

    /// <summary>
    /// Zero vector, a vector with all components set to <c>0</c>.
    /// </summary>
    /// <value>Equivalent to <c>new Vector2(0, 0)</c>.</value>
    public static Vector2 Zero { get { return _zero; } }
    /// <summary>
    /// One vector, a vector with all components set to <c>1</c>.
    /// </summary>
    /// <value>Equivalent to <c>new Vector2(1, 1)</c>.</value>
    public static Vector2 One { get { return _one; } }
    /// <summary>
    /// Infinity vector, a vector with all components set to <see cref="Mathf.Inf"/>.
    /// </summary>
    /// <value>Equivalent to <c>new Vector2(Mathf.Inf, Mathf.Inf)</c>.</value>
    public static Vector2 Inf { get { return _inf; } }

    /// <summary>
    /// Up unit vector. Y is down in 2D, so this vector points -Y.
    /// </summary>
    /// <value>Equivalent to <c>new Vector2(0, -1)</c>.</value>
    public static Vector2 Up { get { return _up; } }
    /// <summary>
    /// Down unit vector. Y is down in 2D, so this vector points +Y.
    /// </summary>
    /// <value>Equivalent to <c>new Vector2(0, 1)</c>.</value>
    public static Vector2 Down { get { return _down; } }
    /// <summary>
    /// Right unit vector. Represents the direction of right.
    /// </summary>
    /// <value>Equivalent to <c>new Vector2(1, 0)</c>.</value>
    public static Vector2 Right { get { return _right; } }
    /// <summary>
    /// Left unit vector. Represents the direction of left.
    /// </summary>
    /// <value>Equivalent to <c>new Vector2(-1, 0)</c>.</value>
    public static Vector2 Left { get { return _left; } }

    /// <summary>
    /// Constructs a new <see cref="Vector2"/> with the given components.
    /// </summary>
    /// <param name="x">The vector's X component.</param>
    /// <param name="y">The vector's Y component.</param>
    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// Creates a unit Vector2 rotated to the given angle. This is equivalent to doing
    /// <c>Vector2(Mathf.Cos(angle), Mathf.Sin(angle))</c> or <c>Vector2.Right.Rotated(angle)</c>.
    /// </summary>
    /// <param name="angle">Angle of the vector, in radians.</param>
    /// <returns>The resulting vector.</returns>
    public static Vector2 FromAngle(float angle)
    {
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    /// <summary>
    /// Adds each component of the <see cref="Vector2"/>
    /// with the components of the given <see cref="Vector2"/>.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>The added vector.</returns>
    public static Vector2 operator +(Vector2 left, Vector2 right)
    {
        left.x += right.x;
        left.y += right.y;
        return left;
    }

    /// <summary>
    /// Subtracts each component of the <see cref="Vector2"/>
    /// by the components of the given <see cref="Vector2"/>.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>The subtracted vector.</returns>
    public static Vector2 operator -(Vector2 left, Vector2 right)
    {
        left.x -= right.x;
        left.y -= right.y;
        return left;
    }

    /// <summary>
    /// Returns the negative value of the <see cref="Vector2"/>.
    /// This is the same as writing <c>new Vector2(-v.x, -v.y)</c>.
    /// This operation flips the direction of the vector while
    /// keeping the same magnitude.
    /// With floats, the number zero can be either positive or negative.
    /// </summary>
    /// <param name="vec">The vector to negate/flip.</param>
    /// <returns>The negated/flipped vector.</returns>
    public static Vector2 operator -(Vector2 vec)
    {
        vec.x = -vec.x;
        vec.y = -vec.y;
        return vec;
    }

    /// <summary>
    /// Multiplies each component of the <see cref="Vector2"/>
    /// by the given <see cref="float"/>.
    /// </summary>
    /// <param name="vec">The vector to multiply.</param>
    /// <param name="scale">The scale to multiply by.</param>
    /// <returns>The multiplied vector.</returns>
    public static Vector2 operator *(Vector2 vec, float scale)
    {
        vec.x *= scale;
        vec.y *= scale;
        return vec;
    }

    /// <summary>
    /// Multiplies each component of the <see cref="Vector2"/>
    /// by the given <see cref="float"/>.
    /// </summary>
    /// <param name="scale">The scale to multiply by.</param>
    /// <param name="vec">The vector to multiply.</param>
    /// <returns>The multiplied vector.</returns>
    public static Vector2 operator *(float scale, Vector2 vec)
    {
        vec.x *= scale;
        vec.y *= scale;
        return vec;
    }

    /// <summary>
    /// Multiplies each component of the <see cref="Vector2"/>
    /// by the components of the given <see cref="Vector2"/>.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>The multiplied vector.</returns>
    public static Vector2 operator *(Vector2 left, Vector2 right)
    {
        left.x *= right.x;
        left.y *= right.y;
        return left;
    }

    /// <summary>
    /// Multiplies each component of the <see cref="Vector2"/>
    /// by the given <see cref="float"/>.
    /// </summary>
    /// <param name="vec">The dividend vector.</param>
    /// <param name="divisor">The divisor value.</param>
    /// <returns>The divided vector.</returns>
    public static Vector2 operator /(Vector2 vec, float divisor)
    {
        vec.x /= divisor;
        vec.y /= divisor;
        return vec;
    }

    /// <summary>
    /// Divides each component of the <see cref="Vector2"/>
    /// by the components of the given <see cref="Vector2"/>.
    /// </summary>
    /// <param name="vec">The dividend vector.</param>
    /// <param name="divisorv">The divisor vector.</param>
    /// <returns>The divided vector.</returns>
    public static Vector2 operator /(Vector2 vec, Vector2 divisorv)
    {
        vec.x /= divisorv.x;
        vec.y /= divisorv.y;
        return vec;
    }

    /// <summary>
    /// Gets the remainder of each component of the <see cref="Vector2"/>
    /// with the components of the given <see cref="float"/>.
    /// This operation uses truncated division, which is often not desired
    /// as it does not work well with negative numbers.
    /// Consider using <see cref="PosMod(float)"/> instead
    /// if you want to handle negative numbers.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.Print(new Vector2(10, -20) % 7); // Prints "(3, -6)"
    /// </code>
    /// </example>
    /// <param name="vec">The dividend vector.</param>
    /// <param name="divisor">The divisor value.</param>
    /// <returns>The remainder vector.</returns>
    public static Vector2 operator %(Vector2 vec, float divisor)
    {
        vec.x %= divisor;
        vec.y %= divisor;
        return vec;
    }

    /// <summary>
    /// Gets the remainder of each component of the <see cref="Vector2"/>
    /// with the components of the given <see cref="Vector2"/>.
    /// This operation uses truncated division, which is often not desired
    /// as it does not work well with negative numbers.
    /// Consider using <see cref="PosMod(Vector2)"/> instead
    /// if you want to handle negative numbers.
    /// </summary>
    /// <example>
    /// <code>
    /// GD.Print(new Vector2(10, -20) % new Vector2(7, 8)); // Prints "(3, -4)"
    /// </code>
    /// </example>
    /// <param name="vec">The dividend vector.</param>
    /// <param name="divisorv">The divisor vector.</param>
    /// <returns>The remainder vector.</returns>
    public static Vector2 operator %(Vector2 vec, Vector2 divisorv)
    {
        vec.x %= divisorv.x;
        vec.y %= divisorv.y;
        return vec;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the vectors are exactly equal.
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the vectors are exactly equal.</returns>
    public static bool operator ==(Vector2 left, Vector2 right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the vectors are not equal.
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the vectors are not equal.</returns>
    public static bool operator !=(Vector2 left, Vector2 right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Compares two <see cref="Vector2"/> vectors by first checking if
    /// the X value of the <paramref name="left"/> vector is less than
    /// the X value of the <paramref name="right"/> vector.
    /// If the X values are exactly equal, then it repeats this check
    /// with the Y values of the two vectors.
    /// This operator is useful for sorting vectors.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the left is less than the right.</returns>
    public static bool operator <(Vector2 left, Vector2 right)
    {
        if (left.x == right.x)
        {
            return left.y < right.y;
        }
        return left.x < right.x;
    }

    /// <summary>
    /// Compares two <see cref="Vector2"/> vectors by first checking if
    /// the X value of the <paramref name="left"/> vector is greater than
    /// the X value of the <paramref name="right"/> vector.
    /// If the X values are exactly equal, then it repeats this check
    /// with the Y values of the two vectors.
    /// This operator is useful for sorting vectors.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the left is greater than the right.</returns>
    public static bool operator >(Vector2 left, Vector2 right)
    {
        if (left.x == right.x)
        {
            return left.y > right.y;
        }
        return left.x > right.x;
    }

    /// <summary>
    /// Compares two <see cref="Vector2"/> vectors by first checking if
    /// the X value of the <paramref name="left"/> vector is less than
    /// or equal to the X value of the <paramref name="right"/> vector.
    /// If the X values are exactly equal, then it repeats this check
    /// with the Y values of the two vectors.
    /// This operator is useful for sorting vectors.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the left is less than or equal to the right.</returns>
    public static bool operator <=(Vector2 left, Vector2 right)
    {
        if (left.x == right.x)
        {
            return left.y <= right.y;
        }
        return left.x < right.x;
    }

    /// <summary>
    /// Compares two <see cref="Vector2"/> vectors by first checking if
    /// the X value of the <paramref name="left"/> vector is greater than
    /// or equal to the X value of the <paramref name="right"/> vector.
    /// If the X values are exactly equal, then it repeats this check
    /// with the Y values of the two vectors.
    /// This operator is useful for sorting vectors.
    /// </summary>
    /// <param name="left">The left vector.</param>
    /// <param name="right">The right vector.</param>
    /// <returns>Whether or not the left is greater than or equal to the right.</returns>
    public static bool operator >=(Vector2 left, Vector2 right)
    {
        if (left.x == right.x)
        {
            return left.y >= right.y;
        }
        return left.x > right.x;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the vector is exactly equal
    /// to the given object (<see paramref="obj"/>).
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>Whether or not the vector and the object are equal.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Vector2 other && Equals(other);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the vectors are exactly equal.
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>Whether or not the vectors are exactly equal.</returns>
    public bool Equals(Vector2 other)
    {
        return x == other.x && y == other.y;
    }

    /// <summary>
    /// Returns <see langword="true"/> if this vector and <paramref name="other"/> are approximately equal,
    /// by running <see cref="Mathf.IsEqualApprox(float, float)"/> on each component.
    /// </summary>
    /// <param name="other">The other vector to compare.</param>
    /// <returns>Whether or not the vectors are approximately equal.</returns>
    public bool IsEqualApprox(Vector2 other)
    {
        return Mathf.IsEqualApprox(x, other.x) && Mathf.IsEqualApprox(y, other.y);
    }

    /// <summary>
    /// Serves as the hash function for <see cref="Vector2"/>.
    /// </summary>
    /// <returns>A hash code for this vector.</returns>
    public override int GetHashCode()
    {
        return y.GetHashCode() ^ x.GetHashCode();
    }

    /// <summary>
    /// Converts this <see cref="Vector2"/> to a string.
    /// </summary>
    /// <returns>A string representation of this vector.</returns>
    public override string ToString()
    {
        return $"({x}, {y})";
    }

    /// <summary>
    /// Converts this <see cref="Vector2"/> to a string with the given <paramref name="format"/>.
    /// </summary>
    /// <returns>A string representation of this vector.</returns>
    public string ToString(string format)
    {
        return $"({x.ToString(format)}, {y.ToString(format)})";
    }

    public static explicit operator Vector2(HmdVector2_t v)
    {
        return new Vector2(v.v0, v.v1);
    }

    public static explicit operator HmdVector2_t(Vector2 v)
    {
        return new HmdVector2_t { v0 = v.x, v1 = v.y };
    }
}