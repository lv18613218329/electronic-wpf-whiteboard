# WPF Electronic Whiteboard

一个基于 WPF 的电子白板教学软件，支持多种图形绘制和编辑功能。

## 功能特点

### 基础绘图工具
- ✏️ 画笔 / 🖍️ 荧光笔
- 🧹 橡皮擦
- 📏 直线 / 射线 / 线段
- ⬜ 矩形 / 正方形 / 圆角矩形
- ⭕ 椭圆 / 圆形
- 🔺 三角形（普通/等腰/直角）
- ⬡ 多边形（点击顶点+双击完成）
- ➡️ 箭头

### 选择与移动
- 👆 选择工具
- 选中图形后拖动移动位置

### 学科工具
- 📐 数学符号
- ⚛️ 物理符号
- 🧪 化学符号

### 其他功能
- 📑 多页面管理
- ↩️ 撤销 / ↪️ 重做
- 🖥️ 全屏模式
- 主题切换

## 运行要求

- .NET 8.0 SDK
- Windows 10/11

## 构建与运行

```bash
cd src
dotnet build
dotnet run
```

## 项目结构

```
electronic-wpf-whiteboard/
├── src/
│   ├── MainWindow.xaml      # 主窗口
│   ├── MainWindow.xaml.cs   # 主窗口逻辑
│   ├── App.xaml             # 应用程序入口
│   ├── Models/              # 数据模型
│   └── Services/            # 服务层
├── .gitignore
└── README.md
```

## 许可证

MIT License