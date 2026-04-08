using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ElectronicWhiteboard
{
    public partial class MainWindow : Window
    {
        #region 字段
        
        // 工具类型
        private enum ToolType { Pen, Highlighter, Eraser, Select, Text, SmartPen, DrawTools, SubjectTools, PageManage, Resource, More }
        private ToolType _currentTool = ToolType.Pen;
        private string _currentShapeTool = "";
        private string _currentSubject = "";
        private Color _currentColor = Colors.Black;
        private double _currentPenSize = 3;
        
        // 页面管理
        private int _currentPage = 1;
        private int _totalPages = 1;
        
        // 模式
        private bool _isTeachMode = false;
        
        // 学科工具映射
        private readonly Dictionary<string, List<(string Name, string Icon)>> _subjectToolsMap = new()
        {
            ["Math"] = new() 
            { 
                ("∠", "∠"), ("°", "°"), ("√", "√"), ("π", "π"), ("∞", "∞"), 
                ("±", "±"), ("×", "×"), ("÷", "÷"), ("≠", "≠"), ("≤", "≤"), (">=", "≥"), ("≈", "≈"),
                ("直线", "—"), ("射线", "→"), ("线段", "―"),
                ("三角形", "△"), ("矩形", "▭"), ("圆", "○"), ("扇形", "◠"),
                ("坐标系", "📐"), ("函数", "ƒ")
            },
            ["Physics"] = new() 
            { 
                ("重力", "→"), ("弹力", "↗"), ("摩擦力", "↔"),
                ("杠杆", "⚖"), ("滑轮", "⊙"), ("斜面", "◢"),
                ("光线", "──"), ("反射", "⟂"), ("凸透镜", ")( "), ("凹透镜", "()"),
                ("F", "F"), ("v", "v"), ("a", "a"), ("m", "m"), ("g", "g")
            },
            ["Chemistry"] = new() 
            { 
                ("H", "H"), ("C", "C"), ("O", "O"), ("N", "N"), ("S", "S"), ("P", "P"),
                ("Fe", "Fe"), ("Cu", "Cu"), ("Zn", "Zn"),
                ("单键", "—"), ("双键", "═"), ("三键", "≡"),
                ("苯环", "⬡"), ("烧杯", "🧪"), ("试管", "🧫"),
                ("→", "→"), ("⇌", "⇌")
            }
        };
        
        // 工具按钮引用
        private readonly Dictionary<ToolType, Button> _toolButtons = new();
        private readonly Dictionary<string, Button> _subjectButtons = new();
        private readonly Dictionary<string, Button> _shapeButtons = new();
        
        // 直线绘制相关字段
        private bool _isDrawingLine = false;
        private Point _lineStartPoint;
        private Line? _previewLine;
        private readonly List<UIElement> _shapes = new(); // 用于撤销（包含直线、射线、线段、矩形等）
        private readonly List<UIElement> _arrowHeads = new(); // 箭头头部（用于射线）
        
        // 矩形绘制相关字段
        private bool _isDrawingRectangle = false;
        private Point _rectStartPoint;
        private Rectangle? _previewRectangle;
        private readonly List<UIElement> _rectangles = new(); // 矩形列表（用于撤销）
        
        // 椭圆绘制相关字段
        private bool _isDrawingEllipse = false;
        private Point _ellipseStartPoint;
        private Ellipse? _previewEllipse;
        private readonly List<UIElement> _ellipses = new(); // 椭圆列表（用于撤销）
        
        // 三角形绘制相关字段
        private bool _isDrawingTriangle = false;
        private string _currentTriangleType = ""; // "TriangleNormal", "TriangleIsosceles", "TriangleRight"
        private Point _triangleStartPoint;
        private Point _triangleSecondPoint; // 用于等腰和直角三角形的底边端点
        private readonly List<Point> _trianglePoints = new(); // 存储一般三角形的三个顶点
        private Path? _previewTriangle;
        private readonly List<UIElement> _triangles = new(); // 三角形列表（用于撤销）
        
        // 多边形绘制相关字段
        private bool _isDrawingPolygon = false;
        private int _polygonSides = 3; // 默认3边形（三角形）
        private Point _polygonStartPoint;
        private readonly List<Point> _polygonVertices = new(); // 多边形顶点列表
        private Path? _previewPolygon;
        private Ellipse? _lastVertexMarker; // 最后一个顶点标记
        private readonly List<UIElement> _polygons = new(); // 多边形列表（用于撤销）
        
        // 图形移动相关字段
        private bool _isMovingShape = false;
        private UIElement? _selectedShape;
        private Point _shapeDragStartPoint;
        private Point _shapeOriginalPosition;
        
        // 框选相关字段
        private bool _isSelectingByBox = false; // 是否正在进行框选
        private Point _selectionBoxStartPoint; // 框选起点
        private Rectangle? _selectionBox; // 框选矩形（虚线）
        private readonly List<UIElement> _selectedShapes = new(); // 选中的多个图形
        private bool _isDraggingSelection = false; // 是否在拖动已选中的图形组
        
        #endregion
        
        #region 构造函数
        
        public MainWindow()
        {
            InitializeComponent();
            
            // 初始化工具按钮引用
            _toolButtons[ToolType.Pen] = btnPen;
            _toolButtons[ToolType.Highlighter] = btnHighlighter;
            _toolButtons[ToolType.Eraser] = btnEraser;
            _toolButtons[ToolType.Select] = btnSelect;
            _toolButtons[ToolType.Text] = btnText;
            _toolButtons[ToolType.SmartPen] = btnSmartPen;
            _toolButtons[ToolType.DrawTools] = btnDrawTools;
            _toolButtons[ToolType.SubjectTools] = btnSubjectTools;
            _toolButtons[ToolType.PageManage] = btnPageManage;
            _toolButtons[ToolType.Resource] = btnResource;
            _toolButtons[ToolType.More] = btnMore;
            
            // 学科按钮
            _subjectButtons["Math"] = btnMath;
            _subjectButtons["Physics"] = btnPhysics;
            _subjectButtons["Chemistry"] = btnChemistry;
            
            // 图形按钮
            _shapeButtons["Line"] = btnLine;
            _shapeButtons["Ray"] = btnRay;
            _shapeButtons["LineSegment"] = btnLineSegment;
            _shapeButtons["Rectangle"] = btnRect;
            _shapeButtons["Square"] = btnSquare;
            _shapeButtons["RoundedRect"] = btnRoundedRect;
            _shapeButtons["Ellipse"] = btnEllipse;
            _shapeButtons["EllipseNormal"] = btnEllipseNormal;
            _shapeButtons["Circle"] = btnCircle;
            _shapeButtons["Triangle"] = btnTriangle;
            _shapeButtons["Arrow"] = btnArrow;
            _shapeButtons["Polygon"] = btnPolygon;
            
            // 初始化
            InitializeInkCanvas();
            InitializeEventHandlers();
            
            // 设置默认工具
            SetTool(ToolType.Pen);
            
            // 更新页面信息
            UpdatePageInfo();
        }
        
        #endregion
        
        #region 初始化
        
        private void InitializeInkCanvas()
        {
            // 配置 InkCanvas
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            
            // 设置默认画笔属性
            inkCanvas.DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = _currentColor,
                Width = _currentPenSize,
                Height = _currentPenSize,
                FitToCurve = true,
                StylusTip = StylusTip.Ellipse
            };
        }
        
        private void InitializeEventHandlers()
        {
            // 鼠标移动事件
            inkCanvas.MouseMove += OnInkCanvasMouseMove;
            shapeCanvas.MouseMove += OnShapeCanvasMouseMove;
            
            // 点击画布收起面板
            inkCanvas.MouseDown += OnCanvasClick;
            shapeCanvas.MouseDown += OnCanvasClick;
            
            // 直线绘制鼠标事件
            shapeCanvas.MouseLeftButtonDown += OnShapeCanvasMouseDown;
            shapeCanvas.MouseLeftButtonUp += OnShapeCanvasMouseUp;
            
            // 键盘快捷键
            KeyDown += OnKeyDown;
        }
        
        #endregion
        
        #region 工具选择
        
        private void OnToolClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            
            // 根据按钮确定工具类型
            var tool = btn.Name switch
            {
                "btnPen" => ToolType.Pen,
                "btnHighlighter" => ToolType.Highlighter,
                "btnEraser" => ToolType.Eraser,
                "btnSelect" => ToolType.Select,
                "btnText" => ToolType.Text,
                "btnSmartPen" => ToolType.SmartPen,
                "btnDrawTools" => ToolType.DrawTools,
                "btnSubjectTools" => ToolType.SubjectTools,
                "btnPageManage" => ToolType.PageManage,
                "btnResource" => ToolType.Resource,
                "btnMore" => ToolType.More,
                _ => ToolType.Pen
            };
            
            SetTool(tool);
        }
        
        private void SetTool(ToolType tool)
        {
            _currentTool = tool;
            
            // 更新按钮选中状态
            foreach (var kvp in _toolButtons)
            {
                if (kvp.Value == null) continue;
                
                if (kvp.Key == tool)
                {
                    kvp.Value.Style = (Style)FindResource("PrimaryToolButtonSelectedStyle");
                }
                else
                {
                    kvp.Value.Style = (Style)FindResource("PrimaryToolButtonStyle");
                }
            }
            
            // 隐藏所有面板
            HideAllPanels();
            
            // 显示对应面板
            switch (tool)
            {
                case ToolType.Pen:
                case ToolType.Highlighter:
                    penPanel.Visibility = Visibility.Visible;
                    SetPenMode(tool == ToolType.Highlighter);
                    break;
                    
                case ToolType.Eraser:
                    eraserPanel.Visibility = Visibility.Visible;
                    inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                    break;
                    
                case ToolType.Select:
                    toolHintPanel.Visibility = Visibility.Visible;
                    inkCanvas.EditingMode = InkCanvasEditingMode.Select;
                    // 选择工具下允许移动 shapeCanvas 上的图形
                    shapeCanvas.Cursor = Cursors.Arrow;
                    break;
                    
                case ToolType.Text:
                    toolHintPanel.Visibility = Visibility.Visible;
                    inkCanvas.EditingMode = InkCanvasEditingMode.None;
                    break;
                    
                case ToolType.SmartPen:
                    toolHintPanel.Visibility = Visibility.Visible;
                    MessageBox.Show("智能笔功能：自动识别手绘图形", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                    
                case ToolType.DrawTools:
                    drawToolsPanel.Visibility = Visibility.Visible;
                    inkCanvas.EditingMode = InkCanvasEditingMode.None;
                    break;
                    
                case ToolType.SubjectTools:
                    subjectToolsPanel.Visibility = Visibility.Visible;
                    // 默认显示数学工具
                    if (string.IsNullOrEmpty(_currentSubject))
                    {
                        _currentSubject = "Math";
                        LoadSubjectTools("Math");
                    }
                    break;
                    
                case ToolType.PageManage:
                    pagePanel.Visibility = Visibility.Visible;
                    break;
                    
                case ToolType.Resource:
                    resourcePanel.Visibility = Visibility.Visible;
                    break;
                    
                case ToolType.More:
                    morePanel.Visibility = Visibility.Visible;
                    break;
            }
        }
        
        private void HideAllPanels()
        {
            penPanel.Visibility = Visibility.Collapsed;
            eraserPanel.Visibility = Visibility.Collapsed;
            drawToolsPanel.Visibility = Visibility.Collapsed;
            subjectToolsPanel.Visibility = Visibility.Collapsed;
            pagePanel.Visibility = Visibility.Collapsed;
            resourcePanel.Visibility = Visibility.Collapsed;
            morePanel.Visibility = Visibility.Collapsed;
            toolHintPanel.Visibility = Visibility.Collapsed;
        }
        
        private void SetPenMode(bool isHighlighter)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            
            var attrs = new DrawingAttributes
            {
                Color = isHighlighter ? Color.FromArgb(128, _currentColor.R, _currentColor.G, _currentColor.B) : _currentColor,
                Width = isHighlighter ? _currentPenSize * 3 : _currentPenSize,
                Height = isHighlighter ? _currentPenSize * 3 : _currentPenSize,
                FitToCurve = true,
                StylusTip = StylusTip.Ellipse,
                IsHighlighter = isHighlighter
            };
            
            inkCanvas.DefaultDrawingAttributes = attrs;
        }
        
        #endregion
        
        #region 画笔设置
        
        private void OnColorSelect(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string colorStr) return;
            
            try
            {
                _currentColor = (Color)ColorConverter.ConvertFromString(colorStr);
                
                // 更新画笔颜色
                var attrs = inkCanvas.DefaultDrawingAttributes;
                attrs.Color = _currentColor;
            }
            catch { }
        }
        
        private void OnPenSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtPenSize == null || penSizeSlider == null) return;
            
            _currentPenSize = penSizeSlider.Value;
            txtPenSize.Text = $"{_currentPenSize:F0}px";
            
            // 更新画笔大小
            var attrs = inkCanvas.DefaultDrawingAttributes;
            attrs.Width = _currentPenSize;
            attrs.Height = _currentPenSize;
        }
        
        #endregion
        
        #region 橡皮擦
        
        private void OnEraserModeChange(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string mode) return;
            
            // 更新按钮样式
            btnEraserPoint.Style = mode == "Point" ? (Style)FindResource("SubjectSelectedStyle") : (Style)FindResource("SecondaryButtonStyle");
            btnEraserStroke.Style = mode == "Stroke" ? (Style)FindResource("SubjectSelectedStyle") : (Style)FindResource("SecondaryButtonStyle");
            
            // 设置橡皮擦模式
            inkCanvas.EditingMode = mode == "Point" ? 
                InkCanvasEditingMode.EraseByPoint : 
                InkCanvasEditingMode.EraseByStroke;
        }
        
        private void OnClearCanvas(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要清空画布吗？", "确认", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                inkCanvas.Strokes.Clear();
                shapeCanvas.Children.Clear();
            }
        }
        
        #endregion
        
        #region 绘图工具
        
        private void OnShapeToolClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string shapeType) return;
            
            // 矩形展开子菜单（仅主按钮触发）
            if (shapeType == "Rectangle" && rectSubMenu.Visibility == Visibility.Collapsed)
            {
                // 显示矩形子菜单
                rectSubMenu.Visibility = Visibility.Visible;
                // 隐藏椭圆子菜单
                ellipseSubMenu.Visibility = Visibility.Collapsed;
                return;
            }
            
            // 椭圆展开子菜单（仅主按钮触发）
            if (shapeType == "Ellipse" && ellipseSubMenu.Visibility == Visibility.Collapsed)
            {
                // 显示椭圆子菜单
                ellipseSubMenu.Visibility = Visibility.Visible;
                // 隐藏矩形子菜单
                rectSubMenu.Visibility = Visibility.Collapsed;
                // 隐藏三角形子菜单
                triangleSubMenu.Visibility = Visibility.Collapsed;
                return;
            }
            
            // 三角形展开子菜单（仅主按钮触发）
            if (shapeType == "Triangle" && triangleSubMenu.Visibility == Visibility.Collapsed)
            {
                // 显示三角形子菜单
                triangleSubMenu.Visibility = Visibility.Visible;
                // 隐藏矩形子菜单
                rectSubMenu.Visibility = Visibility.Collapsed;
                // 隐藏椭圆子菜单
                ellipseSubMenu.Visibility = Visibility.Collapsed;
                return;
            }
            
            // 收起子菜单（如果原来已展开，点击子菜单项时执行）
            if (shapeType == "Rectangle" || shapeType == "Ellipse" || shapeType == "Triangle")
            {
                rectSubMenu.Visibility = Visibility.Collapsed;
                ellipseSubMenu.Visibility = Visibility.Collapsed;
                triangleSubMenu.Visibility = Visibility.Collapsed;
            }
            
            // 更新选中状态（排除矩形、椭圆和三角形主按钮，因为有子菜单）
            if (shapeType != "Rectangle" && shapeType != "Ellipse" && shapeType != "Triangle")
            {
                foreach (var kvp in _shapeButtons)
                {
                    if (kvp.Key == "Rectangle" || kvp.Key == "Square" || kvp.Key == "RoundedRect")
                        continue; // 跳过矩形相关按钮
                    if (kvp.Key == "Ellipse" || kvp.Key == "EllipseNormal")
                        continue; // 跳过椭圆相关按钮
                    if (kvp.Key == "Triangle" || kvp.Key == "TriangleNormal" || kvp.Key == "TriangleIsosceles" || kvp.Key == "TriangleRight")
                        continue; // 跳过三角形相关按钮
                    
                    kvp.Value.Style = kvp.Key == shapeType ? 
                        (Style)FindResource("SubjectSelectedStyle") : 
                        (Style)FindResource("SecondaryButtonStyle");
                }
                
                // 更新矩形子按钮的选中状态
                if (shapeType == "Square")
                {
                    btnRectNormalSub.Style = (Style)FindResource("SecondaryButtonStyle");
                    btnSquareSub.Style = (Style)FindResource("SubjectSelectedStyle");
                    btnRoundedRectSub.Style = (Style)FindResource("SecondaryButtonStyle");
                }
                else if (shapeType == "RoundedRect")
                {
                    btnRectNormalSub.Style = (Style)FindResource("SecondaryButtonStyle");
                    btnSquareSub.Style = (Style)FindResource("SecondaryButtonStyle");
                    btnRoundedRectSub.Style = (Style)FindResource("SubjectSelectedStyle");
                }
                else if (shapeType == "Rectangle")
                {
                    btnRectNormalSub.Style = (Style)FindResource("SubjectSelectedStyle");
                    btnSquareSub.Style = (Style)FindResource("SecondaryButtonStyle");
                    btnRoundedRectSub.Style = (Style)FindResource("SecondaryButtonStyle");
                }
                
                // 更新椭圆子按钮的选中状态
                if (shapeType == "Circle")
                {
                    btnEllipseNormalSub.Style = (Style)FindResource("SecondaryButtonStyle");
                    btnCircleSub.Style = (Style)FindResource("SubjectSelectedStyle");
                }
                else if (shapeType == "Ellipse")
                {
                    btnEllipseNormalSub.Style = (Style)FindResource("SubjectSelectedStyle");
                    btnCircleSub.Style = (Style)FindResource("SecondaryButtonStyle");
                }
                
                // 更新三角形子按钮的选中状态
                if (shapeType == "TriangleNormal")
                {
                    btnTriangleNormalSub.Style = (Style)FindResource("SubjectSelectedStyle");
                    btnTriangleIsoscelesSub.Style = (Style)FindResource("SecondaryButtonStyle");
                    btnTriangleRightSub.Style = (Style)FindResource("SecondaryButtonStyle");
                }
                else if (shapeType == "TriangleIsosceles")
                {
                    btnTriangleNormalSub.Style = (Style)FindResource("SecondaryButtonStyle");
                    btnTriangleIsoscelesSub.Style = (Style)FindResource("SubjectSelectedStyle");
                    btnTriangleRightSub.Style = (Style)FindResource("SecondaryButtonStyle");
                }
                else if (shapeType == "TriangleRight")
                {
                    btnTriangleNormalSub.Style = (Style)FindResource("SecondaryButtonStyle");
                    btnTriangleIsoscelesSub.Style = (Style)FindResource("SecondaryButtonStyle");
                    btnTriangleRightSub.Style = (Style)FindResource("SubjectSelectedStyle");
                }
            }
            
            // 多边形：先显示边数设置对话框
            if (shapeType == "Polygon")
            {
                ShowPolygonSidesDialog();
            }
            
            _currentShapeTool = shapeType;
            
            // 隐藏矩形子菜单（如果原来是展开的）
            rectSubMenu.Visibility = Visibility.Collapsed;
            
            // 隐藏椭圆子菜单（如果原来是展开的）
            ellipseSubMenu.Visibility = Visibility.Collapsed;
            
            // 设置绘图工具的光标和模式
            if (shapeType == "Line" || shapeType == "Ray" || shapeType == "LineSegment")
            {
                // 直线/射线/线段工具：切换到 None 模式以便自定义绘制
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
                shapeCanvas.Cursor = Cursors.Cross;
            }
            else if (shapeType == "Rectangle" || shapeType == "Square" || shapeType == "RoundedRect")
            {
                // 矩形/正方形/圆角矩形工具
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
                shapeCanvas.Cursor = Cursors.Cross;
            }
            else if (shapeType == "Ellipse" || shapeType == "Circle")
            {
                // 椭圆/圆工具
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
                shapeCanvas.Cursor = Cursors.Cross;
            }
            else if (shapeType == "TriangleNormal" || shapeType == "TriangleIsosceles" || shapeType == "TriangleRight")
            {
                // 三角形工具
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
                shapeCanvas.Cursor = Cursors.Cross;
            }
            else
            {
                // 其他图形工具暂时显示提示
                shapeCanvas.Cursor = Cursors.Arrow;
                MessageBox.Show($"当前绘图工具: {shapeType}\n拖动鼠标绘制图形", "绘图工具", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        #endregion
        
        #region 学科工具
        
        private void OnSubjectSelect(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string subject) return;
            
            _currentSubject = subject;
            
            // 更新按钮样式
            foreach (var kvp in _subjectButtons)
            {
                kvp.Value.Style = kvp.Key == subject ? 
                    (Style)FindResource("SubjectSelectedStyle") : 
                    (Style)FindResource("SecondaryButtonStyle");
            }
            
            // 加载学科工具
            LoadSubjectTools(subject);
        }
        
        private void LoadSubjectTools(string subject)
        {
            subjectToolsContent.Children.Clear();
            
            if (_subjectToolsMap.TryGetValue(subject, out var tools))
            {
                foreach (var (name, icon) in tools)
                {
                    var toolBtn = new Button
                    {
                        Width = 50,
                        Height = 40,
                        Margin = new Thickness(3),
                        Content = icon,
                        ToolTip = name,
                        Style = (Style)FindResource("PanelButtonStyle"),
                        Tag = $"{subject}:{name}"
                    };
                    toolBtn.Click += OnSubjectToolClick;
                    subjectToolsContent.Children.Add(toolBtn);
                }
            }
        }
        
        private void OnSubjectToolClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string toolInfo)
            {
                MessageBox.Show($"选择工具: {toolInfo}", "学科工具", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        #endregion
        
        #region 页面管理
        
        private void OnNewPage(object sender, RoutedEventArgs e)
        {
            _totalPages++;
            _currentPage = _totalPages;
            inkCanvas.Strokes.Clear();
            shapeCanvas.Children.Clear();
            UpdatePageInfo();
        }
        
        private void OnCopyPage(object sender, RoutedEventArgs e)
        {
            _totalPages++;
            _currentPage = _totalPages;
            UpdatePageInfo();
            MessageBox.Show("页面复制功能需要实现页面数据结构", "提示", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void OnDeletePage(object sender, RoutedEventArgs e)
        {
            if (_totalPages <= 1)
            {
                MessageBox.Show("至少需要保留一页", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var result = MessageBox.Show($"确定删除第 {_currentPage} 页吗？", "确认", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _totalPages--;
                if (_currentPage > _totalPages) _currentPage = _totalPages;
                inkCanvas.Strokes.Clear();
                shapeCanvas.Children.Clear();
                UpdatePageInfo();
            }
        }
        
        private void OnPrevPage(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                inkCanvas.Strokes.Clear();
                shapeCanvas.Children.Clear();
                UpdatePageInfo();
            }
        }
        
        private void OnNextPage(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                inkCanvas.Strokes.Clear();
                shapeCanvas.Children.Clear();
                UpdatePageInfo();
            }
        }
        
        private void UpdatePageInfo()
        {
            txtPageInfo.Text = $"当前: {_currentPage} / {_totalPages}";
        }
        
        #endregion
        
        #region 资源
        
        private void OnResourceClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string resourceType)
            {
                MessageBox.Show($"选择资源类型: {resourceType}", "资源", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        #endregion
        
        #region 更多功能
        
        private void OnFullScreen(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }
        
        private void OnExport(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("导出功能: 保存画布内容为图片", "导出", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void OnImport(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("导入功能: 从文件导入内容", "导入", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        #endregion
        
        #region 撤销重做
        
        private void OnUndo(object sender, RoutedEventArgs e)
        {
            // 撤销笔画
            if (inkCanvas.Strokes.Count > 0)
            {
                inkCanvas.Strokes.RemoveAt(inkCanvas.Strokes.Count - 1);
            }
            
            // 撤销图形（直线、射线、线段、矩形、椭圆、三角形等）
            if (_shapes.Count > 0)
            {
                var lastShape = _shapes[_shapes.Count - 1];
                shapeCanvas.Children.Remove(lastShape);
                _shapes.RemoveAt(_shapes.Count - 1);
                
                // 如果是射线，也撤销箭头头部
                if (_arrowHeads.Count > 0)
                {
                    var lastArrowHead = _arrowHeads[_arrowHeads.Count - 1];
                    shapeCanvas.Children.Remove(lastArrowHead);
                    _arrowHeads.RemoveAt(_arrowHeads.Count - 1);
                }
                
                // 如果是三角形，也从三角形列表中移除
                if (lastShape is Path path && path.Tag != null && path.Tag.ToString().Contains("Triangle"))
                {
                    if (_triangles.Count > 0)
                    {
                        _triangles.RemoveAt(_triangles.Count - 1);
                    }
                }
            }
        }
        
        private void OnRedo(object sender, RoutedEventArgs e)
        {
            // 重做功能需要保存历史记录，这里简单提示
            MessageBox.Show("重做功能需要实现历史记录", "提示", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        #endregion
        
        #region 模式切换
        
        private void OnModeChange(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string mode) return;
            
            _isTeachMode = mode == "Teach";
            
            btnPrepMode.Style = !_isTeachMode ? 
                (Style)FindResource("SubjectSelectedStyle") : 
                (Style)FindResource("ModeButtonStyle");
            btnTeachMode.Style = _isTeachMode ? 
                (Style)FindResource("SubjectSelectedStyle") : 
                (Style)FindResource("ModeButtonStyle");
            
            var modeName = _isTeachMode ? "授课" : "备课";
            MessageBox.Show($"已切换到{modeName}模式", "模式切换", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        #endregion
        
        #region 鼠标事件
        
        private void OnCanvasClick(object sender, MouseButtonEventArgs e)
        {
            // 点击画布收起所有面板（可选）
            // HideAllPanels();
        }
        
        private void OnInkCanvasMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(inkCanvas);
            // 可以显示坐标信息
        }
        
        private void OnShapeCanvasMouseMove(object sender, MouseEventArgs e)
        {
            // 直线/射线/线段绘制时的实时预览
            if (_isDrawingLine && _previewLine != null)
            {
                var currentPos = e.GetPosition(shapeCanvas);
                
                if (_currentShapeTool == "Ray")
                {
                    // 射线：延伸预览线到画布边界
                    var (extendedX, extendedY) = ExtendLineToBoundary(_lineStartPoint, currentPos, shapeCanvas.ActualWidth, shapeCanvas.ActualHeight);
                    _previewLine.X2 = extendedX;
                    _previewLine.Y2 = extendedY;
                }
                else
                {
                    // 直线和线段：直接使用当前鼠标位置
                    _previewLine.X2 = currentPos.X;
                    _previewLine.Y2 = currentPos.Y;
                }
            }
            
            // 矩形/正方形/圆角矩形绘制时的实时预览
            if (_isDrawingRectangle && _previewRectangle != null)
            {
                var currentPos = e.GetPosition(shapeCanvas);
                UpdatePreviewRectangle(currentPos);
            }
            
            // 椭圆/圆绘制时的实时预览
            if (_isDrawingEllipse && _previewEllipse != null)
            {
                var currentPos = e.GetPosition(shapeCanvas);
                UpdatePreviewEllipse(currentPos);
            }
            
            // 三角形绘制时的实时预览
            // 修复：增加 _currentTriangleType 不为空的检查，避免空引用导致的异常
            if (_isDrawingTriangle && _previewTriangle != null && !string.IsNullOrEmpty(_currentTriangleType))
            {
                var currentPos = e.GetPosition(shapeCanvas);
                UpdatePreviewTriangle(currentPos);
            }
            
            // 多边形绘制时的实时预览（点击顶点+双击完成）
            if (_isDrawingPolygon && _previewPolygon != null)
            {
                var currentPos = e.GetPosition(shapeCanvas);
                HandlePolygonMouseMove(currentPos);
            }
            
            // 图形移动时的实时更新
            if (_isMovingShape && _selectedShape != null)
            {
                var currentPos = e.GetPosition(shapeCanvas);
                MoveShape(currentPos);
            }
            
            // 框选时的实时更新
            if (_isSelectingByBox && _selectionBox != null)
            {
                var currentPos = e.GetPosition(shapeCanvas);
                UpdateSelectionBox(currentPos);
            }
            
            // 拖动已选中的图形组
            if (_isDraggingSelection)
            {
                var currentPos = e.GetPosition(shapeCanvas);
                MoveSelectedShapes(currentPos);
            }
            
            var pos = e.GetPosition(shapeCanvas);
            // 可以显示坐标信息
        }
        
        // 直线/射线/线段绘制：鼠标按下
        private void OnShapeCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 选择工具：处理图形选择和移动
            if (_currentTool == ToolType.Select)
            {
                var clickedElement = e.OriginalSource as FrameworkElement;
                var clickPos = e.GetPosition(shapeCanvas);
                
                // 如果点击的是背景（canvas本身）
                if (clickedElement == shapeCanvas)
                {
                    // 如果已经有选中的图形，点击空白处取消选中
                    if (_selectedShapes.Count > 0)
                    {
                        DeselectAllShapes();
                        return;
                    }
                    
                    // 开始框选
                    StartSelectionBox(clickPos);
                    return;
                }
                
                // 查找点击的图形元素（排除顶点标记）
                var shape = FindParentShape(clickedElement);
                if (shape != null)
                {
                    var tagStr = (shape as FrameworkElement)?.Tag?.ToString() ?? "";
                    if (tagStr != "polygon-vertex")
                    {
                        // Ctrl+点击：追加/取消选中
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            ToggleShapeSelection(shape);
                        }
                        else
                        {
                            // 检查是否已经选中
                            if (_selectedShapes.Contains(shape))
                            {
                                // 已经开始拖动已选中的图形组
                                _isDraggingSelection = true;
                                _shapeDragStartPoint = clickPos;
                                SaveSelectedShapesOriginalPositions();
                                shapeCanvas.CaptureMouse();
                            }
                            else
                            {
                                // 取消之前的选中，选中新的图形
                                DeselectAllShapes();
                                SelectShape(shape);
                            }
                        }
                        
                        // 开始拖动
                        _isMovingShape = true;
                        _shapeDragStartPoint = clickPos;
                        
                        // 保存图形的原始位置
                        _shapeOriginalPosition = new Point(
                            Canvas.GetLeft(shape),
                            Canvas.GetTop(shape)
                        );
                        
                        // 如果位置为0（可能没有设置过），使用0
                        if (double.IsNaN(_shapeOriginalPosition.X)) _shapeOriginalPosition.X = 0;
                        if (double.IsNaN(_shapeOriginalPosition.Y)) _shapeOriginalPosition.Y = 0;
                        
                        shapeCanvas.CaptureMouse();
                        e.Handled = true;
                        return;
                    }
                }
            }
            
            // 直线/射线/线段工具
            if (_currentShapeTool == "Line" || _currentShapeTool == "Ray" || _currentShapeTool == "LineSegment")
            {
                _lineStartPoint = e.GetPosition(shapeCanvas);
                _isDrawingLine = true;
                
                // 创建预览线
                _previewLine = new Line
                {
                    X1 = _lineStartPoint.X,
                    Y1 = _lineStartPoint.Y,
                    X2 = _lineStartPoint.X,
                    Y2 = _lineStartPoint.Y,
                    Stroke = new SolidColorBrush(_currentColor),
                    StrokeThickness = _currentPenSize,
                    StrokeDashArray = new DoubleCollection { 4, 2 }, // 虚线效果
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                
                // 射线使用实线预览（虚线在延伸时效果不好）
                if (_currentShapeTool == "Ray")
                {
                    _previewLine.StrokeDashArray = null; // 实线
                }
                
                shapeCanvas.Children.Add(_previewLine);
                
                // 捕获鼠标
                shapeCanvas.CaptureMouse();
                return;
            }
            
            // 矩形/正方形/圆角矩形工具
            if (_currentShapeTool == "Rectangle" || _currentShapeTool == "Square" || _currentShapeTool == "RoundedRect")
            {
                _rectStartPoint = e.GetPosition(shapeCanvas);
                _isDrawingRectangle = true;
                
                // 创建预览矩形
                _previewRectangle = new Rectangle
                {
                    Stroke = new SolidColorBrush(_currentColor),
                    StrokeThickness = _currentPenSize,
                    Fill = Brushes.Transparent,
                    StrokeDashArray = new DoubleCollection { 4, 2 }, // 虚线效果
                    StrokeDashCap = PenLineCap.Round
                };
                
                // 圆角矩形设置圆角
                if (_currentShapeTool == "RoundedRect")
                {
                    _previewRectangle.RadiusX = 10;
                    _previewRectangle.RadiusY = 10;
                }
                
                // 设置初始位置和大小
                Canvas.SetLeft(_previewRectangle, _rectStartPoint.X);
                Canvas.SetTop(_previewRectangle, _rectStartPoint.Y);
                _previewRectangle.Width = 0;
                _previewRectangle.Height = 0;
                
                shapeCanvas.Children.Add(_previewRectangle);
                
                // 捕获鼠标
                shapeCanvas.CaptureMouse();
                return;
            }
            
            // 椭圆/圆工具
            if (_currentShapeTool == "Ellipse" || _currentShapeTool == "Circle")
            {
                _ellipseStartPoint = e.GetPosition(shapeCanvas);
                _isDrawingEllipse = true;
                
                // 创建预览椭圆
                _previewEllipse = new Ellipse
                {
                    Stroke = new SolidColorBrush(_currentColor),
                    StrokeThickness = _currentPenSize,
                    Fill = Brushes.Transparent,
                    StrokeDashArray = new DoubleCollection { 4, 2 }, // 虚线效果
                    StrokeDashCap = PenLineCap.Round
                };
                
                // 设置初始位置和大小
                Canvas.SetLeft(_previewEllipse, _ellipseStartPoint.X);
                Canvas.SetTop(_previewEllipse, _ellipseStartPoint.Y);
                _previewEllipse.Width = 0;
                _previewEllipse.Height = 0;
                
                shapeCanvas.Children.Add(_previewEllipse);
                
                // 捕获鼠标
                shapeCanvas.CaptureMouse();
                return;
            }
            
            // 三角形工具 - 简化版：拖拽绘制
            if (_currentShapeTool == "TriangleNormal" || _currentShapeTool == "TriangleIsosceles" || _currentShapeTool == "TriangleRight")
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[三角形] MouseDown: {_currentShapeTool}, 颜色={_currentColor}");
                    
                    _currentTriangleType = _currentShapeTool;
                    _triangleStartPoint = e.GetPosition(shapeCanvas);
                    _isDrawingTriangle = true;
                    
                    // 创建预览三角形
                    _previewTriangle = new Path
                    {
                        Stroke = new SolidColorBrush(_currentColor),
                        StrokeThickness = _currentPenSize,
                        Fill = Brushes.Transparent,
                        StrokeDashArray = new DoubleCollection { 4, 2 }
                    };
                    
                    UpdateTrianglePreview(_triangleStartPoint);
                    shapeCanvas.Children.Add(_previewTriangle);
                    shapeCanvas.CaptureMouse();
                    
                    System.Diagnostics.Debug.WriteLine("[三角形] MouseDown 完成");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[三角形] MouseDown 异常: {ex.Message}");
                    MessageBox.Show($"三角形绘制错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }
            
            // 多边形工具 - 点击顶点+双击完成
            if (_currentShapeTool == "Polygon")
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[多边形] MouseDown: {_currentShapeTool}, 颜色={_currentColor}");
                    
                    var clickPoint = e.GetPosition(shapeCanvas);
                    
                    // 检测双击：完成多边形
                    if (e.ClickCount == 2 && _isDrawingPolygon && _polygonVertices.Count >= 3)
                    {
                        System.Diagnostics.Debug.WriteLine("[多边形] 双击完成");
                        FinishPolygon();
                        return;
                    }
                    
                    if (!_isDrawingPolygon)
                    {
                        // 开始新的多边形绘制
                        _isDrawingPolygon = true;
                        _polygonVertices.Clear();
                        _polygonVertices.Add(clickPoint);
                        
                        // 创建预览多边形
                        _previewPolygon = new Path
                        {
                            Stroke = new SolidColorBrush(_currentColor),
                            StrokeThickness = _currentPenSize,
                            Fill = Brushes.Transparent,
                            StrokeDashArray = new DoubleCollection { 4, 2 }
                        };
                        
                        // 添加第一个顶点标记
                        AddPolygonVertexMarker(clickPoint, isFirst: true);
                        
                        UpdatePolygonPreview();
                        shapeCanvas.Children.Add(_previewPolygon);
                        
                        System.Diagnostics.Debug.WriteLine("[多边形] 开始绘制，顶点数: 1");
                    }
                    else
                    {
                        // 继续添加顶点
                        // 检查是否接近第一个顶点（自动闭合）
                        if (_polygonVertices.Count >= 3)
                        {
                            var firstVertex = _polygonVertices[0];
                            var distance = Math.Sqrt(Math.Pow(clickPoint.X - firstVertex.X, 2) + Math.Pow(clickPoint.Y - firstVertex.Y, 2));
                            
                            if (distance < 15) // 接近第一个顶点
                            {
                                // 自动闭合多边形
                                FinishPolygon();
                                return;
                            }
                        }
                        
                        // 添加新顶点
                        _polygonVertices.Add(clickPoint);
                        AddPolygonVertexMarker(clickPoint, isFirst: false);
                        UpdatePolygonPreview();
                        
                        System.Diagnostics.Debug.WriteLine($"[多边形] 添加顶点，顶点数: {_polygonVertices.Count}");
                    }
                    
                    shapeCanvas.CaptureMouse();
                    System.Diagnostics.Debug.WriteLine("[多边形] MouseDown 完成");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[多边形] MouseDown 异常: {ex.Message}");
                    MessageBox.Show($"多边形绘制错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }
        }
        
        // 更新三角形预览（简化版）
        private void UpdateTrianglePreview(Point endPoint)
        {
            if (_previewTriangle == null) return;
            
            double x = Math.Min(_triangleStartPoint.X, endPoint.X);
            double y = Math.Min(_triangleStartPoint.Y, endPoint.Y);
            double width = Math.Abs(endPoint.X - _triangleStartPoint.X);
            double height = Math.Abs(endPoint.Y - _triangleStartPoint.Y);
            
            if (_currentTriangleType == "TriangleNormal")
            {
                // 一般三角形：三点确定
                var p1 = new Point(_triangleStartPoint.X, _triangleStartPoint.Y);
                var p2 = new Point(endPoint.X, _triangleStartPoint.Y);
                var p3 = new Point(_triangleStartPoint.X + width / 2, y);
                
                var pathGeometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = p1, IsClosed = true };
                figure.Segments.Add(new LineSegment(p2, true));
                figure.Segments.Add(new LineSegment(p3, true));
                pathGeometry.Figures.Add(figure);
                _previewTriangle.Data = pathGeometry;
            }
            else if (_currentTriangleType == "TriangleIsosceles")
            {
                // 等腰三角形
                var p1 = new Point(_triangleStartPoint.X, _triangleStartPoint.Y);
                var p2 = new Point(endPoint.X, _triangleStartPoint.Y);
                var p3 = new Point(_triangleStartPoint.X + width / 2, y);
                
                var pathGeometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = p1, IsClosed = true };
                figure.Segments.Add(new LineSegment(p2, true));
                figure.Segments.Add(new LineSegment(p3, true));
                pathGeometry.Figures.Add(figure);
                _previewTriangle.Data = pathGeometry;
            }
            else if (_currentTriangleType == "TriangleRight")
            {
                // 直角三角形
                var p1 = new Point(_triangleStartPoint.X, _triangleStartPoint.Y);
                var p2 = new Point(endPoint.X, _triangleStartPoint.Y);
                var p3 = new Point(_triangleStartPoint.X, endPoint.Y);
                
                var pathGeometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = p1, IsClosed = true };
                figure.Segments.Add(new LineSegment(p2, true));
                figure.Segments.Add(new LineSegment(p3, true));
                pathGeometry.Figures.Add(figure);
                _previewTriangle.Data = pathGeometry;
            }
        }
        
        #region 多边形绘制方法（点击顶点+双击完成）
        
        // 添加多边形顶点标记
        private void AddPolygonVertexMarker(Point position, bool isFirst)
        {
            var marker = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = isFirst ? new SolidColorBrush(Color.FromRgb(76, 175, 80)) : Brushes.White,
                Stroke = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                StrokeThickness = 2,
                Tag = "polygon-vertex"
            };
            
            // 最后一个顶点需要保存引用以便移除
            if (_lastVertexMarker != null)
            {
                // 将之前的最后一个顶点改为普通顶点
                _lastVertexMarker.Fill = Brushes.White;
            }
            _lastVertexMarker = marker;
            
            Canvas.SetLeft(marker, position.X - 6);
            Canvas.SetTop(marker, position.Y - 6);
            shapeCanvas.Children.Add(marker);
        }
        
        // 清除所有多边形顶点标记
        private void ClearPolygonVertexMarkers()
        {
            var toRemove = new List<UIElement>();
            foreach (UIElement child in shapeCanvas.Children)
            {
                if (child is Ellipse ellipse && ellipse.Tag?.ToString() == "polygon-vertex")
                {
                    toRemove.Add(child);
                }
            }
            foreach (var item in toRemove)
            {
                shapeCanvas.Children.Remove(item);
            }
            _lastVertexMarker = null;
        }
        
        // 更新多边形预览
        private void UpdatePolygonPreview()
        {
            if (_previewPolygon == null || _polygonVertices.Count < 1) return;
            
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = _polygonVertices[0], IsClosed = false };
            
            // 添加所有顶点
            for (int i = 1; i < _polygonVertices.Count; i++)
            {
                figure.Segments.Add(new LineSegment(_polygonVertices[i], true));
            }
            
            pathGeometry.Figures.Add(figure);
            _previewPolygon.Data = pathGeometry;
        }
        
        // 完成多边形绘制
        private void FinishPolygon()
        {
            if (_polygonVertices.Count < 3)
            {
                // 顶点不足3个，取消绘制
                CancelPolygonDrawing();
                return;
            }
            
            // 清除预览和顶点标记
            if (_previewPolygon != null)
            {
                shapeCanvas.Children.Remove(_previewPolygon);
                _previewPolygon = null;
            }
            ClearPolygonVertexMarkers();
            
            // 创建正式多边形
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = _polygonVertices[0], IsClosed = true };
            
            for (int i = 1; i < _polygonVertices.Count; i++)
            {
                figure.Segments.Add(new LineSegment(_polygonVertices[i], true));
            }
            
            pathGeometry.Figures.Add(figure);
            
            var polygon = new Path
            {
                Data = pathGeometry,
                Stroke = new SolidColorBrush(_currentColor),
                StrokeThickness = _currentPenSize,
                Fill = Brushes.Transparent,
                Tag = "Polygon"
            };
            
            shapeCanvas.Children.Add(polygon);
            _polygons.Add(polygon);
            _shapes.Add(polygon); // 添加到 shapes 列表用于撤销
            
            System.Diagnostics.Debug.WriteLine($"[多边形] 创建完成，顶点数: {_polygonVertices.Count}");
            
            // 重置状态
            _isDrawingPolygon = false;
            _polygonVertices.Clear();
        }
        
        // 取消多边形绘制
        private void CancelPolygonDrawing()
        {
            if (_previewPolygon != null)
            {
                shapeCanvas.Children.Remove(_previewPolygon);
                _previewPolygon = null;
            }
            ClearPolygonVertexMarkers();
            
            _isDrawingPolygon = false;
            _polygonVertices.Clear();
            
            shapeCanvas.ReleaseMouseCapture();
        }
        
        // 多边形鼠标移动处理
        private void HandlePolygonMouseMove(Point currentPos)
        {
            if (!_isDrawingPolygon || _previewPolygon == null || _polygonVertices.Count < 1) return;
            
            // 更新预览：显示从最后一个顶点到鼠标位置的连线
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = _polygonVertices[0], IsClosed = false };
            
            // 添加所有顶点
            for (int i = 1; i < _polygonVertices.Count; i++)
            {
                figure.Segments.Add(new LineSegment(_polygonVertices[i], true));
            }
            
            // 添加到当前鼠标位置的连线
            figure.Segments.Add(new LineSegment(currentPos, true));
            
            pathGeometry.Figures.Add(figure);
            _previewPolygon.Data = pathGeometry;
        }
        
        #endregion
        // 显示多边形边数设置对话框
        private void ShowPolygonSidesDialog()
        {
            // 创建输入对话框
            var dialog = new Window
            {
                Title = "设置多边形边数",
                Width = 300,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };
            
            var panel = new StackPanel { Margin = new Thickness(20) };
            
            // 说明文字
            var label = new TextBlock
            {
                Text = "设置多边形边数 (3-10)",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15)
            };
            panel.Children.Add(label);
            
            // 预设按钮面板
            var presetPanel = new WrapPanel { Margin = new Thickness(0, 0, 0, 15) };
            for (int i = 3; i <= 10; i++)
            {
                var btn = new Button
                {
                    Content = i.ToString(),
                    Width = 35,
                    Height = 30,
                    Margin = new Thickness(3),
                    Tag = i
                };
                int sides = i;
                btn.Click += (s, e) =>
                {
                    _polygonSides = sides;
                    dialog.DialogResult = true;
                    dialog.Close();
                };
                presetPanel.Children.Add(btn);
            }
            panel.Children.Add(presetPanel);
            
            // 取消按钮
            var cancelBtn = new Button
            {
                Content = "取消",
                Width = 80,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            cancelBtn.Click += (s, e) =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            };
            panel.Children.Add(cancelBtn);
            
            dialog.Content = panel;
            dialog.ShowDialog();
            
            System.Diagnostics.Debug.WriteLine($"[多边形] 边数设置为: {_polygonSides}");
        }
        
        // 直线/射线/线段绘制：鼠标释放
        private void OnShapeCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            // 图形移动结束
            if (_isMovingShape)
            {
                _isMovingShape = false;
                shapeCanvas.ReleaseMouseCapture();
                System.Diagnostics.Debug.WriteLine("[选择] 移动完成");
                return;
            }
            
            // 框选结束
            if (_isSelectingByBox)
            {
                EndSelectionBox(e.GetPosition(shapeCanvas));
                return;
            }
            
            // 拖动选中图形组结束
            if (_isDraggingSelection)
            {
                _isDraggingSelection = false;
                shapeCanvas.ReleaseMouseCapture();
                System.Diagnostics.Debug.WriteLine("[选择] 批量移动完成");
                return;
            }
            
            // 三角形绘制完成 - 优先处理（必须在最前面，因为三角形也是通过 Path 绘制）
            // 修复：增加 _currentTriangleType 不为空的检查，避免空引用
            if (_isDrawingTriangle && !string.IsNullOrEmpty(_currentTriangleType) && 
                (_currentShapeTool == "TriangleNormal" || _currentShapeTool == "TriangleIsosceles" || _currentShapeTool == "TriangleRight"))
            {
                HandleTriangleMouseUp(e);
                return;
            }
            
            // 椭圆/圆绘制完成 - 优先处理
            if (_isDrawingEllipse && _currentShapeTool != null && 
                (_currentShapeTool == "Ellipse" || _currentShapeTool == "Circle"))
            {
                // 释放鼠标捕获
                shapeCanvas.ReleaseMouseCapture();
                
                var ellipseEndPoint = e.GetPosition(shapeCanvas);
                
                // 计算椭圆区域
                double x = Math.Min(_ellipseStartPoint.X, ellipseEndPoint.X);
                double y = Math.Min(_ellipseStartPoint.Y, ellipseEndPoint.Y);
                double width = Math.Abs(ellipseEndPoint.X - _ellipseStartPoint.X);
                double height = Math.Abs(ellipseEndPoint.Y - _ellipseStartPoint.Y);
                
                // 移除预览椭圆
                if (_previewEllipse != null)
                {
                    shapeCanvas.Children.Remove(_previewEllipse);
                }
                
                // 太小的椭圆不创建
                if (width > 5 || height > 5)
                {
                    // 创建正式椭圆
                    var ellipse = new Ellipse
                    {
                        Stroke = new SolidColorBrush(_currentColor),
                        StrokeThickness = _currentPenSize,
                        Fill = Brushes.Transparent,
                        Tag = _currentShapeTool
                    };
                    
                    // 圆形：锁定宽度=高度
                    if (_currentShapeTool == "Circle")
                    {
                        var size = Math.Max(width, height);
                        ellipse.Width = size;
                        ellipse.Height = size;
                        
                        // 调整位置以保持中心不变
                        if (width > height)
                        {
                            y = _ellipseStartPoint.Y - (size - height) / 2;
                        }
                        else
                        {
                            x = _ellipseStartPoint.X - (size - width) / 2;
                        }
                    }
                    else
                    {
                        ellipse.Width = width;
                        ellipse.Height = height;
                    }
                    
                    Canvas.SetLeft(ellipse, x);
                    Canvas.SetTop(ellipse, y);
                    
                    shapeCanvas.Children.Add(ellipse);
                    _shapes.Add(ellipse); // 添加到 shapes 列表用于撤销
                    _ellipses.Add(ellipse); // 添加到 ellipses 列表
                }
                
                // 清理状态
                _previewEllipse = null;
                _isDrawingEllipse = false;
                return;
            }
            
            // 矩形/正方形/圆角矩形绘制完成 - 优先处理
            if (_isDrawingRectangle && _currentShapeTool != null && 
                (_currentShapeTool == "Rectangle" || _currentShapeTool == "Square" || _currentShapeTool == "RoundedRect"))
            {
                // 释放鼠标捕获
                shapeCanvas.ReleaseMouseCapture();
                
                var rectEndPoint = e.GetPosition(shapeCanvas);
                
                // 计算矩形区域
                double x = Math.Min(_rectStartPoint.X, rectEndPoint.X);
                double y = Math.Min(_rectStartPoint.Y, rectEndPoint.Y);
                double width = Math.Abs(rectEndPoint.X - _rectStartPoint.X);
                double height = Math.Abs(rectEndPoint.Y - _rectStartPoint.Y);
                
                // 移除预览矩形
                if (_previewRectangle != null)
                {
                    shapeCanvas.Children.Remove(_previewRectangle);
                }
                
                // 太小的矩形不创建
                if (width > 5 || height > 5)
                {
                    // 创建正式矩形
                    var rectangle = new Rectangle
                    {
                        Stroke = new SolidColorBrush(_currentColor),
                        StrokeThickness = _currentPenSize,
                        Fill = Brushes.Transparent,
                        Width = width,
                        Height = height,
                        Tag = _currentShapeTool
                    };
                    
                    // 圆角矩形
                    if (_currentShapeTool == "RoundedRect")
                    {
                        rectangle.RadiusX = 10;
                        rectangle.RadiusY = 10;
                    }
                    
                    // 正方形：锁定宽度=高度
                    if (_currentShapeTool == "Square")
                    {
                        var size = Math.Max(width, height);
                        rectangle.Width = size;
                        rectangle.Height = size;
                        
                        // 调整位置以保持中心不变
                        if (width > height)
                        {
                            y = _rectStartPoint.Y - (size - height) / 2;
                        }
                        else
                        {
                            x = _rectStartPoint.X - (size - width) / 2;
                        }
                    }
                    
                    Canvas.SetLeft(rectangle, x);
                    Canvas.SetTop(rectangle, y);
                    
                    shapeCanvas.Children.Add(rectangle);
                    _shapes.Add(rectangle); // 添加到 shapes 列表用于撤销
                    _rectangles.Add(rectangle); // 添加到 rectangles 列表
                }
                
                // 清理状态
                _previewRectangle = null;
                _isDrawingRectangle = false;
                return;
            }
            
            // 直线/射线/线段绘制
            if (!_isDrawingLine) return;
            if (_currentShapeTool != "Line" && _currentShapeTool != "Ray" && _currentShapeTool != "LineSegment") return;
            
            // 释放鼠标捕获
            shapeCanvas.ReleaseMouseCapture();
            
            var endPoint = e.GetPosition(shapeCanvas);
            
            // 移除预览线
            if (_previewLine != null)
            {
                shapeCanvas.Children.Remove(_previewLine);
            }
            
            // 计算距离，太短则不创建
            var dx = endPoint.X - _lineStartPoint.X;
            var dy = endPoint.Y - _lineStartPoint.Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);
            
            if (distance > 5) // 至少5像素
            {
                if (_currentShapeTool == "Ray")
                {
                    // 射线：延伸到画布边界，添加箭头
                    var (extendedX, extendedY) = ExtendLineToBoundary(_lineStartPoint, endPoint, shapeCanvas.ActualWidth, shapeCanvas.ActualHeight);
                    
                    // 创建射线
                    var ray = new Line
                    {
                        X1 = _lineStartPoint.X,
                        Y1 = _lineStartPoint.Y,
                        X2 = extendedX,
                        Y2 = extendedY,
                        Stroke = new SolidColorBrush(_currentColor),
                        StrokeThickness = _currentPenSize,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round,
                        Tag = "Ray"
                    };
                    
                    shapeCanvas.Children.Add(ray);
                    _shapes.Add(ray);
                    
                    // 添加箭头头部
                    var arrowHead = CreateArrowHead(_lineStartPoint, endPoint, extendedX, extendedY);
                    if (arrowHead != null)
                    {
                        shapeCanvas.Children.Add(arrowHead);
                        _arrowHeads.Add(arrowHead);
                    }
                }
                else if (_currentShapeTool == "LineSegment")
                {
                    // 线段：两端固定，不延伸
                    var lineSegment = new Line
                    {
                        X1 = _lineStartPoint.X,
                        Y1 = _lineStartPoint.Y,
                        X2 = endPoint.X,
                        Y2 = endPoint.Y,
                        Stroke = new SolidColorBrush(_currentColor),
                        StrokeThickness = _currentPenSize,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round,
                        Tag = "LineSegment"
                    };
                    
                    shapeCanvas.Children.Add(lineSegment);
                    _shapes.Add(lineSegment);
                }
                else // Line
                {
                    // 直线
                    var line = new Line
                    {
                        X1 = _lineStartPoint.X,
                        Y1 = _lineStartPoint.Y,
                        X2 = endPoint.X,
                        Y2 = endPoint.Y,
                        Stroke = new SolidColorBrush(_currentColor),
                        StrokeThickness = _currentPenSize,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round,
                        Tag = "Line"
                    };
                    
                    shapeCanvas.Children.Add(line);
                    _shapes.Add(line);
                }
            }
            
            // 清理状态
            _previewLine = null;
            _isDrawingLine = false;
        }
        
        // 延伸线段到画布边界
        private (double X, double Y) ExtendLineToBoundary(Point start, Point end, double width, double height)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            
            // 如果两点重合，返回原终点
            if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
            {
                return (end.X, end.Y);
            }
            
            // 归一化方向向量
            var length = Math.Sqrt(dx * dx + dy * dy);
            var dirX = dx / length;
            var dirY = dy / length;
            
            // 计算与画布边界的交点
            // 我们需要找到射线与画布四条边的交点，选择最远的那个
            double maxT = double.MinValue;
            double intersectX = end.X;
            double intersectY = end.Y;
            
            // 左边 (x = 0)
            if (dirX < 0)
            {
                var t = -start.X / dirX;
                if (t > 0 && t > maxT && start.Y + t * dirY >= 0 && start.Y + t * dirY <= height)
                {
                    maxT = t;
                    intersectX = 0;
                    intersectY = start.Y + t * dirY;
                }
            }
            
            // 右边 (x = width)
            if (dirX > 0)
            {
                var t = (width - start.X) / dirX;
                if (t > maxT && start.Y + t * dirY >= 0 && start.Y + t * dirY <= height)
                {
                    maxT = t;
                    intersectX = width;
                    intersectY = start.Y + t * dirY;
                }
            }
            
            // 上边 (y = 0)
            if (dirY < 0)
            {
                var t = -start.Y / dirY;
                if (t > maxT && start.X + t * dirX >= 0 && start.X + t * dirX <= width)
                {
                    maxT = t;
                    intersectX = start.X + t * dirX;
                    intersectY = 0;
                }
            }
            
            // 下边 (y = height)
            if (dirY > 0)
            {
                var t = (height - start.Y) / dirY;
                if (t > maxT && start.X + t * dirX >= 0 && start.X + t * dirX <= width)
                {
                    maxT = t;
                    intersectX = start.X + t * dirX;
                    intersectY = height;
                }
            }
            
            // 如果没有找到交点（可能方向不指向画布），使用一个较大的值延伸到画布外
            if (maxT == double.MinValue)
            {
                // 延伸到足够远
                intersectX = start.X + dirX * 2000;
                intersectY = start.Y + dirY * 2000;
            }
            
            return (intersectX, intersectY);
        }
        
        // 创建箭头头部
        private Path? CreateArrowHead(Point start, Point end, double extendedX, double extendedY)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            
            // 归一化方向
            var length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 0.001) return null;
            
            var dirX = dx / length;
            var dirY = dy / length;
            
            // 箭头头部大小
            var arrowSize = _currentPenSize * 4;
            if (arrowSize < 12) arrowSize = 12;
            if (arrowSize > 30) arrowSize = 30;
            
            // 计算箭头两翼的点
            var angle = Math.PI / 6; // 30度
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            
            // 使用延伸后的终点作为箭头位置
            var tipX = extendedX;
            var tipY = extendedY;
            
            // 计算终点到起点的向量（用于确定箭头方向）
            var backDirX = -dirX;
            var backDirY = -dirY;
            
            // 箭头两翼的点
            var wing1X = tipX + (backDirX * cos - backDirY * sin) * arrowSize;
            var wing1Y = tipY + (backDirX * sin + backDirY * cos) * arrowSize;
            var wing2X = tipX + (backDirX * cos + backDirY * sin) * arrowSize;
            var wing2Y = tipY + (-backDirX * sin + backDirY * cos) * arrowSize;
            
            // 创建箭头路径
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(wing1X, wing1Y),
                IsClosed = true
            };
            figure.Segments.Add(new LineSegment(new Point(tipX, tipY), true));
            figure.Segments.Add(new LineSegment(new Point(wing2X, wing2Y), true));
            pathGeometry.Figures.Add(figure);
            
            var arrowHead = new Path
            {
                Data = pathGeometry,
                Fill = new SolidColorBrush(_currentColor),
                Stroke = new SolidColorBrush(_currentColor),
                StrokeThickness = 1,
                Tag = "ArrowHead"
            };
            
            return arrowHead;
        }
        
        // 更新预览矩形的位置和大小
        private void UpdatePreviewRectangle(Point currentPos)
        {
            if (_previewRectangle == null) return;
            
            double x = Math.Min(_rectStartPoint.X, currentPos.X);
            double y = Math.Min(_rectStartPoint.Y, currentPos.Y);
            double width = Math.Abs(currentPos.X - _rectStartPoint.X);
            double height = Math.Abs(currentPos.Y - _rectStartPoint.Y);
            
            // 正方形：锁定宽度=高度
            if (_currentShapeTool == "Square")
            {
                var size = Math.Max(width, height);
                _previewRectangle.Width = size;
                _previewRectangle.Height = size;
                
                if (width > height)
                {
                    y = _rectStartPoint.Y - (size - height) / 2;
                }
                else
                {
                    x = _rectStartPoint.X - (size - width) / 2;
                }
            }
            else
            {
                _previewRectangle.Width = width;
                _previewRectangle.Height = height;
            }
            
            Canvas.SetLeft(_previewRectangle, x);
            Canvas.SetTop(_previewRectangle, y);
        }
        
        // 更新预览椭圆的位置和大小
        private void UpdatePreviewEllipse(Point currentPos)
        {
            if (_previewEllipse == null) return;
            
            double x = Math.Min(_ellipseStartPoint.X, currentPos.X);
            double y = Math.Min(_ellipseStartPoint.Y, currentPos.Y);
            double width = Math.Abs(currentPos.X - _ellipseStartPoint.X);
            double height = Math.Abs(currentPos.Y - _ellipseStartPoint.Y);
            
            // 圆形：锁定宽度=高度
            if (_currentShapeTool == "Circle")
            {
                var size = Math.Max(width, height);
                _previewEllipse.Width = size;
                _previewEllipse.Height = size;
                
                if (width > height)
                {
                    y = _ellipseStartPoint.Y - (size - height) / 2;
                }
                else
                {
                    x = _ellipseStartPoint.X - (size - width) / 2;
                }
            }
            else
            {
                _previewEllipse.Width = width;
                _previewEllipse.Height = height;
            }
            
            Canvas.SetLeft(_previewEllipse, x);
            Canvas.SetTop(_previewEllipse, y);
        }
        
        #endregion
        
        #region 三角形绘制
        
        // 处理三角形绘制完成（简化版：拖拽完成）
        private void HandleTriangleMouseUp(MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[三角形] HandleTriangleMouseUp 开始");
            
            if (!_isDrawingTriangle || _previewTriangle == null) 
            {
                System.Diagnostics.Debug.WriteLine("[三角形] HandleTriangleMouseUp 提前返回");
                return;
            }
            
            try
            {
                System.Diagnostics.Debug.WriteLine("[三角形] 开始处理 MouseUp");
                
                shapeCanvas.ReleaseMouseCapture();
                
                var currentPos = e.GetPosition(shapeCanvas);
                
                // 计算三角形大小
                double width = Math.Abs(currentPos.X - _triangleStartPoint.X);
                double height = Math.Abs(currentPos.Y - _triangleStartPoint.Y);
                
                // 太小不创建
                if (width < 5 && height < 5)
                {
                    shapeCanvas.Children.Remove(_previewTriangle);
                    _isDrawingTriangle = false;
                    _previewTriangle = null;
                    return;
                }
                
                // 移除预览三角形
                shapeCanvas.Children.Remove(_previewTriangle);
                
                // 创建正式三角形
                var triangle = new Path
                {
                    Stroke = new SolidColorBrush(_currentColor),
                    StrokeThickness = _currentPenSize,
                    Fill = Brushes.Transparent,
                    Tag = _currentTriangleType
                };
                
                // 设置三角形形状
                UpdateTrianglePreviewForFinal(currentPos, triangle);
                
                shapeCanvas.Children.Add(triangle);
                _shapes.Add(triangle);
                _triangles.Add(triangle);
                
                _isDrawingTriangle = false;
                _previewTriangle = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"三角形完成错误: {ex.Message}");
                _isDrawingTriangle = false;
                _previewTriangle = null;
            }
        }
        
        // 为正式三角形设置形状
        private void UpdateTrianglePreviewForFinal(Point endPoint, Path triangle)
        {
            if (_currentTriangleType == "TriangleNormal")
            {
                var p1 = new Point(_triangleStartPoint.X, _triangleStartPoint.Y);
                var p2 = new Point(endPoint.X, _triangleStartPoint.Y);
                var p3 = new Point(_triangleStartPoint.X + Math.Abs(endPoint.X - _triangleStartPoint.X) / 2, 
                                   Math.Min(_triangleStartPoint.Y, endPoint.Y));
                
                var pathGeometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = p1, IsClosed = true };
                figure.Segments.Add(new LineSegment(p2, true));
                figure.Segments.Add(new LineSegment(p3, true));
                pathGeometry.Figures.Add(figure);
                triangle.Data = pathGeometry;
            }
            else if (_currentTriangleType == "TriangleIsosceles")
            {
                var p1 = new Point(_triangleStartPoint.X, _triangleStartPoint.Y);
                var p2 = new Point(endPoint.X, _triangleStartPoint.Y);
                var p3 = new Point(_triangleStartPoint.X + Math.Abs(endPoint.X - _triangleStartPoint.X) / 2, 
                                   Math.Min(_triangleStartPoint.Y, endPoint.Y));
                
                var pathGeometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = p1, IsClosed = true };
                figure.Segments.Add(new LineSegment(p2, true));
                figure.Segments.Add(new LineSegment(p3, true));
                pathGeometry.Figures.Add(figure);
                triangle.Data = pathGeometry;
            }
            else if (_currentTriangleType == "TriangleRight")
            {
                var p1 = new Point(_triangleStartPoint.X, _triangleStartPoint.Y);
                var p2 = new Point(endPoint.X, _triangleStartPoint.Y);
                var p3 = new Point(_triangleStartPoint.X, endPoint.Y);
                
                var pathGeometry = new PathGeometry();
                var figure = new PathFigure { StartPoint = p1, IsClosed = true };
                figure.Segments.Add(new LineSegment(p2, true));
                figure.Segments.Add(new LineSegment(p3, true));
                pathGeometry.Figures.Add(figure);
                triangle.Data = pathGeometry;
            }
        }
        
        // 重置三角形状态
        private void ResetTriangleState()
        {
            _previewTriangle = null;
            _isDrawingTriangle = false;
            _trianglePoints?.Clear();
            _currentTriangleType = "";
        }
        
        // 创建三角形路径
        private Path CreateTrianglePath(Point p1, Point p2, Point p3)
        {
            var pathGeometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = true
            };
            
            figure.Segments.Add(new LineSegment(p2, true));
            figure.Segments.Add(new LineSegment(p3, true));
            
            pathGeometry.Figures.Add(figure);
            
            return new Path
            {
                Data = pathGeometry
            };
        }
        
        // 更新预览三角形
        private void UpdatePreviewTriangle(Point currentPos)
        {
            // 修复：增加安全检查
            if (_previewTriangle == null || string.IsNullOrEmpty(_currentTriangleType)) return;
            
            Path newPreview = null;
            
            // 修复：增加 _trianglePoints 的空检查和边界检查
            if (_currentTriangleType == "TriangleNormal")
            {
                // 一般三角形：根据已有点的数量更新
                if (_trianglePoints != null && _trianglePoints.Count >= 1)
                {
                    // 第1点已确定，显示预览到第2点
                    newPreview = CreateTrianglePath(_trianglePoints[0], _trianglePoints[0], currentPos);
                }
                else if (_trianglePoints != null && _trianglePoints.Count >= 2)
                {
                    // 第2点已确定，显示预览到第3点
                    newPreview = CreateTrianglePath(_trianglePoints[0], _trianglePoints[1], currentPos);
                }
            }
            else if (_currentTriangleType == "TriangleIsosceles")
            {
                // 等腰三角形：拖动时实时显示
                var p1 = _triangleStartPoint;
                var p2 = currentPos;
                
                // 计算底边中点
                var midX = (p1.X + p2.X) / 2;
                var midY = (p1.Y + p2.Y) / 2;
                
                // 底边长度
                var baseDx = p2.X - p1.X;
                var baseDy = p2.Y - p1.Y;
                var baseLength = Math.Sqrt(baseDx * baseDx + baseDy * baseDy);
                
                if (baseLength > 0.001)
                {
                    // 归一化方向
                    var dirX = baseDx / baseLength;
                    var dirY = baseDy / baseLength;
                    
                    // 垂直向量
                    var perpX = -dirY;
                    var perpY = dirX;
                    
                    // 顶点位置
                    var p3 = new Point(midX + perpX * baseLength, midY + perpY * baseLength);
                    
                    newPreview = CreateTrianglePath(p1, p2, p3);
                }
            }
            else if (_currentTriangleType == "TriangleRight")
            {
                // 直角三角形：拖动时实时显示
                var p1 = _triangleStartPoint;
                var p2 = currentPos;
                
                var baseDx = p2.X - p1.X;
                var baseDy = p2.Y - p1.Y;
                var baseLength = Math.Sqrt(baseDx * baseDx + baseDy * baseDy);
                
                if (baseLength > 0.001)
                {
                    // 归一化方向
                    var dirX = baseDx / baseLength;
                    var dirY = baseDy / baseLength;
                    
                    // 垂直向量
                    var perpX = -dirY;
                    var perpY = dirX;
                    
                    // 第3点
                    var p3 = new Point(p2.X + perpX * baseLength, p2.Y + perpY * baseLength);
                    
                    newPreview = CreateTrianglePath(p1, p2, p3);
                }
            }
            
            if (newPreview != null)
            {
                newPreview.Stroke = _previewTriangle.Stroke;
                newPreview.StrokeThickness = _previewTriangle.StrokeThickness;
                newPreview.Fill = _previewTriangle.Fill;
                newPreview.StrokeDashArray = _previewTriangle.StrokeDashArray;
                
                // 替换旧预览
                var index = shapeCanvas.Children.IndexOf(_previewTriangle);
                if (index >= 0)
                {
                    shapeCanvas.Children[index] = newPreview;
                }
                else
                {
                    shapeCanvas.Children.Add(newPreview);
                }
                _previewTriangle = newPreview;
            }
        }
        
        #endregion
        
        #region 图形选择和移动方法
        
        // 查找父级图形元素
        private UIElement FindParentShape(FrameworkElement element)
        {
            var current = element;
            while (current != null && current != shapeCanvas)
            {
                // 检查是否是图形元素（不是顶点标记）
                if (current is Shape || current is Path)
                {
                    var tagStr = current.Tag?.ToString() ?? "";
                    if (tagStr != "polygon-vertex")
                    {
                        return current;
                    }
                }
                current = VisualTreeHelper.GetParent(current) as FrameworkElement;
            }
            return null;
        }
        
        // 选择图形
        private void SelectShape(UIElement shape)
        {
            // 清除之前的单选选中（但保留框选的多个图形）
            if (_selectedShape != null && !_selectedShapes.Contains(_selectedShape))
            {
                RemoveSelectionHighlight(_selectedShape);
            }
            
            _selectedShape = shape;
            
            // 如果不在选中列表中，添加进去
            if (!_selectedShapes.Contains(shape))
            {
                _selectedShapes.Add(shape);
            }
            
            // 添加选中效果（例如边框或阴影）
            if (shape is Shape s)
            {
                s.Stroke = new SolidColorBrush(Color.FromRgb(74, 144, 217));
                s.StrokeThickness = s.StrokeThickness + 2;
            }
            else if (shape is Path p)
            {
                p.Stroke = new SolidColorBrush(Color.FromRgb(74, 144, 217));
                p.StrokeThickness = p.StrokeThickness + 2;
            }
        }
        
        // 取消选择图形
        private void DeselectShape()
        {
            if (_selectedShape != null)
            {
                // 从选中列表中移除
                _selectedShapes.Remove(_selectedShape);
                
                // 恢复原始样式
                if (_selectedShape is Shape s)
                {
                    s.Stroke = new SolidColorBrush(_currentColor);
                    s.StrokeThickness = _currentPenSize;
                }
                else if (_selectedShape is Path p)
                {
                    p.Stroke = new SolidColorBrush(_currentColor);
                    p.StrokeThickness = _currentPenSize;
                }
                _selectedShape = null;
            }
        }
        
        // 移动图形
        private void MoveShape(Point currentPos)
        {
            if (_selectedShape != null)
            {
                // 计算移动偏移量
                double offsetX = currentPos.X - _shapeDragStartPoint.X;
                double offsetY = currentPos.Y - _shapeDragStartPoint.Y;
                
                // 更新图形位置
                double newLeft = _shapeOriginalPosition.X + offsetX;
                double newTop = _shapeOriginalPosition.Y + offsetY;
                
                Canvas.SetLeft(_selectedShape, newLeft);
                Canvas.SetTop(_selectedShape, newTop);
            }
        }
        
        #endregion
        
        #region 框选方法
        
        // 开始框选
        private void StartSelectionBox(Point startPoint)
        {
            // 如果没有按住Ctrl，先取消所有选中
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                DeselectAllShapes();
            }
            
            _isSelectingByBox = true;
            _selectionBoxStartPoint = startPoint;
            
            // 创建虚线选择框
            _selectionBox = new Rectangle
            {
                Stroke = new SolidColorBrush(Color.FromRgb(74, 144, 217)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Fill = new SolidColorBrush(Color.FromArgb(30, 74, 144, 217)),
                Tag = "selection-box"
            };
            
            // 设置初始位置和大小
            Canvas.SetLeft(_selectionBox, startPoint.X);
            Canvas.SetTop(_selectionBox, startPoint.Y);
            _selectionBox.Width = 0;
            _selectionBox.Height = 0;
            
            shapeCanvas.Children.Add(_selectionBox);
            shapeCanvas.CaptureMouse();
        }
        
        // 更新框选矩形
        private void UpdateSelectionBox(Point currentPoint)
        {
            if (_selectionBox == null) return;
            
            double x = Math.Min(_selectionBoxStartPoint.X, currentPoint.X);
            double y = Math.Min(_selectionBoxStartPoint.Y, currentPoint.Y);
            double width = Math.Abs(currentPoint.X - _selectionBoxStartPoint.X);
            double height = Math.Abs(currentPoint.Y - _selectionBoxStartPoint.Y);
            
            Canvas.SetLeft(_selectionBox, x);
            Canvas.SetTop(_selectionBox, y);
            _selectionBox.Width = width;
            _selectionBox.Height = height;
        }
        
        // 结束框选，选中框内的图形
        private void EndSelectionBox(Point endPoint)
        {
            _isSelectingByBox = false;
            shapeCanvas.ReleaseMouseCapture();
            
            if (_selectionBox == null) return;
            
            // 计算选择框的区域
            double x = Math.Min(_selectionBoxStartPoint.X, endPoint.X);
            double y = Math.Min(_selectionBoxStartPoint.Y, endPoint.Y);
            double width = Math.Abs(endPoint.X - _selectionBoxStartPoint.X);
            double height = Math.Abs(endPoint.Y - _selectionBoxStartPoint.Y);
            
            // 移除选择框
            shapeCanvas.Children.Remove(_selectionBox);
            _selectionBox = null;
            
            // 太小的选择框忽略（避免误操作）
            if (width < 5 || height < 5)
            {
                return;
            }
            
            // 查找框内的所有图形
            var shapesInBox = GetShapesInRect(x, y, width, height);
            
            // 添加到选中列表
            foreach (var shape in shapesInBox)
            {
                if (!_selectedShapes.Contains(shape))
                {
                    _selectedShapes.Add(shape);
                    AddSelectionHighlight(shape);
                }
            }
            
            if (_selectedShapes.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[框选] 选中 {_selectedShapes.Count} 个图形");
            }
        }
        
        // 获取指定矩形区域内的所有图形
        private List<UIElement> GetShapesInRect(double x, double y, double width, double height)
        {
            var result = new List<UIElement>();
            
            // 创建选择区域
            var selectionRect = new Rect(x, y, width, height);
            
            foreach (UIElement child in shapeCanvas.Children)
            {
                // 跳过选择框本身和顶点标记
                if (child is Rectangle rect && rect.Tag?.ToString() == "selection-box")
                    continue;
                if (child is Ellipse ellipse && ellipse.Tag?.ToString() == "polygon-vertex")
                    continue;
                
                // 获取图形的边界
                var bounds = GetElementBounds(child);
                if (bounds == Rect.Empty) continue;
                
                // 检查是否与选择区域相交
                if (selectionRect.IntersectsWith(bounds))
                {
                    result.Add(child);
                }
            }
            
            return result;
        }
        
        // 获取元素的边界矩形
        private Rect GetElementBounds(UIElement element)
        {
            double left = 0, top = 0, width = 0, height = 0;
            
            if (element is Shape shape)
            {
                // 对于基本形状
                left = Canvas.GetLeft(element);
                top = Canvas.GetTop(element);
                
                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;
                
                if (shape is Line line)
                {
                    // 直线需要特殊处理
                    double x1 = line.X1, y1 = line.Y1, x2 = line.X2, y2 = line.Y2;
                    left = Math.Min(x1, x2) + left;
                    top = Math.Min(y1, y2) + top;
                    width = Math.Abs(x2 - x1);
                    height = Math.Abs(y2 - y1);
                }
                else
                {
                    width = shape.Width;
                    height = shape.Height;
                    if (double.IsNaN(width)) width = 0;
                    if (double.IsNaN(height)) height = 0;
                }
            }
            else if (element is Path path)
            {
                // 对于路径（三角形、多边形等）
                left = Canvas.GetLeft(element);
                top = Canvas.GetTop(element);
                
                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;
                
                // 获取Path的边界
                if (path.Data is PathGeometry pathGeom)
                {
                    var bounds = pathGeom.Bounds;
                    width = bounds.Width;
                    height = bounds.Height;
                    left = bounds.Left;
                    top = bounds.Top;
                }
            }
            else
            {
                // 其他元素
                left = Canvas.GetLeft(element);
                top = Canvas.GetTop(element);
                
                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;
            }
            
            // 如果边界无效，返回空矩形
            if (width <= 0 || height <= 0)
                return Rect.Empty;
            
            return new Rect(left, top, width, height);
        }
        
        // 添加选中高亮效果
        private void AddSelectionHighlight(UIElement shape)
        {
            if (shape is Shape s)
            {
                // 保存原始边框到Tag中（如果是Path则特殊处理）
                var originalStroke = s.Stroke;
                var originalThickness = s.StrokeThickness;
                
                // 设置高亮样式
                s.Stroke = new SolidColorBrush(Color.FromRgb(74, 144, 217));
                s.StrokeThickness = s.StrokeThickness + 2;
            }
            else if (shape is Path p)
            {
                p.Stroke = new SolidColorBrush(Color.FromRgb(74, 144, 217));
                p.StrokeThickness = p.StrokeThickness + 2;
            }
        }
        
        // 移除选中高亮效果
        private void RemoveSelectionHighlight(UIElement shape)
        {
            if (shape is Shape s)
            {
                s.Stroke = new SolidColorBrush(_currentColor);
                s.StrokeThickness = Math.Max(1, s.StrokeThickness - 2);
            }
            else if (shape is Path p)
            {
                p.Stroke = new SolidColorBrush(_currentColor);
                p.StrokeThickness = Math.Max(1, p.StrokeThickness - 2);
            }
        }
        
        // 取消所有图形选中
        private void DeselectAllShapes()
        {
            foreach (var shape in _selectedShapes.ToList())
            {
                RemoveSelectionHighlight(shape);
            }
            _selectedShapes.Clear();
            _selectedShape = null;
        }
        
        // 切换图形选中状态（Ctrl+点击时使用）
        private void ToggleShapeSelection(UIElement shape)
        {
            if (_selectedShapes.Contains(shape))
            {
                // 取消选中
                _selectedShapes.Remove(shape);
                RemoveSelectionHighlight(shape);
                
                if (_selectedShape == shape)
                {
                    _selectedShape = _selectedShapes.FirstOrDefault();
                }
            }
            else
            {
                // 添加选中
                _selectedShapes.Add(shape);
                AddSelectionHighlight(shape);
                _selectedShape = shape;
            }
        }
        
        // 保存所有选中图形的原始位置
        private Dictionary<UIElement, Point> _selectedShapesOriginalPositions = new();
        
        private void SaveSelectedShapesOriginalPositions()
        {
            _selectedShapesOriginalPositions.Clear();
            
            foreach (var shape in _selectedShapes)
            {
                var pos = new Point(
                    Canvas.GetLeft(shape),
                    Canvas.GetTop(shape)
                );
                
                if (double.IsNaN(pos.X)) pos.X = 0;
                if (double.IsNaN(pos.Y)) pos.Y = 0;
                
                _selectedShapesOriginalPositions[shape] = pos;
            }
        }
        
        // 移动所有选中的图形
        private void MoveSelectedShapes(Point currentPos)
        {
            if (_selectedShapes.Count == 0) return;
            
            double offsetX = currentPos.X - _shapeDragStartPoint.X;
            double offsetY = currentPos.Y - _shapeDragStartPoint.Y;
            
            foreach (var shape in _selectedShapes)
            {
                if (_selectedShapesOriginalPositions.TryGetValue(shape, out var originalPos))
                {
                    double newLeft = originalPos.X + offsetX;
                    double newTop = originalPos.Y + offsetY;
                    
                    Canvas.SetLeft(shape, newLeft);
                    Canvas.SetTop(shape, newTop);
                }
            }
        }
        
        #endregion
        
        #region 键盘事件
        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+Z 撤销
            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OnUndo(this, new RoutedEventArgs());
                e.Handled = true;
            }
            // Ctrl+Y 重做
            else if (e.Key == Key.Y && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OnRedo(this, new RoutedEventArgs());
                e.Handled = true;
            }
            // Delete 删除选中的图形
            else if (e.Key == Key.Delete)
            {
                DeleteSelectedShapes();
                e.Handled = true;
            }
            // Escape 取消选中
            else if (e.Key == Key.Escape)
            {
                DeselectAllShapes();
                e.Handled = true;
            }
            // F11 全屏
            else if (e.Key == Key.F11)
            {
                OnFullScreen(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }
        
        // 删除选中的图形
        private void DeleteSelectedShapes()
        {
            if (_selectedShapes.Count == 0) return;
            
            foreach (var shape in _selectedShapes.ToList())
            {
                // 从画布移除
                shapeCanvas.Children.Remove(shape);
                
                // 从相关列表中移除
                _shapes.Remove(shape);
                _rectangles.Remove(shape);
                _ellipses.Remove(shape);
                _triangles.Remove(shape);
                _polygons.Remove(shape);
            }
            
            var count = _selectedShapes.Count;
            _selectedShapes.Clear();
            _selectedShape = null;
            
            System.Diagnostics.Debug.WriteLine($"[选择] 删除了 {count} 个图形");
        }
        
        #endregion
    }
}