using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ElectronicWhiteboard.Models
{
    public enum ShapeType
    {
        Rectangle,
        Ellipse
    }

    /// <summary>
    /// 连接点：图形的连接端口，相对坐标范围 0~1
    /// </summary>
    public class Connector
    {
        /// <summary>相对X坐标 (0~1)，相对于图形宽度</summary>
        public double RelativeX { get; set; }
        
        /// <summary>相对Y坐标 (0~1)，相对于图形高度</summary>
        public double RelativeY { get; set; }
        
        /// <summary>连接点名称（如 Top, Bottom, Left, Right, Center）</summary>
        public string Name { get; set; } = "";
        
        /// <summary>是否启用连接</summary>
        public bool IsEnabled { get; set; } = true;

        public Connector() { }

        public Connector(string name, double relativeX, double relativeY)
        {
            Name = name;
            RelativeX = Math.Clamp(relativeX, 0, 1);
            RelativeY = Math.Clamp(relativeY, 0, 1);
        }

        /// <summary>
        /// 计算绝对坐标
        /// </summary>
        public Point GetAbsolutePosition(double shapeX, double shapeY, double width, double height)
        {
            return new Point(
                shapeX + width * RelativeX,
                shapeY + height * RelativeY
            );
        }
    }

    /// <summary>
    /// 连接线：连接两个图形的连接点
    /// </summary>
    public class ConnectionModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>源图形ID</summary>
        public Guid SourceShapeId { get; set; }
        
        /// <summary>目标图形ID</summary>
        public Guid TargetShapeId { get; set; }
        
        /// <summary>源连接点索引</summary>
        public int SourceConnectorIndex { get; set; }
        
        /// <summary>目标连接点索引</summary>
        public int TargetConnectorIndex { get; set; }
        
        /// <summary>连接线颜色</summary>
        public Color StrokeColor { get; set; } = Colors.Gray;
        
        /// <summary>连接线粗细</summary>
        public double StrokeThickness { get; set; } = 2;
    }

    public class ShapeModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ShapeType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 100;
        public double Height { get; set; } = 80;
        public double RotationAngle { get; set; } = 0;
        public Color FillColor { get; set; } = Colors.LightBlue;
        public Color StrokeColor { get; set; } = Colors.DarkBlue;
        public double StrokeThickness { get; set; } = 2;
        
        /// <summary>连接点列表</summary>
        public List<Connector> Connectors { get; set; } = new()
        {
            new Connector("Left", 0, 0.5),      // 左侧中心
            new Connector("Right", 1, 0.5),     // 右侧中心
            new Connector("Top", 0.5, 0),       // 顶部中心
            new Connector("Bottom", 0.5, 1),    // 底部中心
            new Connector("Center", 0.5, 0.5)   // 正中心
        };

        /// <summary>
        /// 获取指定位置的绝对连接点坐标
        /// </summary>
        public List<Point> GetConnectorPositions()
        {
            var positions = new List<Point>();
            foreach (var connector in Connectors)
            {
                if (connector.IsEnabled)
                {
                    positions.Add(connector.GetAbsolutePosition(X, Y, Width, Height));
                }
            }
            return positions;
        }

        public ShapeModel Clone()
        {
            return new ShapeModel
            {
                Id = this.Id,
                Type = this.Type,
                X = this.X,
                Y = this.Y,
                Width = this.Width,
                Height = this.Height,
                RotationAngle = this.RotationAngle,
                FillColor = this.FillColor,
                StrokeColor = this.StrokeColor,
                StrokeThickness = this.StrokeThickness,
                Connectors = new List<Connector>(this.Connectors)
            };
        }
    }
}