// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Aura.UI.Components;
using Aura.UI.Layout;
using PegasiOS.Shell.Controls;

namespace PegasiOS.Shell;

/// <summary>
/// One screen of the shell: it fills the space between the status bar and the navigation bar,
/// and lays its controls out one under the other with the kit's stack layout.
/// </summary>
internal abstract class Screen : Container
{
    /// <summary>Space between the edge of the screen and its content.</summary>
    protected const int Margin = 24;

    /// <summary>Space between two controls of the screen.</summary>
    private const int Spacing = 24;

    /// <summary>How much larger than the font a screen's title is drawn.</summary>
    private const int TitleScale = 2;

    private readonly StackNode _stack = new()
    {
        Spacing = Spacing,
        Padding = new Thickness(Margin, Margin, Margin, Margin)
    };

    /// <summary>What the serial log calls the screen.</summary>
    public string Name { get; }

    protected PhoneShell Shell { get; }

    /// <summary>The width a control has when it spans the screen between its margins.</summary>
    protected int ContentWidth => Width - (Margin * 2);

    protected Screen(PhoneShell shell, string name) : base(0, shell.ContentTop, shell.Width, shell.ContentHeight)
    {
        Shell = shell;
        Name = name;
        Background = Palette.Background;
    }

    /// <summary>Called once per frame while the screen is the one shown.</summary>
    public virtual void Tick(DateTime now)
    {
    }

    /// <summary>Adds the screen's title, in large letters, at the top.</summary>
    protected void AddTitle(string title)
    {
        Add(new ScaledLabel(title, Palette.Text, TitleScale, 0, 0), Alignment.Start);
    }

    /// <summary>
    /// Adds a control under the ones already on the screen. Stretch gives it the content's width;
    /// the other alignments keep its own and place it left, centered or right.
    /// </summary>
    /// <param name="control">The control to add.</param>
    /// <param name="alignment">Where the control sits across the screen.</param>
    /// <param name="topMargin">Space above the control, on top of the screen's spacing.</param>
    protected void Add(Component control, Alignment alignment = Alignment.Stretch, int topMargin = 0)
    {
        AddChild(control);
        _stack.Children.Add(new ControlNode(control)
        {
            HorizontalAlignment = alignment,
            Margin = new Thickness(0, topMargin, 0, 0)
        });
    }

    /// <summary>Places the controls added so far. A screen calls it once it has added them all.</summary>
    protected void Arrange()
    {
        _stack.Measure();
        _stack.Arrange(0, 0, Width, Height);
        MarkDirty();
    }
}
