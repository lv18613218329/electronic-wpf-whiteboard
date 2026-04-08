using ElectronicWhiteboard.Models;

namespace ElectronicWhiteboard.Services
{
    public interface ICanvasCommandService
    {
        void AddShape(ShapeModel shape);
        void RemoveShape(ShapeModel shape);
        void MoveShape(ShapeModel shape, double deltaX, double deltaY);
        void RotateShape(ShapeModel shape, double angle);
        void SelectShape(ShapeModel? shape);
        bool CanUndo();
        bool CanRedo();
        void Undo();
        void Redo();
        void ClearHistory();
    }
}