using System;
using System.Windows;
using System.Windows.Media;

namespace ElectronicWhiteboard.Models
{
    public enum ShapeType
    {
        Rectangle,
        Ellipse
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
                StrokeThickness = this.StrokeThickness
            };
        }
    }
}