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

namespace WlxOverlay.Numerics;

/// <summary>
/// 3×4 matrix (3 rows, 4 columns) used for 3D linear transformations.
/// It can represent transformations such as translation, rotation, or scaling.
/// It consists of a <see cref="Basis"/> (first 3 columns) and a
/// <see cref="Vector3"/> for the origin (last column).
///
/// For more information, read this documentation article:
/// https://docs.godotengine.org/en/latest/tutorials/math/matrices_and_transforms.html
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Transform3D : IEquatable<Transform3D>
{
    /// <summary>
    /// The <see cref="Basis"/> of this transform. Contains the X, Y, and Z basis
    /// vectors (columns 0 to 2) and is responsible for rotation and scale.
    /// </summary>
    public Basis basis;

    /// <summary>
    /// The origin vector (column 3, the fourth column). Equivalent to array index <c>[3]</c>.
    /// </summary>
    public Vector3 origin;

    /// <summary>
    /// Access whole columns in the form of <see cref="Vector3"/>.
    /// The fourth column is the <see cref="origin"/> vector.
    /// </summary>
    /// <param name="column">Which column vector.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="column"/> is not 0, 1, 2 or 3.
    /// </exception>
    public Vector3 this[int column]
    {
        get
        {
            switch (column)
            {
                case 0:
                    return basis.Column0;
                case 1:
                    return basis.Column1;
                case 2:
                    return basis.Column2;
                case 3:
                    return origin;
                default:
                    throw new ArgumentOutOfRangeException(nameof(column));
            }
        }
        set
        {
            switch (column)
            {
                case 0:
                    basis.Column0 = value;
                    return;
                case 1:
                    basis.Column1 = value;
                    return;
                case 2:
                    basis.Column2 = value;
                    return;
                case 3:
                    origin = value;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(column));
            }
        }
    }

    /// <summary>
    /// Access matrix elements in column-major order.
    /// The fourth column is the <see cref="origin"/> vector.
    /// </summary>
    /// <param name="column">Which column, the matrix horizontal position.</param>
    /// <param name="row">Which row, the matrix vertical position.</param>
    public float this[int column, int row]
    {
        get
        {
            if (column == 3)
            {
                return origin[row];
            }
            return basis[column, row];
        }
        set
        {
            if (column == 3)
            {
                origin[row] = value;
                return;
            }
            basis[column, row] = value;
        }
    }

    /// <summary>
    /// Returns the inverse of the transform, under the assumption that
    /// the transformation is composed of rotation, scaling, and translation.
    /// </summary>
    /// <seealso cref="Inverse"/>
    /// <returns>The inverse transformation matrix.</returns>
    public Transform3D AffineInverse()
    {
        Basis basisInv = basis.Inverse();
        return new Transform3D(basisInv, basisInv * -origin);
    }

    /// <summary>
    /// Interpolates this transform to the other <paramref name="transform"/> by <paramref name="weight"/>.
    /// </summary>
    /// <param name="transform">The other transform.</param>
    /// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
    /// <returns>The interpolated transform.</returns>
    public Transform3D InterpolateWith(Transform3D transform, float weight)
    {
        Basis retBasis = basis.Lerp(transform.basis, weight);
        Vector3 retOrigin = origin.Lerp(transform.origin, weight);
        return new Transform3D(retBasis, retOrigin);
    }

    /// <summary>
    /// Returns the inverse of the transform, under the assumption that
    /// the transformation is composed of rotation and translation
    /// (no scaling, use <see cref="AffineInverse"/> for transforms with scaling).
    /// </summary>
    /// <returns>The inverse matrix.</returns>
    public Transform3D Inverse()
    {
        Basis basisTr = basis.Transposed();
        return new Transform3D(basisTr, basisTr * -origin);
    }

    /// <summary>
    /// Returns a copy of the transform rotated such that its
    /// -Z axis (forward) points towards the <paramref name="target"/> position.
    ///
    /// The transform will first be rotated around the given <paramref name="up"/> vector,
    /// and then fully aligned to the <paramref name="target"/> by a further rotation around
    /// an axis perpendicular to both the <paramref name="target"/> and <paramref name="up"/> vectors.
    ///
    /// Operations take place in global space.
    /// </summary>
    /// <param name="target">The object to look at.</param>
    /// <param name="up">The relative up direction.</param>
    /// <returns>The resulting transform.</returns>
    public readonly Transform3D LookingAt(Vector3 target, Vector3 up)
    {
        Transform3D t = this;
        t.SetLookAt(origin, target, up);
        return t;
    }

    /// <summary>
    /// Returns the transform with the basis orthogonal (90 degrees),
    /// and normalized axis vectors (scale of 1 or -1).
    /// </summary>
    /// <returns>The orthonormalized transform.</returns>
    public Transform3D Orthonormalized()
    {
        return new Transform3D(basis.Orthonormalized(), origin);
    }

    /// <summary>
    /// Rotates the transform around the given <paramref name="axis"/> by <paramref name="angle"/> (in radians).
    /// The axis must be a normalized vector.
    /// The operation is done in the parent/global frame, equivalent to
    /// multiplying the matrix from the left.
    /// </summary>
    /// <param name="axis">The axis to rotate around. Must be normalized.</param>
    /// <param name="angle">The angle to rotate, in radians.</param>
    /// <returns>The rotated transformation matrix.</returns>
    public readonly Transform3D Rotated(Vector3 axis, float angle)
    {
        return new Transform3D(new Basis(axis, angle), new Vector3()) * this;
    }

    /// <summary>
    /// Rotates the transform around the given <paramref name="axis"/> by <paramref name="angle"/> (in radians).
    /// The axis must be a normalized vector.
    /// The operation is done in the local frame, equivalent to
    /// multiplying the matrix from the right.
    /// </summary>
    /// <param name="axis">The axis to rotate around. Must be normalized.</param>
    /// <param name="angle">The angle to rotate, in radians.</param>
    /// <returns>The rotated transformation matrix.</returns>
    public Transform3D RotatedLocal(Vector3 axis, float angle)
    {
        Basis tmpBasis = new Basis(axis, angle);
        return new Transform3D(basis * tmpBasis, origin);
    }

    /// <summary>
    /// Scales the transform by the given 3D <paramref name="scale"/> factor.
    /// The operation is done in the parent/global frame, equivalent to
    /// multiplying the matrix from the left.
    /// </summary>
    /// <param name="scale">The scale to introduce.</param>
    /// <returns>The scaled transformation matrix.</returns>
    public Transform3D Scaled(Vector3 scale)
    {
        return new Transform3D(basis.Scaled(scale), origin * scale);
    }

    /// <summary>
    /// Scales the transform by the given 3D <paramref name="scale"/> factor.
    /// The operation is done in the local frame, equivalent to
    /// multiplying the matrix from the right.
    /// </summary>
    /// <param name="scale">The scale to introduce.</param>
    /// <returns>The scaled transformation matrix.</returns>
    public Transform3D ScaledLocal(Vector3 scale)
    {
        Basis tmpBasis = Basis.FromScale(scale);
        return new Transform3D(basis * tmpBasis, origin);
    }

    /// <summary>
    /// Returns a transform spherically interpolated between this transform and
    /// another <paramref name="transform"/> by <paramref name="weight"/>.
    /// </summary>
    /// <param name="transform">The other transform.</param>
    /// <param name="weight">A value on the range of 0.0 to 1.0, representing the amount of interpolation.</param>
    /// <returns>The interpolated transform.</returns>
    public Transform3D SphericalInterpolateWith(Transform3D transform, float weight)
    {
        /* not sure if very "efficient" but good enough? */

        Vector3 sourceScale = basis.Scale;
        Quaternion sourceRotation = basis.GetRotationQuaternion();
        Vector3 sourceLocation = origin;

        Vector3 destinationScale = transform.basis.Scale;
        Quaternion destinationRotation = transform.basis.GetRotationQuaternion();
        Vector3 destinationLocation = transform.origin;

        var interpolated = new Transform3D();
        Quaternion quaternion = sourceRotation.Slerp(destinationRotation, weight).Normalized();
        Vector3 scale = sourceScale.Lerp(destinationScale, weight);
        interpolated.basis.SetQuaternionScale(quaternion, scale);
        interpolated.origin = sourceLocation.Lerp(destinationLocation, weight);

        return interpolated;
    }

    private void SetLookAt(Vector3 eye, Vector3 target, Vector3 up)
    {
        // Make rotation matrix
        // Z vector
        Vector3 column2 = eye - target;

        column2.Normalize();

        Vector3 column1 = up;

        Vector3 column0 = column1.Cross(column2);

        // Recompute Y = Z cross X
        column1 = column2.Cross(column0);

        column0.Normalize();
        column1.Normalize();

        basis = new Basis(column0, column1, column2);

        origin = eye;
    }

    /// <summary>
    /// Translates the transform by the given <paramref name="offset"/>.
    /// The operation is done in the parent/global frame, equivalent to
    /// multiplying the matrix from the left.
    /// </summary>
    /// <param name="offset">The offset to translate by.</param>
    /// <returns>The translated matrix.</returns>
    public Transform3D Translated(Vector3 offset)
    {
        return new Transform3D(basis, origin + offset);
    }

    /// <summary>
    /// Translates the transform by the given <paramref name="offset"/>.
    /// The operation is done in the local frame, equivalent to
    /// multiplying the matrix from the right.
    /// </summary>
    /// <param name="offset">The offset to translate by.</param>
    /// <returns>The translated matrix.</returns>
    public Transform3D TranslatedLocal(Vector3 offset)
    {
        return new Transform3D(basis, new Vector3
        (
            origin[0] + basis.Row0.Dot(offset),
            origin[1] + basis.Row1.Dot(offset),
            origin[2] + basis.Row2.Dot(offset)
        ));
    }

    // Constants
    private static readonly Transform3D _identity = new Transform3D(Basis.Identity, Vector3.Zero);
    private static readonly Transform3D _flipX = new Transform3D(new Basis(-1, 0, 0, 0, 1, 0, 0, 0, 1), Vector3.Zero);
    private static readonly Transform3D _flipY = new Transform3D(new Basis(1, 0, 0, 0, -1, 0, 0, 0, 1), Vector3.Zero);
    private static readonly Transform3D _flipZ = new Transform3D(new Basis(1, 0, 0, 0, 1, 0, 0, 0, -1), Vector3.Zero);

    /// <summary>
    /// The identity transform, with no translation, rotation, or scaling applied.
    /// This is used as a replacement for <c>Transform()</c> in GDScript.
    /// Do not use <c>new Transform()</c> with no arguments in C#, because it sets all values to zero.
    /// </summary>
    /// <value>Equivalent to <c>new Transform(Vector3.Right, Vector3.Up, Vector3.Back, Vector3.Zero)</c>.</value>
    public static Transform3D Identity { get { return _identity; } }
    /// <summary>
    /// The transform that will flip something along the X axis.
    /// </summary>
    /// <value>Equivalent to <c>new Transform(Vector3.Left, Vector3.Up, Vector3.Back, Vector3.Zero)</c>.</value>
    public static Transform3D FlipX { get { return _flipX; } }
    /// <summary>
    /// The transform that will flip something along the Y axis.
    /// </summary>
    /// <value>Equivalent to <c>new Transform(Vector3.Right, Vector3.Down, Vector3.Back, Vector3.Zero)</c>.</value>
    public static Transform3D FlipY { get { return _flipY; } }
    /// <summary>
    /// The transform that will flip something along the Z axis.
    /// </summary>
    /// <value>Equivalent to <c>new Transform(Vector3.Right, Vector3.Up, Vector3.Forward, Vector3.Zero)</c>.</value>
    public static Transform3D FlipZ { get { return _flipZ; } }

    /// <summary>
    /// Constructs a transformation matrix from 4 vectors (matrix columns).
    /// </summary>
    /// <param name="column0">The X vector, or column index 0.</param>
    /// <param name="column1">The Y vector, or column index 1.</param>
    /// <param name="column2">The Z vector, or column index 2.</param>
    /// <param name="origin">The origin vector, or column index 3.</param>
    public Transform3D(Vector3 column0, Vector3 column1, Vector3 column2, Vector3 origin)
    {
        basis = new Basis(column0, column1, column2);
        this.origin = origin;
    }

    /// <summary>
    /// Constructs a transformation matrix from the given <paramref name="quaternion"/>
    /// and <paramref name="origin"/> vector.
    /// </summary>
    /// <param name="quaternion">The <see cref="Quaternion"/> to create the basis from.</param>
    /// <param name="origin">The origin vector, or column index 3.</param>
    public Transform3D(Quaternion quaternion, Vector3 origin)
    {
        basis = new Basis(quaternion);
        this.origin = origin;
    }

    /// <summary>
    /// Constructs a transformation matrix from the given <paramref name="basis"/> and
    /// <paramref name="origin"/> vector.
    /// </summary>
    /// <param name="basis">The <see cref="Basis"/> to create the basis from.</param>
    /// <param name="origin">The origin vector, or column index 3.</param>
    public Transform3D(Basis basis, Vector3 origin)
    {
        this.basis = basis;
        this.origin = origin;
    }

    /// <summary>
    /// Composes these two transformation matrices by multiplying them
    /// together. This has the effect of transforming the second transform
    /// (the child) by the first transform (the parent).
    /// </summary>
    /// <param name="left">The parent transform.</param>
    /// <param name="right">The child transform.</param>
    /// <returns>The composed transform.</returns>
    public static Transform3D operator *(Transform3D left, Transform3D right)
    {
        left.origin = left * right.origin;
        left.basis *= right.basis;
        return left;
    }

    /// <summary>
    /// Returns a Vector3 transformed (multiplied) by the transformation matrix.
    /// </summary>
    /// <param name="transform">The transformation to apply.</param>
    /// <param name="vector">A Vector3 to transform.</param>
    /// <returns>The transformed Vector3.</returns>
    public static Vector3 operator *(Transform3D transform, Vector3 vector)
    {
        return new Vector3
        (
            transform.basis.Row0.Dot(vector) + transform.origin.x,
            transform.basis.Row1.Dot(vector) + transform.origin.y,
            transform.basis.Row2.Dot(vector) + transform.origin.z
        );
    }

    /// <summary>
    /// Returns a Vector3 transformed (multiplied) by the transposed transformation matrix.
    ///
    /// Note: This results in a multiplication by the inverse of the
    /// transformation matrix only if it represents a rotation-reflection.
    /// </summary>
    /// <param name="vector">A Vector3 to inversely transform.</param>
    /// <param name="transform">The transformation to apply.</param>
    /// <returns>The inversely transformed Vector3.</returns>
    public static Vector3 operator *(Vector3 vector, Transform3D transform)
    {
        Vector3 vInv = vector - transform.origin;

        return new Vector3
        (
            (transform.basis.Row0[0] * vInv.x) + (transform.basis.Row1[0] * vInv.y) + (transform.basis.Row2[0] * vInv.z),
            (transform.basis.Row0[1] * vInv.x) + (transform.basis.Row1[1] * vInv.y) + (transform.basis.Row2[1] * vInv.z),
            (transform.basis.Row0[2] * vInv.x) + (transform.basis.Row1[2] * vInv.y) + (transform.basis.Row2[2] * vInv.z)
        );
    }

    /// <summary>
    /// Returns a copy of the given Vector3[] transformed (multiplied) by the transformation matrix.
    /// </summary>
    /// <param name="transform">The transformation to apply.</param>
    /// <param name="array">A Vector3[] to transform.</param>
    /// <returns>The transformed copy of the Vector3[].</returns>
    public static Vector3[] operator *(Transform3D transform, Vector3[] array)
    {
        Vector3[] newArray = new Vector3[array.Length];

        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = transform * array[i];
        }

        return newArray;
    }

    /// <summary>
    /// Returns a copy of the given Vector3[] transformed (multiplied) by the inverse transformation matrix.
    /// </summary>
    /// <param name="array">A Vector3[] to inversely transform.</param>
    /// <param name="transform">The transformation to apply.</param>
    /// <returns>The inversely transformed copy of the Vector3[].</returns>
    public static Vector3[] operator *(Vector3[] array, Transform3D transform)
    {
        Vector3[] newArray = new Vector3[array.Length];

        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = array[i] * transform;
        }

        return newArray;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the transforms are exactly equal.
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="left">The left transform.</param>
    /// <param name="right">The right transform.</param>
    /// <returns>Whether or not the transforms are exactly equal.</returns>
    public static bool operator ==(Transform3D left, Transform3D right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the transforms are not equal.
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="left">The left transform.</param>
    /// <param name="right">The right transform.</param>
    /// <returns>Whether or not the transforms are not equal.</returns>
    public static bool operator !=(Transform3D left, Transform3D right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the transform is exactly equal
    /// to the given object (<see paramref="obj"/>).
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>Whether or not the transform and the object are exactly equal.</returns>
    public override readonly bool Equals(object? obj)
    {
        return obj is Transform3D other && Equals(other);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the transforms are exactly equal.
    /// Note: Due to floating-point precision errors, consider using
    /// <see cref="IsEqualApprox"/> instead, which is more reliable.
    /// </summary>
    /// <param name="other">The other transform to compare.</param>
    /// <returns>Whether or not the matrices are exactly equal.</returns>
    public readonly bool Equals(Transform3D other)
    {
        return basis.Equals(other.basis) && origin.Equals(other.origin);
    }

    /// <summary>
    /// Returns <see langword="true"/> if this transform and <paramref name="other"/> are approximately equal,
    /// by running <see cref="Vector3.IsEqualApprox(Vector3)"/> on each component.
    /// </summary>
    /// <param name="other">The other transform to compare.</param>
    /// <returns>Whether or not the matrices are approximately equal.</returns>
    public bool IsEqualApprox(Transform3D other)
    {
        return basis.IsEqualApprox(other.basis) && origin.IsEqualApprox(other.origin);
    }

    /// <summary>
    /// Serves as the hash function for <see cref="Transform3D"/>.
    /// </summary>
    /// <returns>A hash code for this transform.</returns>
    public override int GetHashCode()
    {
        return basis.GetHashCode() ^ origin.GetHashCode();
    }

    /// <summary>
    /// Converts this <see cref="Transform3D"/> to a string.
    /// </summary>
    /// <returns>A string representation of this transform.</returns>
    public override string ToString()
    {
        return $"[X: {basis.x}, Y: {basis.y}, Z: {basis.z}, O: {origin}]";
    }

    /// <summary>
    /// Converts this <see cref="Transform3D"/> to a string with the given <paramref name="format"/>.
    /// </summary>
    /// <returns>A string representation of this transform.</returns>
    public string ToString(string format)
    {
        return $"[X: {basis.x.ToString(format)}, Y: {basis.y.ToString(format)}, Z: {basis.z.ToString(format)}, O: {origin.ToString(format)}]";
    }
}