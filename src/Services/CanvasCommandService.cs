using System.Collections.Generic;
using ElectronicWhiteboard.Models;

namespace ElectronicWhiteboard.Services
{
    public interface ICanvasCommand
    {
        void Execute();
        void Undo();
    }

    public class CanvasCommandService : ICanvasCommandService
    {
        private readonly CanvasState _state;
        private readonly Stack<ICanvasCommand> _undoStack = new();
        private readonly Stack<ICanvasCommand> _redoStack = new();
        private readonly System.Action _onStateChanged;

        public CanvasCommandService(CanvasState state, System.Action onStateChanged)
        {
            _state = state;
            _onStateChanged = onStateChanged;
        }

        public void AddShape(ShapeModel shape)
        {
            ExecuteCommand(new AddShapeCommand(_state, shape));
        }

        public void RemoveShape(ShapeModel shape)
        {
            ExecuteCommand(new RemoveShapeCommand(_state, shape));
        }

        public void MoveShape(ShapeModel shape, double deltaX, double deltaY)
        {
            ExecuteCommand(new MoveShapeCommand(shape, deltaX, deltaY));
        }

        public void RotateShape(ShapeModel shape, double angle)
        {
            ExecuteCommand(new RotateShapeCommand(shape, angle));
        }

        public void SelectShape(ShapeModel? shape)
        {
            _state.SelectedShape = shape;
            _onStateChanged?.Invoke();
        }

        public bool CanUndo() => _undoStack.Count > 0;
        public bool CanRedo() => _redoStack.Count > 0;

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var cmd = _undoStack.Pop();
                cmd.Undo();
                _redoStack.Push(cmd);
                _onStateChanged?.Invoke();
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var cmd = _redoStack.Pop();
                cmd.Execute();
                _undoStack.Push(cmd);
                _onStateChanged?.Invoke();
            }
        }

        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        private void ExecuteCommand(ICanvasCommand cmd)
        {
            cmd.Execute();
            _undoStack.Push(cmd);
            _redoStack.Clear();
            _onStateChanged?.Invoke();
        }

        private class AddShapeCommand : ICanvasCommand
        {
            private readonly CanvasState _state;
            private readonly ShapeModel _shape;
            public AddShapeCommand(CanvasState state, ShapeModel shape)
            {
                _state = state;
                _shape = shape;
            }
            public void Execute() => _state.Shapes.Add(_shape);
            public void Undo() => _state.Shapes.Remove(_shape);
        }

        private class RemoveShapeCommand : ICanvasCommand
        {
            private readonly CanvasState _state;
            private readonly ShapeModel _shape;
            private int _index;
            public RemoveShapeCommand(CanvasState state, ShapeModel shape)
            {
                _state = state;
                _shape = shape;
            }
            public void Execute()
            {
                _index = _state.Shapes.IndexOf(_shape);
                _state.Shapes.Remove(_shape);
                if (_state.SelectedShape == _shape) _state.SelectedShape = null;
            }
            public void Undo()
            {
                if (_index >= 0 && _index <= _state.Shapes.Count)
                    _state.Shapes.Insert(_index, _shape);
            }
        }

        private class MoveShapeCommand : ICanvasCommand
        {
            private readonly ShapeModel _shape;
            private readonly double _deltaX, _deltaY;
            public MoveShapeCommand(ShapeModel shape, double deltaX, double deltaY)
            {
                _shape = shape;
                _deltaX = deltaX;
                _deltaY = deltaY;
            }
            public void Execute()
            {
                _shape.X += _deltaX;
                _shape.Y += _deltaY;
            }
            public void Undo()
            {
                _shape.X -= _deltaX;
                _shape.Y -= _deltaY;
            }
        }

        private class RotateShapeCommand : ICanvasCommand
        {
            private readonly ShapeModel _shape;
            private readonly double _angle;
            public RotateShapeCommand(ShapeModel shape, double angle)
            {
                _shape = shape;
                _angle = angle;
            }
            public void Execute() => _shape.RotationAngle += _angle;
            public void Undo() => _shape.RotationAngle -= _angle;
        }
    }
}