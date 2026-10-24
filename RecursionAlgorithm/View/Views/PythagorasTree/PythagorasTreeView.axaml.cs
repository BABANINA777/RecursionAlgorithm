using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RecursionAlgorithm.View.Views;

public partial class PythagorasTreeView : UserControl
{
    public PythagorasTreeView()
    {
        InitializeComponent();
        TreeControl.RenderCompleted += DrawRecursionTree;

    }

    public void DrawRecursionTree()
    {
        RecursionControl.RecursionCalls = TreeControl.RecursionCalls;
        RecursionControl.RenderRecursionTree();

    }
    
}