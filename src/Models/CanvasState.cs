using System.Collections.Generic;
using System.Linq;

namespace ElectronicWhiteboard.Models
{
    public class CanvasState
    {
        public List<ShapeModel> Shapes { get; set; } = new List<ShapeModel>();
        public ShapeModel? SelectedShape { get; set; }

        public CanvasState Clone()
        {
            return new CanvasState
            {
                Shapes = this.Shapes.Select(s => s.Clone()).ToList(),
                SelectedShape = this.SelectedShape?.Clone()
            };
        }
    }
}