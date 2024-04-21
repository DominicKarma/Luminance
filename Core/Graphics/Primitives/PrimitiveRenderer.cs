using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Luminance.Core.Graphics
{
    public class PrimitiveRenderer : ILoadable
    {
        #region Fields/Properties
        private static DynamicVertexBuffer VertexBuffer;

        private static DynamicIndexBuffer IndexBuffer;

        private static IPrimitiveSettings MainSettings;

        private static Vector2[] MainPositions;

        private static VertexPosition2DColorTexture[] MainVertices;

        private static short[] MainIndices;

        private const short MaxTrailPositions = 2000;

        /// <summary>
        /// Must be lower than <see cref="MaxTrailPositions"/>, less than 1/4 of <see cref="MaxVertices"/> and less than 1/6 of <see cref="MaxIndices"/>.
        /// </summary>
        private const short MaxCirclePositions = 1500;

        private const short MaxVertices = 6144;

        private const short MaxIndices = 16384;

        private static short PositionsIndex;

        private static short VerticesIndex;

        private static short IndicesIndex;

        private static readonly short[] QuadIndices = [0, 1, 2, 2, 3, 0];

        private static Matrix QuadVertexMatrix;
        #endregion

        #region General Methods
        void ILoadable.Load(Mod mod)
        {
            Main.QueueMainThreadAction(() =>
            {
                if (Main.netMode == NetmodeID.Server)
                    return;

                MainPositions = new Vector2[MaxTrailPositions];
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
                if (Main.netMode == NetmodeID.Server)
                    return;

                MainPositions = null;
                MainVertices = null;
                MainIndices = null;
                VertexBuffer?.Dispose();
                VertexBuffer = null;
                IndexBuffer?.Dispose();
                IndexBuffer = null;
            });
        }

        private static void PerformPixelationSafetyChecks(IPrimitiveSettings settings)
        {
            // Don't allow accidental screw ups with these.
            if (settings.Pixelate && !PrimitivePixelationSystem.CurrentlyRendering)
                throw new Exception("Error: Primitives using pixelation MUST be prepared/rendered from the IPixelatedPrimitiveRenderer.RenderPixelatedPrimitives method, did you forget to use the interface?");
            else if (!settings.Pixelate && PrimitivePixelationSystem.CurrentlyRendering)
                throw new Exception("Error: Primitives not using pixelation MUST NOT be prepared/rendered from the IPixelatedPrimitiveRenderer.RenderPixelatedPrimitives method.");
        }
        #endregion

        #region Trail Rendering
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

            if (count >= MaxTrailPositions)
                return;

            // IF this is false, a correct position trail could not be made and rendering should not continue.
            if (!AssignPointsRectangleTrail(positions, settings, pointsToCreate ?? count))
                return;

            // A trail with only one point or less has nothing to connect to, and therefore, can't make a trail.
            if (MainPositions.Length <= 2)
                return;

            MainSettings = settings;

            AssignVerticesRectangleTrail(settings);
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

        private static bool AssignPointsRectangleTrail(IEnumerable<Vector2> positions, PrimitiveSettings settings, int pointsToCreate)
        {
            // Don't smoothen the points unless explicitly told do so.
            int positionsCount = positions.Count();
            pointsToCreate = Math.Min(pointsToCreate, MaxTrailPositions - 1);
            if (!settings.Smoothen)
            {
                PositionsIndex = 0;

                // Would like to remove this, but unsure how else to properly ensure that none are zero.
                positions = positions.Where(originalPosition => originalPosition != Vector2.Zero);
                positionsCount = positions.Count();

                if (positionsCount <= 2)
                    return false;

                // Remap the original positions across a certain length.
                for (int i = 0; i < pointsToCreate; i++)
                {
                    float completionRatio = i / (float)(pointsToCreate - 1f);
                    int currentIndex = (int)(completionRatio * (positionsCount - 1));
                    Vector2 currentPoint = positions.ElementAt(currentIndex);
                    Vector2 nextPoint = positions.ElementAt((currentIndex + 1) % positionsCount);
                    MainPositions[PositionsIndex++] = Vector2.Lerp(currentPoint, nextPoint, completionRatio * (positionsCount - 1) % 0.99999f) - Main.screenPosition;
                }
                return true;
            }

            // Due to the first point being manually added, points should be added starting at the second position instead of the first.
            PositionsIndex = 1;

            // Create the control points for the spline.
            List<Vector2> controlPoints = [];
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

                MainPositions[PositionsIndex++] = Vector2.CatmullRom(farLeft, left, right, farRight, localSplineInterpolant);
            }

            // Manually insert the front and end points.
            MainPositions[0] = controlPoints.First();
            MainPositions[PositionsIndex++] = controlPoints.Last();
            return true;
        }

        private static void AssignVerticesRectangleTrail(PrimitiveSettings settings)
        {
            VerticesIndex = 0;
            for (int i = 0; i < PositionsIndex; i++)
            {
                float completionRatio = i / (float)(PositionsIndex - 1);
                float widthAtVertex = settings.WidthFunction(completionRatio);
                Color vertexColor = settings.ColorFunction(completionRatio);
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
                if (i == 0 && settings.InitialVertexPositionsOverride.HasValue && settings.InitialVertexPositionsOverride.Value.Left != Vector2.Zero && settings.InitialVertexPositionsOverride.Value.Right != Vector2.Zero)
                {
                    left = settings.InitialVertexPositionsOverride.Value.Left;
                    right = settings.InitialVertexPositionsOverride.Value.Right;
                }

                // What this is doing, at its core, is defining a rectangle based on two triangles.
                // These triangles are defined based on the width of the strip at that point.
                // The resulting rectangles combined are what make the trail itself.
                MainVertices[VerticesIndex++] = new VertexPosition2DColorTexture(left, vertexColor, leftCurrentTextureCoord, widthAtVertex);
                MainVertices[VerticesIndex++] = new VertexPosition2DColorTexture(right, vertexColor, rightCurrentTextureCoord, widthAtVertex);
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
                MainIndices[IndicesIndex++] = connectToIndex;
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 1);
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 2);
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 2);
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 1);
                MainIndices[IndicesIndex++] = (short)(connectToIndex + 3);
            }
        }

        private static void CalculateUnscaledMatrices(int width, int height, out Matrix viewMatrix, out Matrix projectionMatrix)
        {
            // Due to the scaling, the normal transformation calculations do not work with pixelated primitives.
            projectionMatrix = Matrix.CreateOrthographicOffCenter(0, width, height, 0f, -1f, 1f);
            viewMatrix = Matrix.Identity;
        }
        #endregion

        #region Quad Rendering
        public static void RenderQuad(Texture2D texture, Vector2 center, float scale, float rotation, Color? color = null, ManagedShader shader = null, Quaternion? rotationQuarternion = null)
            => RenderQuad(texture, center, new Vector2(scale), rotation, color, shader, rotationQuarternion);

        public static void RenderQuad(Texture2D texture, Vector2 center, Vector2 scale, float rotation, Color? color = null, ManagedShader shader = null, Quaternion? rotationQuarternion = null)
        {
            var rotationMatrix = rotationQuarternion is null ? Matrix.CreateRotationZ(rotation) : Matrix.CreateFromQuaternion(rotationQuarternion.Value) * Matrix.CreateRotationZ(rotation);
            var scaleMatrix = Matrix.CreateScale(scale.X, scale.Y, 1f);
            var viewMatrix = Matrix.CreateTranslation(new Vector3(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y, 0f))
                * Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -150f, 150f);

            QuadVertexMatrix = rotationMatrix * scaleMatrix * viewMatrix;

            Vector2 quadArea = texture.Size();
            color ??= Lighting.GetColor(center.ToTileCoordinates());

            MainVertices[0] = new(new(0f, -quadArea.Y), color.Value, Vector2.One * 0.01f, 1f);
            MainVertices[1] = new(new(quadArea.X, -quadArea.Y), color.Value, Vector2.UnitX * 0.99f, 1f);
            MainVertices[2] = new(new(quadArea.X, 0f), color.Value, Vector2.One * 0.99f, 1f);
            MainVertices[3] = new(new(0f, 0f), color.Value, Vector2.UnitY * 0.99f, 1f);
            VerticesIndex = 4;

            Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
            Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

            shader ??= ShaderManager.GetShader("Luminance.QuadRenderer");
            shader.TrySetParameter("uWorldViewProjection", QuadVertexMatrix);
            shader.SetTexture(texture, 1, SamplerState.PointClamp);
            shader.Apply();

            VertexBuffer.SetData(MainVertices, 0, VerticesIndex, SetDataOptions.Discard);
            IndexBuffer.SetData(QuadIndices, 0, QuadIndices.Length, SetDataOptions.Discard);

            Main.instance.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            Main.instance.GraphicsDevice.Indices = IndexBuffer;
            Main.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VerticesIndex, 0, 2);
        }
        #endregion

        #region Circle Rendering
        public static void RenderCircle(Vector2 center, PrimitiveSettingsCircle settings, int sideCount = 512)
        {
            if (sideCount <= 0)
                return;

            PerformPixelationSafetyChecks(settings);
            MainSettings = settings;
            sideCount = Math.Min(sideCount, MaxCirclePositions - 1);

            float sideAngle = TwoPi / sideCount;
            float sideLengthMinusRadius = Sqrt(2f - Cos(sideAngle) * 2f);

            VerticesIndex = 0;
            IndicesIndex = 0;

            for (int i = 0; i < sideCount; i++)
            {
                float completionRatio = i / (float)sideCount;
                float nextSideCompletionRatio = (i + 1f) / sideCount;
                float radius = settings.RadiusFunction(completionRatio);
                Color color = settings.ColorFunction(completionRatio);

                Vector2 orthogonal = (TwoPi * completionRatio + PiOver2).ToRotationVector2();
                Vector2 radiusOffset = (TwoPi * completionRatio).ToRotationVector2() * radius;
                Vector2 leftEdge = center + radiusOffset + orthogonal * (sideLengthMinusRadius * radius) * -0.5f;
                Vector2 rightEdge = center + radiusOffset + orthogonal * (sideLengthMinusRadius * radius) * 0.5f;

                MainVertices[VerticesIndex++] = new(leftEdge - Main.screenPosition, color, new(completionRatio, 1f), 1f);
                MainVertices[VerticesIndex++] = new(rightEdge - Main.screenPosition, color, new(nextSideCompletionRatio, 1f), 1f);
                MainVertices[VerticesIndex++] = new(center - Main.screenPosition, color, new(nextSideCompletionRatio, 0f), 1f);
                MainVertices[VerticesIndex++] = new(center - Main.screenPosition, color, new(completionRatio, 0f), 1f);

                MainIndices[IndicesIndex++] = (short)(i * 4);
                MainIndices[IndicesIndex++] = (short)(i * 4 + 1);
                MainIndices[IndicesIndex++] = (short)(i * 4 + 2);
                MainIndices[IndicesIndex++] = (short)(i * 4);
                MainIndices[IndicesIndex++] = (short)(i * 4 + 2);
                MainIndices[IndicesIndex++] = (short)(i * 4 + 3);
            }

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
            Main.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VerticesIndex, 0, sideCount * 2);
        }
        #endregion

        #region Circle Edge Renderer
        public static void RenderCircleEdge(Vector2 center, PrimitiveSettingsCircleEdge settings, int totalPoints = 200)
        {
            if (totalPoints <= 0)
                return;

            PerformPixelationSafetyChecks(settings);
            MainSettings = settings;
            totalPoints = Math.Min(totalPoints, MaxCirclePositions);

            VerticesIndex = 0;
            for (int i = 0; i <= totalPoints; i++)
            {
                float interpolant = i / (float)totalPoints;
                float currentWidth = settings.EdgeWidthFunction(interpolant);
                Color color = settings.ColorFunction(interpolant);
                float radius = settings.RadiusFunction(interpolant);

                Vector2 innerPosition = center - Main.screenPosition + (i * TwoPi / totalPoints).ToRotationVector2() * radius;
                Vector2 outerPosition = center - Main.screenPosition + (i * TwoPi / totalPoints).ToRotationVector2() * (radius + currentWidth);

                MainVertices[VerticesIndex++] = new(innerPosition, color, new(interpolant, 0f), 1f);
                MainVertices[VerticesIndex++] = new(outerPosition, color, new(interpolant, 1f), 1f);
            }

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
            Main.instance.GraphicsDevice.SetVertexBuffer(VertexBuffer);
            Main.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, VerticesIndex - 2);
        }
        #endregion
    }
}
