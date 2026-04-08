using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ElectronicWhiteboard
{
    public class ShapeAdorner : Adorner
    {
        // 旋转把手
        private Ellipse _rotateHandle;
        // 旋转中心
        private Point _rotationCenter;
        // 起始角度向量
        private Vector _startVector;
        // 起始旋转角度
        private double _startRotation;
        // 是否正在旋转
        private bool _isRotating;
        // 旋转事件
        public event System.Action<UIElement, double> RotationChanged;
        // 选中的图形
        private UIElement _adornedElement;

        public ShapeAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _adornedElement = adornedElement;
            _rotateHandle = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = new SolidColorBrush(Color.FromRgb(74, 144, 217)),
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Cursor = Cursors.Hand
            };

            // 添加鼠标事件
            _rotateHandle.MouseLeftButtonDown += OnRotateHandleMouseDown;
            _rotateHandle.MouseMove += OnRotateHandleMouseMove;
            _rotateHandle.MouseLeftButtonUp += OnRotateHandleMouseUp;

            // 设置旋转把手的位置
            UpdateHandlePosition();
        }

        // 旋转把手位置
        private Point _handlePosition;
        
        // 更新旋转把手位置（图形顶部中心上方）
        private void UpdateHandlePosition()
        {
            if (_adornedElement is Shape shape)
            {
                double left = Canvas.GetLeft(_adornedElement);
                double top = Canvas.GetTop(_adornedElement);
                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;

                double centerX = left + shape.Width / 2;
                double handleY = top - 15;

                _handlePosition = new Point(centerX - 6, handleY - 6);
                _rotationCenter = new Point(left + shape.Width / 2, top + shape.Height / 2);
            }
            else if (_adornedElement is Path path)
            {
                double left = Canvas.GetLeft(_adornedElement);
                double top = Canvas.GetTop(_adornedElement);
                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;

                if (path.Data is PathGeometry pathGeom)
                {
                    var bounds = pathGeom.Bounds;
                    double centerX = left + bounds.Width / 2;
                    double handleY = top - 15;

                    _handlePosition = new Point(centerX - 6, handleY - 6);
                    _rotationCenter = new Point(left + bounds.Width / 2, top + bounds.Height / 2);
                }
            }
        }

        // 旋转把手鼠标按下
        private void OnRotateHandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isRotating = true;
            _startVector = e.GetPosition(this) - _rotationCenter;
            _startRotation = GetCurrentRotation();
            this.CaptureMouse();
            e.Handled = true;
        }

        // 旋转把手鼠标移动
        private void OnRotateHandleMouseMove(object sender, MouseEventArgs e)
        {
            if (_isRotating)
            {
                var currentVector = e.GetPosition(this) - _rotationCenter;
                double angleDelta = Vector.AngleBetween(_startVector, currentVector);
                double newRotation = _startRotation + angleDelta;

                // 更新图形旋转
                ApplyRotation(newRotation);

                // 触发旋转事件
                RotationChanged?.Invoke(_adornedElement, newRotation);
            }
        }

        // 旋转把手鼠标释放
        private void OnRotateHandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isRotating = false;
            this.ReleaseMouseCapture();
            e.Handled = true;
        }

        // 获取当前旋转角度
        private double GetCurrentRotation()
        {
            if (_adornedElement.RenderTransform is TransformGroup transformGroup)
            {
                foreach (var transform in transformGroup.Children)
                {
                    if (transform is RotateTransform rotateTransform)
                    {
                        return rotateTransform.Angle;
                    }
                }
            }
            else if (_adornedElement.RenderTransform is RotateTransform rotateTransform)
            {
                return rotateTransform.Angle;
            }
            return 0;
        }

        // 应用旋转
        private void ApplyRotation(double angle)
        {
            // 设置旋转变换
            var rotateTransform = new RotateTransform(angle, _rotationCenter.X, _rotationCenter.Y);

            // 如果已有变换组，更新旋转变换
            if (_adornedElement.RenderTransform is TransformGroup transformGroup)
            {
                // 查找并更新旋转变换
                bool found = false;
                for (int i = 0; i < transformGroup.Children.Count; i++)
                {
                    if (transformGroup.Children[i] is RotateTransform)
                    {
                        transformGroup.Children[i] = rotateTransform;
                        found = true;
                        break;
                    }
                }
                // 如果没有旋转变换，添加一个
                if (!found)
                {
                    transformGroup.Children.Add(rotateTransform);
                }
            }
            else if (_adornedElement.RenderTransform is RotateTransform)
            {
                // 替换现有旋转变换
                _adornedElement.RenderTransform = rotateTransform;
            }
            else
            {
                // 创建新的变换组
                var newTransformGroup = new TransformGroup();
                newTransformGroup.Children.Add(_adornedElement.RenderTransform);
                newTransformGroup.Children.Add(rotateTransform);
                _adornedElement.RenderTransform = newTransformGroup;
            }
        }

        // 测量
        protected override Size MeasureOverride(Size constraint)
        {
            _rotateHandle.Measure(constraint);
            return base.MeasureOverride(constraint);
        }

        // 排列
        protected override Size ArrangeOverride(Size finalSize)
        {
            _rotateHandle.Arrange(new Rect(_handlePosition, _rotateHandle.DesiredSize));
            return base.ArrangeOverride(finalSize);
        }

        // 渲染
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            // 绘制旋转中心
            drawingContext.DrawEllipse(
                new SolidColorBrush(Color.FromRgb(74, 144, 217)),
                null,
                _rotationCenter,
                4,
                4
            );
            // 绘制从旋转中心到旋转把手的连接线
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(Color.FromRgb(74, 144, 217)), 1),
                _rotationCenter,
                new Point(_handlePosition.X + 6, _handlePosition.Y + 6)
            );
        }

        // 视觉子元素
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
                return _rotateHandle;
            return base.GetVisualChild(index);
        }

        // 视觉子元素数量
        protected override int VisualChildrenCount => 1;
    }
}