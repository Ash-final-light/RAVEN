using System;
using UnityEngine;

namespace Tools.Action.Data.Collide.EditorHelp
{
    public class DebugDrawMeshCollide
    {
        public static Mesh Generate2DSectorMesh(
            float insideRadius,
            float radius,
            float anglePivot,
            float angle,
            Vector3 forwardDirection,
            float deltaAngle = 2f
        )
        {
            return GenerateSectorTopAndBottomSurfaceMesh(insideRadius, radius, anglePivot, angle, forwardDirection, 0f,
                    deltaAngle)
                .mesh;
        }

        private static (Mesh mesh, int rectCount, int lineCount) GenerateSectorTopAndBottomSurfaceMesh(
            float insideRadius,
            float radius,
            float anglePivot,
            float angle,
            Vector3 forwardDirection,
            float height,
            float deltaAngle = 2f
        )
        {
            // 检查数据
            radius = Mathf.Max(radius, 0f);
            insideRadius = Mathf.Clamp(insideRadius, 0f, radius);
            height = Mathf.Abs(height);

            var mesh = new Mesh();
            var position = Vector3.zero;
            var startLineDirection =
                Quaternion.AngleAxis(anglePivot + angle / 2, Vector3.up) * forwardDirection; // 起始线方向
            var rectCount = (int)(angle / deltaAngle);
            var lineCount = rectCount + 1;

            // 扇形顶点数组
            var vertexes = new Vector3[lineCount * 2 * 2];
            // 扇形三角形顶点索引数组
            var triangleVertexIndexes = new int[rectCount * 6 * 2];

            // 计算底面
            for (var i = 0; i < lineCount; i++)
            {
                // 计算扇形分区的顶点
                var lineDirection = Quaternion.AngleAxis(-i * deltaAngle, Vector3.up) * startLineDirection;
                var insideVertex = position + insideRadius * lineDirection - Vector3.up * height / 2;
                var outsideVertex = position + radius * lineDirection - Vector3.up * height / 2;
                vertexes[i * 2] = insideVertex;
                vertexes[i * 2 + 1] = outsideVertex;

                // 计算扇形分区的三角形顶点索引
                if (i < lineCount - 1)
                {
                    triangleVertexIndexes[i * 6] = i * 2 + 1;
                    triangleVertexIndexes[i * 6 + 1] = i * 2 + 2;
                    triangleVertexIndexes[i * 6 + 2] = i * 2;
                    triangleVertexIndexes[i * 6 + 3] = i * 2 + 1;
                    triangleVertexIndexes[i * 6 + 4] = i * 2 + 3;
                    triangleVertexIndexes[i * 6 + 5] = i * 2 + 2;
                }
            }

            // 计算顶面
            for (var i = lineCount; i < 2 * lineCount; i++)
            {
                // 计算扇形分区的顶点
                var lineDirection = Quaternion.AngleAxis(-(i - lineCount) * deltaAngle, Vector3.up) *
                                    startLineDirection;
                var insideVertex = position + insideRadius * lineDirection + Vector3.up * height / 2;
                var outsideVertex = position + radius * lineDirection + Vector3.up * height / 2;
                vertexes[i * 2] = insideVertex;
                vertexes[i * 2 + 1] = outsideVertex;

                // 计算扇形分区的三角形顶点索引
                if (i < 2 * lineCount - 1)
                {
                    triangleVertexIndexes[(i - 1) * 6] = i * 2 + 1;
                    triangleVertexIndexes[(i - 1) * 6 + 1] = i * 2;
                    triangleVertexIndexes[(i - 1) * 6 + 2] = i * 2 + 2;
                    triangleVertexIndexes[(i - 1) * 6 + 3] = i * 2 + 1;
                    triangleVertexIndexes[(i - 1) * 6 + 4] = i * 2 + 2;
                    triangleVertexIndexes[(i - 1) * 6 + 5] = i * 2 + 3;
                }
            }

            mesh.vertices = vertexes;
            mesh.triangles = triangleVertexIndexes;
            mesh.RecalculateNormals();

            return (mesh, rectCount, lineCount);
        }

        public static Mesh Generate3DSectorMesh(
            float insideRadius,
            float radius,
            float anglePivot,
            float angle,
            Vector3 forwardDirection,
            float height,
            float deltaAngle = 2f
        )
        {
            var mesh = new Mesh();
            var result =
                GenerateSectorTopAndBottomSurfaceMesh(insideRadius, radius, anglePivot, angle, forwardDirection, height,
                    deltaAngle);
            var rectCount = result.rectCount;
            var oldSectorMesh = result.mesh;
            var oldVertexes = oldSectorMesh.vertices;
            var oldTriangleVertexIndexes = oldSectorMesh.triangles;
            var newTriangleVertexIndexes = new int[oldTriangleVertexIndexes.Length + 3 * 2 * 2 + 3 * 2 * rectCount * 2];
            Array.Copy(oldTriangleVertexIndexes, newTriangleVertexIndexes, oldTriangleVertexIndexes.Length);

            // 计算扇形起始侧面的三角形顶点索引
            {
                var start = oldTriangleVertexIndexes.Length;
                newTriangleVertexIndexes[start] = 0;
                newTriangleVertexIndexes[start + 1] = oldVertexes.Length / 2;
                newTriangleVertexIndexes[start + 2] = 1;
                newTriangleVertexIndexes[start + 3] = 1;
                newTriangleVertexIndexes[start + 4] = oldVertexes.Length / 2;
                newTriangleVertexIndexes[start + 5] = oldVertexes.Length / 2 + 1;
            }

            // 计算扇形结束侧面的三角形顶点索引
            {
                var start = oldTriangleVertexIndexes.Length + 3 * 2;
                newTriangleVertexIndexes[start] = oldVertexes.Length / 2 - 2;
                newTriangleVertexIndexes[start + 1] = oldVertexes.Length / 2 - 1;
                newTriangleVertexIndexes[start + 2] = oldVertexes.Length - 2;
                newTriangleVertexIndexes[start + 3] = oldVertexes.Length / 2 - 1;
                newTriangleVertexIndexes[start + 4] = oldVertexes.Length - 1;
                newTriangleVertexIndexes[start + 5] = oldVertexes.Length - 2;
            }

            // 计算扇形内侧面的三角形顶点索引
            {
                var start = oldTriangleVertexIndexes.Length + 3 * 2 * 2;
                for (int i = 0; i < rectCount; i++)
                {
                    newTriangleVertexIndexes[start + i * 6] = i * 2;
                    newTriangleVertexIndexes[start + i * 6 + 1] = (i + 1) * 2 + oldVertexes.Length / 2;
                    newTriangleVertexIndexes[start + i * 6 + 2] = i * 2 + oldVertexes.Length / 2;
                    newTriangleVertexIndexes[start + i * 6 + 3] = i * 2;
                    newTriangleVertexIndexes[start + i * 6 + 4] = (i + 1) * 2;
                    newTriangleVertexIndexes[start + i * 6 + 5] = (i + 1) * 2 + oldVertexes.Length / 2;
                }
            }

            // 计算扇形外侧面的三角形顶点索引
            {
                var start = oldTriangleVertexIndexes.Length + 3 * 2 * 2 + 3 * 2 * rectCount;
                for (int i = 0; i < rectCount; i++)
                {
                    newTriangleVertexIndexes[start + i * 6] = 1 + i * 2;
                    newTriangleVertexIndexes[start + i * 6 + 1] = 1 + i * 2 + oldVertexes.Length / 2;
                    newTriangleVertexIndexes[start + i * 6 + 2] = 1 + (i + 1) * 2 + oldVertexes.Length / 2;
                    newTriangleVertexIndexes[start + i * 6 + 3] = 1 + i * 2;
                    ;
                    newTriangleVertexIndexes[start + i * 6 + 4] = 1 + (i + 1) * 2 + oldVertexes.Length / 2;
                    newTriangleVertexIndexes[start + i * 6 + 5] = 1 + (i + 1) * 2;
                }
            }

            mesh.vertices = oldVertexes;
            mesh.triangles = newTriangleVertexIndexes;
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}