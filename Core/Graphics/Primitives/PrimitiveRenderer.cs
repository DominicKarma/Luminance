﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class PrimitiveRenderer : ILoadable
    {
        #region Static Members
        private static DynamicVertexBuffer VertexBuffer;

        private static DynamicIndexBuffer IndexBuffer;

        private static PrimitiveSettings MainSettings;

        private static Vector2[] MainPositions;

        private static VertexPosition2DColorTexture[] MainVertices;

        private static short[] MainIndices;

        private const short MaxPositions = 1000;

        private const short MaxVertices = 3072;

        private const short MaxIndices = 8192;

        private static short PositionsIndex;

        private static short VerticesIndex;

        private static short IndicesIndex;

        void ILoadable.Load(Mod mod)
        {
            Main.QueueMainThreadAction(() =>
            {
                MainPositions = new Vector2[MaxPositions];
                MainVertices = new VertexPosition2DColorTexture[MaxVertices];
                MainIndices = new short[MaxIndices];
                VertexBuffer ??= new DynamicVertexBuffer(Main.instance.GraphicsDevice, VertexPosition2DColorTexture.VertexDeclaration2D, MaxVertices, BufferUsage.WriteOnly);
                IndexBuffer ??= new DynamicIndexBuffer(Main.instance.GraphicsDevice, IndexElementSize.SixteenBits, MaxIndices, BufferUsage.WriteOnly);
            });
        }

        void ILoadable.Unload()
        {
            Main.QueueMainThreadAction(() =>
            {
                MainPositions = null;
                MainVertices = null;
                MainIndices = null;
                VertexBuffer?.Dispose();
                VertexBuffer = null;
                IndexBuffer?.Dispose();
                IndexBuffer = null;
            });
        }

        private static void PerformPixelationSafetyChecks(PrimitiveSettings settings)
        {
            // Don't allow accidental screw ups with these.
            if (settings.Pixelate && !PrimitivePixelationSystem.CurrentlyRendering)
                throw new Exception("Error: Primitives using pixelation MUST be prepared/rendered from the IPixelatedPrimitiveRenderer.RenderPixelatedPrimitives method, did you forget to use the interface?");
            else if (!settings.Pixelate && PrimitivePixelationSystem.CurrentlyRendering)
                throw new Exception("Error: Primitives not using pixelation MUST NOT be prepared/rendered from the IPixelatedPrimitiveRenderer.RenderPixelatedPrimitives method.");
        }

        /// <summary>
        /// Renders a primitive trail.
        /// </summary>
        /// <param name="positions">The list of positions to use. Keep in mind that these are expected to be in <b>world position</b>, and <see cref="Main.screenPosition"/> is automatically subtracted from them all.<br/>At least 4 points are required to use smoothing.</param>
        /// <param name="settings">The primitive draw settings to use.</param>
        /// <param name="pointsToCreate">The amount of points to use. More is higher detailed, but less performant. By default, is the number of positions provided. <b>Going above 100 is NOT recommended.</b></param>
        public static void RenderTrail(IEnumerable<Vector2> positions, PrimitiveSettings settings, int? pointsToCreate = null)
        {
            PerformPixelationSafetyChecks(settings);

            int count = positions.Count();
            if (count <= 2)
                return;

            if (count >= MaxPositions)
                return;

            // IF this is false, a correct position trail could not be made and rendering should not continue.
            if (!AssignPointsRectangleTrail(positions, settings, pointsToCreate ?? count))
                return;

            // A trail with only one point or less has nothing to connect to, and therefore, can't make a trail.
            if (MainPositions.Length <= 2)
                return;

            MainSettings = settings;

            AssignVerticesRectangleTrail();
            AssignIndicesRectangleTrail();

            PrivateRender();
        }

        private static void PrivateRender()
        {
            if (IndicesIndex % 6 != 0 || VerticesIndex <= 3)
                return;

            // Perform screen culling, for performance reasons.
            Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            Matrix view;
            Matrix projection;
            int width = MainSettings.ProjectionAreaWidth ?? Main.screenWidth;
            int height = MainSettings.ProjectionAreaHeight ?? Main.screenHeight;
            if (MainSettings.Pixelate || MainSettings.UseUnscaledMatrix)
                CalculateUnscaledMatrices(width, height, out view, out projection);
            else
                CalculatePrimitiveMatrices(width, height, out view, out projection);

            var shaderToUse = MainSettings.Shader ?? ShaderManager.GetShader("Luminance.StandardPrimitiveShader");
            shaderToUse.TrySetParameter("uWorldViewProjection", view * projection);
            shaderToUse.Apply();

            VertexBuffer.SetData(MainVertices, 0, VerticesIndex, SetDataOptions.Discard);
            IndexBuffer.SetData(MainIndices, 0, IndicesIndex, SetDataOptions.Discard);

            Main.instance.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            Main.instance.GraphicsDevice.Indices = IndexBuffer;
            Main.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VerticesIndex, 0, IndicesIndex / 3);
        }
        #endregion

        #region Set Preperation
        private static bool AssignPointsRectangleTrail(IEnumerable<Vector2> positions, PrimitiveSettings settings, int pointsToCreate)
        {
            // Don't smoothen the points unless explicitly told do so.
            int positionsCount = positions.Count();
            if (!settings.Smoothen)
            {
                PositionsIndex = 0;

                // Would like to remove this, but unsure how else to properly ensure that none are zero.
                positions = positions.Where(originalPosition => originalPosition != Vector2.Zero);

                if (positionsCount <= 2)
                    return false;

                // Remap the original positions across a certain length.
                for (int i = 0; i < pointsToCreate; i++)
                {
                    float completionRatio = i / (float)(pointsToCreate - 1f);
                    int currentIndex = (int)(completionRatio * (positionsCount - 1));
                    Vector2 currentPoint = positions.ElementAt(currentIndex);
                    Vector2 nextPoint = positions.ElementAt((currentIndex + 1) % positionsCount);
                    MainPositions[PositionsIndex] = Vector2.Lerp(currentPoint, nextPoint, completionRatio * (positionsCount - 1) % 0.99999f) - Main.screenPosition;
                    PositionsIndex++;
                }
                return true;
            }

            // Due to the first point being manually added, points should be added starting at the second position instead of the first.
            PositionsIndex = 1;

            // Create the control points for the spline.
            List<Vector2> controlPoints = new();
            int index = 0;
            foreach (var position in positions)
            {
                // Don't incorporate points that are zeroed out.
                // They are almost certainly a result of incomplete oldPos arrays.
                if (position == Vector2.Zero)
                    continue;

                float completionRatio = index / (float)positionsCount;
                Vector2 offset = -Main.screenPosition;
                if (settings.OffsetFunction != null)
                    offset += settings.OffsetFunction(completionRatio);
                controlPoints.Add(position + offset);
                index++;
            }

            // Avoid stupid index errors.
            if (controlPoints.Count <= 4)
                return false;

            for (int j = 0; j < pointsToCreate; j++)
            {
                float splineInterpolant = j / (float)pointsToCreate;
                float localSplineInterpolant = splineInterpolant * (controlPoints.Count - 1f) % 1f;
                int localSplineIndex = (int)(splineInterpolant * (controlPoints.Count - 1f));

                Vector2 farLeft;
                Vector2 left = controlPoints[localSplineIndex];
                Vector2 right = controlPoints[localSplineIndex + 1];
                Vector2 farRight;

                // Special case: If the spline attempts to access the previous/next index but the index is already at the very beginning/end, simply
                // cheat a little bit by creating a phantom point that's mirrored from the previous one.
                if (localSplineIndex <= 0)
                {
                    Vector2 mirrored = left * 2f - right;
                    farLeft = mirrored;
                }
                else
                    farLeft = controlPoints[localSplineIndex - 1];

                if (localSplineIndex >= controlPoints.Count - 2)
                {
                    Vector2 mirrored = right * 2f - left;
                    farRight = mirrored;
                }
                else
                    farRight = controlPoints[localSplineIndex + 2];

                MainPositions[PositionsIndex] = Vector2.CatmullRom(farLeft, left, right, farRight, localSplineInterpolant);
                PositionsIndex++;
            }

            // Manually insert the front and end points.
            MainPositions[0] = controlPoints.First();
            MainPositions[PositionsIndex] = controlPoints.Last();
            PositionsIndex++;
            return true;
        }

        private static void AssignVerticesRectangleTrail()
        {
            VerticesIndex = 0;
            for (int i = 0; i < PositionsIndex; i++)
            {
                float completionRatio = i / (float)(PositionsIndex - 1);
                float widthAtVertex = MainSettings.WidthFunction(completionRatio);
                Color vertexColor = MainSettings.ColorFunction(completionRatio);
                Vector2 currentPosition = MainPositions[i];
                Vector2 directionToAhead = i == PositionsIndex - 1 ? (MainPositions[i] - MainPositions[i - 1]).SafeNormalize(Vector2.Zero) : (MainPositions[i + 1] - MainPositions[i]).SafeNormalize(Vector2.Zero);

                Vector2 leftCurrentTextureCoord = new(completionRatio, 0.5f - widthAtVertex * 0.5f);
                Vector2 rightCurrentTextureCoord = new(completionRatio, 0.5f + widthAtVertex * 0.5f);

                // Point 90 degrees away from the direction towards the next point, and use it to mark the edges of the rectangle.
                // This doesn't use RotatedBy for the sake of performance (there can potentially be a lot of trail points).
                Vector2 sideDirection = new(-directionToAhead.Y, directionToAhead.X);

                Vector2 left = currentPosition - sideDirection * widthAtVertex;
                Vector2 right = currentPosition + sideDirection * widthAtVertex;

                // Override the initial vertex positions if requested.
                if (i == 0 && MainSettings.InitialVertexPositionsOverride.HasValue && MainSettings.InitialVertexPositionsOverride.Value.Left != Vector2.Zero && MainSettings.InitialVertexPositionsOverride.Value.Right != Vector2.Zero)
                {
                    left = MainSettings.InitialVertexPositionsOverride.Value.Left;
                    right = MainSettings.InitialVertexPositionsOverride.Value.Right;
                }

                // What this is doing, at its core, is defining a rectangle based on two triangles.
                // These triangles are defined based on the width of the strip at that point.
                // The resulting rectangles combined are what make the trail itself.
                MainVertices[VerticesIndex] = new VertexPosition2DColorTexture(left, vertexColor, leftCurrentTextureCoord, widthAtVertex);
                VerticesIndex++;
                MainVertices[VerticesIndex] = new VertexPosition2DColorTexture(right, vertexColor, rightCurrentTextureCoord, widthAtVertex);
                VerticesIndex++;
            }
        }

        private static void AssignIndicesRectangleTrail()
        {
            // What this is doing is basically representing each point on the vertices list as
            // indices. These indices should come together to create a tiny rectangle that acts
            // as a segment on the trail. This is achieved here by splitting the indices (or rather, points)
            // into 2 triangles, which requires 6 points.
            // The logic here basically determines which indices are connected together.
            IndicesIndex = 0;
            for (short i = 0; i < PositionsIndex - 2; i++)
            {
                short connectToIndex = (short)(i * 2);
                MainIndices[IndicesIndex] = connectToIndex;
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 1);
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 2);
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 2);
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 1);
                IndicesIndex++;

                MainIndices[IndicesIndex] = (short)(connectToIndex + 3);
                IndicesIndex++;
            }
        }

        private static void CalculateUnscaledMatrices(int width, int height, out Matrix viewMatrix, out Matrix projectionMatrix)
        {
            // Due to the scaling, the normal transformation calculations do not work with pixelated primitives.
            projectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, height, 0f, -1f, 1f);
            viewMatrix = Matrix.Identity;
        }
        #endregion
    }
}
