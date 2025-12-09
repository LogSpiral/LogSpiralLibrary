namespace LogSpiralLibrary.CodeLibrary.Utilties;

/// <summary>
/// 几何碰撞检测辅助类<br/>
/// 直接问DeepSeek把作业写完.png
/// </summary>
public static class CollisionHelper
{
    #region 辅助结构和枚举

    /// <summary>
    /// 表示圆
    /// </summary>
    public struct Circle(Vector2 center, float radius)
    {
        public Vector2 Center = center;
        public float Radius = radius;
    }

    /// <summary>
    /// 表示正交矩形
    /// </summary>
    public struct OrthogonalRectangle(Vector2 topLeft, float width, float height)
    {
        public Vector2 TopLeft = topLeft;
        public float Width = width;
        public float Height = height;

        public readonly Vector2 BottomRight => TopLeft + new Vector2(Width, Height);
        public readonly Vector2 Center => TopLeft + new Vector2(Width * 0.5f, Height * 0.5f);

        public static explicit operator OrthogonalRectangle(Rectangle rectangle) 
        {
            return new OrthogonalRectangle(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
        }

        public static explicit operator Rectangle(OrthogonalRectangle rectangle) 
        {
            return new Rectangle((int)rectangle.TopLeft.X, (int)rectangle.TopLeft.Y, (int)rectangle.Width, (int)rectangle.Height);
        }

        public static OrthogonalRectangle FromRectangle(Rectangle rectangle) => (OrthogonalRectangle)rectangle;

        public readonly Rectangle ToRectangle() => (Rectangle)this;
    }

    /// <summary>
    /// 表示扇形
    /// </summary>
    public struct Sector(Vector2 center, float radius, float startAngle, float sweepAngle)
    {
        public Vector2 Center = center;
        public float Radius = radius;
        public float StartAngle = startAngle;  // 起始角度（弧度）
        public float SweepAngle = sweepAngle;  // 扫描角度（弧度）
        public Vector2 Direction = new((float)Math.Cos(startAngle), (float)Math.Sin(startAngle)); // 方向向量
    }

    /// <summary>
    /// 表示椭圆
    /// </summary>
    public struct Ellipse(Vector2 center, float radiusX, float radiusY, float rotation = 0)
    {
        public Vector2 Center = center;
        public float RadiusX = radiusX;     // X轴半径
        public float RadiusY = radiusY;     // Y轴半径
        public float Rotation = rotation;    // 旋转角度（弧度）
    }

    /// <summary>
    /// 表示椭圆扇形
    /// </summary>
    public struct EllipticSector(Vector2 center, float radiusX, float radiusY,
                         float rotation, float startAngle, float sweepAngle)
    {
        public Vector2 Center = center;
        public float RadiusX = radiusX;     // X轴半径
        public float RadiusY = radiusY;     // Y轴半径
        public float Rotation = rotation;    // 椭圆旋转角度（弧度）
        public float StartAngle = startAngle;  // 起始角度（弧度，在椭圆坐标系中）
        public float SweepAngle = sweepAngle;  // 扫描角度（弧度）
    }

    #endregion

    #region 基础辅助方法

    /// <summary>
    /// 将点从世界坐标系转换到椭圆局部坐标系
    /// </summary>
    private static Vector2 TransformToEllipseLocal(Vector2 point, Vector2 ellipseCenter,
                                                 float ellipseRotation, float scaleX = 1, float scaleY = 1)
    {
        // 平移
        Vector2 translated = point - ellipseCenter;

        // 旋转（反向旋转）
        float cos = (float)Math.Cos(-ellipseRotation);
        float sin = (float)Math.Sin(-ellipseRotation);
        Vector2 rotated = new Vector2(
            translated.X * cos - translated.Y * sin,
            translated.X * sin + translated.Y * cos
        );

        // 缩放
        if (Math.Abs(scaleX) > float.Epsilon && Math.Abs(scaleY) > float.Epsilon)
        {
            return new Vector2(rotated.X / scaleX, rotated.Y / scaleY);
        }

        return rotated;
    }

    /// <summary>
    /// 检查点是否在矩形内
    /// </summary>
    private static bool IsPointInRectangle(Vector2 point, OrthogonalRectangle rect)
    {
        return point.X >= rect.TopLeft.X &&
               point.X <= rect.TopLeft.X + rect.Width &&
               point.Y >= rect.TopLeft.Y &&
               point.Y <= rect.TopLeft.Y + rect.Height;
    }

    /// <summary>
    /// 获取点到线段的最短距离平方
    /// </summary>
    private static float DistanceSquaredToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 line = segmentEnd - segmentStart;
        float lineLengthSquared = line.LengthSquared();

        if (lineLengthSquared == 0)
            return Vector2.DistanceSquared(point, segmentStart);

        float t = Math.Max(0, Math.Min(1, Vector2.Dot(point - segmentStart, line) / lineLengthSquared));
        Vector2 projection = segmentStart + t * line;
        return Vector2.DistanceSquared(point, projection);
    }

    /// <summary>
    /// 获取点到矩形的最短距离平方
    /// </summary>
    private static float DistanceSquaredToRectangle(Vector2 point, OrthogonalRectangle rect)
    {
        // 如果点在矩形内，距离为0
        if (IsPointInRectangle(point, rect))
            return 0;

        // 计算到四条边的最短距离
        float left = rect.TopLeft.X;
        float right = rect.TopLeft.X + rect.Width;
        float top = rect.TopLeft.Y;
        float bottom = rect.TopLeft.Y + rect.Height;

        float dx = Math.Max(left - point.X, Math.Max(point.X - right, 0));
        float dy = Math.Max(top - point.Y, Math.Max(point.Y - bottom, 0));

        return dx * dx + dy * dy;
    }

    /// <summary>
    /// 检查角度是否在扇形角度范围内
    /// </summary>
    private static bool IsAngleInSector(float angle, float startAngle, float sweepAngle)
    {
        // 规范化角度到 [0, 2π)
        angle = NormalizeAngle(angle);
        startAngle = NormalizeAngle(startAngle);

        float endAngle = NormalizeAngle(startAngle + sweepAngle);

        if (sweepAngle >= 0)
        {
            if (startAngle <= endAngle)
                return angle >= startAngle && angle <= endAngle;
            else
                return angle >= startAngle || angle <= endAngle;
        }
        else
        {
            if (endAngle <= startAngle)
                return angle >= endAngle && angle <= startAngle;
            else
                return angle >= endAngle || angle <= startAngle;
        }
    }

    /// <summary>
    /// 规范化角度到 [0, 2π)
    /// </summary>
    private static float NormalizeAngle(float angle)
    {
        angle = angle % (2 * (float)Math.PI);
        if (angle < 0)
            angle += 2 * (float)Math.PI;
        return angle;
    }

    /// <summary>
    /// 获取矩形的四个顶点
    /// </summary>
    private static Vector2[] GetRectangleVertices(OrthogonalRectangle rect)
    {
        return new Vector2[]
        {
            rect.TopLeft,
            rect.TopLeft + new Vector2(rect.Width, 0),
            rect.TopLeft + new Vector2(rect.Width, rect.Height),
            rect.TopLeft + new Vector2(0, rect.Height)
        };
    }

    #endregion

    #region 1. 圆和正交矩形碰撞检测

    /// <summary>
    /// 检查圆和正交矩形是否碰撞
    /// </summary>
    public static bool CheckCircleAndRectangle(Circle circle, OrthogonalRectangle rect)
    {
        // 方法1：计算圆心到矩形的最短距离
        float distanceSquared = DistanceSquaredToRectangle(circle.Center, rect);
        return distanceSquared <= circle.Radius * circle.Radius;

        // 方法2：分离轴定理（SAT）的简化版本
        // return CheckCircleAndRectangleSAT(circle, rect);
    }

    /// <summary>
    /// 使用分离轴定理检查圆和矩形碰撞
    /// </summary>
    private static bool CheckCircleAndRectangleSAT(Circle circle, OrthogonalRectangle rect)
    {
        Vector2 rectCenter = rect.Center;
        Vector2 circleCenter = circle.Center;

        // 计算矩形半宽高
        float halfWidth = rect.Width * 0.5f;
        float halfHeight = rect.Height * 0.5f;

        // 计算圆心到矩形中心的向量
        Vector2 difference = circleCenter - rectCenter;

        // 将向量限制在矩形边界内
        float clampedX = Math.Clamp(difference.X, -halfWidth, halfWidth);
        float clampedY = Math.Clamp(difference.Y, -halfHeight, halfHeight);

        // 计算最近点
        Vector2 closest = rectCenter + new Vector2(clampedX, clampedY);

        // 检查最近点是否在圆内
        Vector2 distance = circleCenter - closest;
        return distance.LengthSquared() <= circle.Radius * circle.Radius;
    }

    #endregion

    #region 2. 扇形和正交矩形碰撞检测

    /// <summary>
    /// 检查扇形和正交矩形是否碰撞
    /// </summary>
    public static bool CheckSectorAndRectangle(Sector sector, OrthogonalRectangle rect)
    {
        // 1. 检查矩形顶点是否在扇形内
        Vector2[] vertices = GetRectangleVertices(rect);
        foreach (var vertex in vertices)
        {
            if (IsPointInSector(vertex, sector))
                return true;
        }

        // 2. 检查扇形弧与矩形边是否相交
        if (CheckSectorArcAndRectangle(sector, rect))
            return true;

        // 3. 检查扇形两条半径与矩形边是否相交
        float endAngle = sector.StartAngle + sector.SweepAngle;
        Vector2 radiusStart = new Vector2(
            (float)Math.Cos(sector.StartAngle),
            (float)Math.Sin(sector.StartAngle)
        ) * sector.Radius + sector.Center;

        Vector2 radiusEnd = new Vector2(
            (float)Math.Cos(endAngle),
            (float)Math.Sin(endAngle)
        ) * sector.Radius + sector.Center;

        // 检查两条半径线段与矩形边的相交
        if (CheckLineSegmentAndRectangle(radiusStart, sector.Center, rect) ||
            CheckLineSegmentAndRectangle(radiusEnd, sector.Center, rect))
            return true;

        // 4. 检查扇形中心是否在矩形内
        if (IsPointInRectangle(sector.Center, rect))
            return true;

        return false;
    }

    /// <summary>
    /// 检查点是否在扇形内
    /// </summary>
    private static bool IsPointInSector(Vector2 point, Sector sector)
    {
        // 计算点到扇形中心的向量
        Vector2 toPoint = point - sector.Center;
        float distanceSquared = toPoint.LengthSquared();

        // 检查距离
        if (distanceSquared > sector.Radius * sector.Radius)
            return false;

        // 计算角度
        float angle = (float)Math.Atan2(toPoint.Y, toPoint.X);
        angle = NormalizeAngle(angle);

        // 检查角度是否在扇形范围内
        return IsAngleInSector(angle, sector.StartAngle, sector.SweepAngle);
    }

    /// <summary>
    /// 检查扇形弧与矩形是否相交
    /// </summary>
    private static bool CheckSectorArcAndRectangle(Sector sector, OrthogonalRectangle rect)
    {
        // 将矩形边离散化，检查离散点是否在扇形弧附近
        // 这是一个简化方法，实际应用中可能需要更精确的算法

        float segmentCount = 32;
        float angleStep = sector.SweepAngle / segmentCount;

        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = sector.StartAngle + i * angleStep;
            Vector2 arcPoint = sector.Center + new Vector2(
                (float)Math.Cos(currentAngle) * sector.Radius,
                (float)Math.Sin(currentAngle) * sector.Radius
            );

            // 如果弧上的点在矩形内或附近，认为相交
            if (IsPointInRectangle(arcPoint, rect) ||
                DistanceSquaredToRectangle(arcPoint, rect) < 0.01f)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 检查线段与矩形是否相交
    /// </summary>
    private static bool CheckLineSegmentAndRectangle(Vector2 p1, Vector2 p2, OrthogonalRectangle rect)
    {
        // 检查线段端点是否在矩形内
        if (IsPointInRectangle(p1, rect) || IsPointInRectangle(p2, rect))
            return true;

        // 检查与矩形四条边的相交
        Vector2[] rectVertices = GetRectangleVertices(rect);

        for (int i = 0; i < 4; i++)
        {
            Vector2 a = rectVertices[i];
            Vector2 b = rectVertices[(i + 1) % 4];

            if (DoLinesIntersect(p1, p2, a, b))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 检查两条线段是否相交
    /// </summary>
    private static bool DoLinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float d = (a2.X - a1.X) * (b2.Y - b1.Y) - (a2.Y - a1.Y) * (b2.X - b1.X);

        if (Math.Abs(d) < float.Epsilon)
            return false;

        float u = ((b1.X - a1.X) * (b2.Y - b1.Y) - (b1.Y - a1.Y) * (b2.X - b1.X)) / d;
        float v = ((b1.X - a1.X) * (a2.Y - a1.Y) - (b1.Y - a1.Y) * (a2.X - a1.X)) / d;

        return u >= 0 && u <= 1 && v >= 0 && v <= 1;
    }

    #endregion

    #region 3. 椭圆和正交矩形碰撞检测

    /// <summary>
    /// 检查椭圆和正交矩形是否碰撞
    /// </summary>
    public static bool CheckEllipseAndRectangle(Ellipse ellipse, OrthogonalRectangle rect)
    {
        // 方法：将矩形顶点转换到椭圆的局部坐标系（单位圆）
        // 在椭圆局部坐标系中，椭圆变为单位圆

        // 1. 检查矩形顶点是否在椭圆内
        Vector2[] vertices = GetRectangleVertices(rect);
        foreach (var vertex in vertices)
        {
            Vector2 localPoint = TransformToEllipseLocal(vertex, ellipse.Center,
                                                       ellipse.Rotation, ellipse.RadiusX, ellipse.RadiusY);

            // 在局部坐标系中检查是否在单位圆内
            if (localPoint.LengthSquared() <= 1.0f)
                return true;
        }

        // 2. 检查椭圆是否完全包含矩形
        // 检查矩形的四个顶点是否都在椭圆外但椭圆中心在矩形内
        if (IsPointInRectangle(ellipse.Center, rect))
        {
            // 椭圆中心在矩形内，一定相交
            return true;
        }

        // 3. 检查椭圆与矩形边的相交
        // 将矩形边离散化，检查点是否在椭圆内
        float segmentCount = 16;

        // 检查四条边
        for (int side = 0; side < 4; side++)
        {
            Vector2 start, end;
            switch (side)
            {
                case 0: // 上边
                    start = rect.TopLeft;
                    end = rect.TopLeft + new Vector2(rect.Width, 0);
                    break;
                case 1: // 右边
                    start = rect.TopLeft + new Vector2(rect.Width, 0);
                    end = rect.TopLeft + new Vector2(rect.Width, rect.Height);
                    break;
                case 2: // 下边
                    start = rect.TopLeft + new Vector2(rect.Width, rect.Height);
                    end = rect.TopLeft + new Vector2(0, rect.Height);
                    break;
                default: // 左边
                    start = rect.TopLeft + new Vector2(0, rect.Height);
                    end = rect.TopLeft;
                    break;
            }

            for (int i = 0; i <= segmentCount; i++)
            {
                float t = i / segmentCount;
                Vector2 point = Vector2.Lerp(start, end, t);
                Vector2 localPoint = TransformToEllipseLocal(point, ellipse.Center,
                                                           ellipse.Rotation, ellipse.RadiusX, ellipse.RadiusY);

                if (localPoint.LengthSquared() <= 1.0f)
                    return true;
            }
        }

        return false;
    }

    #endregion

    #region 4. 椭圆扇形和正交矩形碰撞检测

    /// <summary>
    /// 检查椭圆扇形和正交矩形是否碰撞
    /// </summary>
    public static bool CheckEllipticSectorAndRectangle(EllipticSector ellipticSector, OrthogonalRectangle rect)
    {
        // 方法：将矩形顶点转换到椭圆扇形的局部坐标系
        // 在局部坐标系中，椭圆扇形变为单位圆扇形

        // 1. 检查矩形顶点是否在椭圆扇形内
        Vector2[] vertices = GetRectangleVertices(rect);
        foreach (var vertex in vertices)
        {
            if (IsPointInEllipticSector(vertex, ellipticSector))
                return true;
        }

        // 2. 检查椭圆扇形弧与矩形边是否相交
        if (CheckEllipticSectorArcAndRectangle(ellipticSector, rect))
            return true;

        // 3. 检查椭圆扇形两条半径与矩形边是否相交
        // 在椭圆局部坐标系中计算半径端点
        Vector2 localRadiusStart = new Vector2(
            (float)Math.Cos(ellipticSector.StartAngle),
            (float)Math.Sin(ellipticSector.StartAngle)
        );

        Vector2 localRadiusEnd = new Vector2(
            (float)Math.Cos(ellipticSector.StartAngle + ellipticSector.SweepAngle),
            (float)Math.Sin(ellipticSector.StartAngle + ellipticSector.SweepAngle)
        );

        // 转换回世界坐标系
        Vector2 worldRadiusStart = TransformFromEllipseLocal(localRadiusStart, ellipticSector.Center,
                                                            ellipticSector.Rotation, ellipticSector.RadiusX, ellipticSector.RadiusY);

        Vector2 worldRadiusEnd = TransformFromEllipseLocal(localRadiusEnd, ellipticSector.Center,
                                                          ellipticSector.Rotation, ellipticSector.RadiusX, ellipticSector.RadiusY);

        // 检查两条半径线段与矩形边的相交
        if (CheckLineSegmentAndRectangle(worldRadiusStart, ellipticSector.Center, rect) ||
            CheckLineSegmentAndRectangle(worldRadiusEnd, ellipticSector.Center, rect))
            return true;

        // 4. 检查椭圆扇形中心是否在矩形内
        if (IsPointInRectangle(ellipticSector.Center, rect))
            return true;

        return false;
    }

    /// <summary>
    /// 检查点是否在椭圆扇形内
    /// </summary>
    private static bool IsPointInEllipticSector(Vector2 point, EllipticSector ellipticSector)
    {
        // 将点转换到椭圆局部坐标系
        Vector2 localPoint = TransformToEllipseLocal(point, ellipticSector.Center,
                                                   ellipticSector.Rotation, ellipticSector.RadiusX, ellipticSector.RadiusY);

        // 检查距离（在局部坐标系中是单位圆）
        if (localPoint.LengthSquared() > 1.0f)
            return false;

        // 计算角度
        float angle = (float)Math.Atan2(localPoint.Y, localPoint.X);
        angle = NormalizeAngle(angle);

        // 检查角度是否在扇形范围内
        return IsAngleInSector(angle, ellipticSector.StartAngle, ellipticSector.SweepAngle);
    }

    /// <summary>
    /// 从椭圆局部坐标系转换回世界坐标系
    /// </summary>
    private static Vector2 TransformFromEllipseLocal(Vector2 localPoint, Vector2 ellipseCenter,
                                                    float ellipseRotation, float scaleX, float scaleY)
    {
        // 缩放
        Vector2 scaled = new Vector2(localPoint.X * scaleX, localPoint.Y * scaleY);

        // 旋转
        float cos = (float)Math.Cos(ellipseRotation);
        float sin = (float)Math.Sin(ellipseRotation);
        Vector2 rotated = new Vector2(
            scaled.X * cos - scaled.Y * sin,
            scaled.X * sin + scaled.Y * cos
        );

        // 平移
        return rotated + ellipseCenter;
    }

    /// <summary>
    /// 检查椭圆扇形弧与矩形是否相交
    /// </summary>
    private static bool CheckEllipticSectorArcAndRectangle(EllipticSector ellipticSector, OrthogonalRectangle rect)
    {
        // 将椭圆扇形弧离散化，检查离散点是否在矩形内或附近
        float segmentCount = 32;
        float angleStep = ellipticSector.SweepAngle / segmentCount;

        for (int i = 0; i <= segmentCount; i++)
        {
            float currentAngle = ellipticSector.StartAngle + i * angleStep;

            // 在局部坐标系中的弧点
            Vector2 localArcPoint = new Vector2(
                (float)Math.Cos(currentAngle),
                (float)Math.Sin(currentAngle)
            );

            // 转换到世界坐标系
            Vector2 worldArcPoint = TransformFromEllipseLocal(localArcPoint, ellipticSector.Center,
                                                            ellipticSector.Rotation, ellipticSector.RadiusX, ellipticSector.RadiusY);

            // 检查点是否在矩形内或附近
            if (IsPointInRectangle(worldArcPoint, rect) ||
                DistanceSquaredToRectangle(worldArcPoint, rect) < 0.01f)
                return true;
        }

        return false;
    }

    #endregion

    #region 优化版本（使用更精确的算法）

    /// <summary>
    /// 优化的圆和矩形碰撞检测（使用投影法）
    /// </summary>
    public static bool CheckCircleAndRectangleOptimized(Circle circle, OrthogonalRectangle rect)
    {
        Vector2 circleCenter = circle.Center;
        Vector2 rectCenter = rect.Center;

        // 计算矩形半宽高
        float halfWidth = rect.Width * 0.5f;
        float halfHeight = rect.Height * 0.5f;

        // 计算圆心到矩形中心的向量
        Vector2 difference = circleCenter - rectCenter;

        // 将向量限制在矩形边界内
        float clampedX = Math.Clamp(difference.X, -halfWidth, halfWidth);
        float clampedY = Math.Clamp(difference.Y, -halfHeight, halfHeight);

        // 计算最近点
        Vector2 closest = rectCenter + new Vector2(clampedX, clampedY);

        // 检查最近点是否在圆内
        Vector2 distance = circleCenter - closest;
        return distance.LengthSquared() <= circle.Radius * circle.Radius;
    }

    /// <summary>
    /// 优化的椭圆和矩形碰撞检测（使用分离轴定理）
    /// </summary>
    public static bool CheckEllipseAndRectangleOptimized(Ellipse ellipse, OrthogonalRectangle rect)
    {
        // 将问题转化为：检查矩形是否与椭圆的外接圆相交
        float maxRadius = Math.Max(ellipse.RadiusX, ellipse.RadiusY);
        Circle boundingCircle = new Circle(ellipse.Center, maxRadius);

        // 先进行外接圆检测
        if (!CheckCircleAndRectangleOptimized(boundingCircle, rect))
            return false;

        // 如果外接圆相交，进行更精确的检测
        // 将矩形顶点转换到椭圆局部坐标系
        Vector2[] vertices = GetRectangleVertices(rect);

        foreach (var vertex in vertices)
        {
            Vector2 localPoint = TransformToEllipseLocal(vertex, ellipse.Center,
                                                       ellipse.Rotation, ellipse.RadiusX, ellipse.RadiusY);

            if (localPoint.LengthSquared() <= 1.0f)
                return true;
        }

        // 检查椭圆中心是否在矩形内
        return IsPointInRectangle(ellipse.Center, rect);
    }

    #endregion

    #region 实用扩展方法

    /// <summary>
    /// 创建扇形（使用起始方向和扫描角度）
    /// </summary>
    public static Sector CreateSector(Vector2 center, float radius, Vector2 direction, float sweepAngle)
    {
        float startAngle = (float)Math.Atan2(direction.Y, direction.X);
        return new Sector(center, radius, startAngle, sweepAngle);
    }

    /// <summary>
    /// 创建椭圆扇形（使用起始方向和扫描角度）
    /// </summary>
    public static EllipticSector CreateEllipticSector(Vector2 center, float radiusX, float radiusY,
                                                     float rotation, Vector2 direction, float sweepAngle)
    {
        // 需要将方向向量转换到椭圆局部坐标系
        Vector2 localDirection = TransformToEllipseLocal(center + direction, center, rotation, 1, 1);
        float startAngle = (float)Math.Atan2(localDirection.Y, localDirection.X);

        return new EllipticSector(center, radiusX, radiusY, rotation, startAngle, sweepAngle);
    }

    /// <summary>
    /// 计算两个形状的相交深度（用于碰撞响应）
    /// </summary>
    public static Vector2 CalculateCollisionDepth(Circle circle, OrthogonalRectangle rect)
    {
        Vector2 circleCenter = circle.Center;
        Vector2 rectCenter = rect.Center;

        float halfWidth = rect.Width * 0.5f;
        float halfHeight = rect.Height * 0.5f;

        Vector2 difference = circleCenter - rectCenter;
        float clampedX = Math.Clamp(difference.X, -halfWidth, halfWidth);
        float clampedY = Math.Clamp(difference.Y, -halfHeight, halfHeight);

        Vector2 closest = rectCenter + new Vector2(clampedX, clampedY);
        Vector2 distance = circleCenter - closest;

        if (distance.LengthSquared() > circle.Radius * circle.Radius)
            return Vector2.Zero;

        float penetrationDepth = circle.Radius - distance.Length();
        return distance.Length() > 0 ? Vector2.Normalize(distance) * penetrationDepth : new Vector2(0, -penetrationDepth);
    }

    #endregion
}