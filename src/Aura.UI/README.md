# Aura.UI

The UI kit of [Aura Operating System](https://github.com/aura-systems/Aura-Operating-System) as a library, for any kernel built on Cosmos gen3. It draws on `Cosmos.Kernel.System`'s `Canvas` and needs nothing else.

PegasiOS builds its phone shell on it. Aura and GlieseOS can reference it the same way; neither has been moved onto it yet.

## Where the code comes from

`SRC/Aura_OS/System/Graphics/UI/GUI` of Aura at commit `821a5f15`, with the parts of Aura's mouse manager, keyboard manager and window manager that only deal with components. The files keep Aura's headers, style and public names.

| In the library | From Aura |
|---|---|
| `Components/`: `Component`, `Button`, `Checkbox`, `DropDown`, `EditMenu`, `Label`, `ListBox`, `Panel`, `Picture`, `RightClick`, `Slider`, `Surface`, `TextBox`, `Window` | `GUI/Components/` |
| `Skin/`: `Frame`, `RegionListBuilder`, `SkinParsing`, `ThemeManager` | `GUI/Skin/` |
| `Layout/`: `LayoutNode`, `StackNode`, `GridNode`, `PanelNode`, `ControlNode` | `GUI/Layout/` |
| `Rectangle`, `TextClipboard` | `GUI/Rectangle.cs`, `System/TextClipboard.cs` |
| `WindowManager` | the component half of `GUI/WindowManager.cs` |
| `PointerManager` | `System/Input/MouseManager.cs` |
| `KeyboardInput` | `System/Input/KeyboardManager.cs` |
| `Parser/NanoXMLParser.cs` | `System/Parser/` |
| `Resources/`: the Suave skin, the window and cursor icons | `Resources/UI/` |

What stayed in Aura because it is Aura's desktop and not a kit: `Application`, `Desktop`, `Dialog`, `LoginScreen`, `StartMenu`, `Taskbar`, the `Console` and `Filesystem` components, `ClipboardButton`, `KeyboardLayoutButton`, and the loader of layout files (`AppLayout`, `LayoutLoader`), which builds Aura applications and dialogs.

## Referencing it

The library is built against `Cosmos.Kernel.System` 3.0.97, the version this repository's `global.json` pins: a kernel on an older Cosmos has to move up first. It is not published on nuget.org. Two ways to take it:

- A project reference, with this repository as a submodule of the kernel's: `<ProjectReference Include="path/to/PegasiOS/src/Aura.UI/Aura.UI.csproj" />`. PegasiOS references it this way.
- A package from a folder feed: `dotnet pack src/Aura.UI/Aura.UI.csproj -c Release -o <feed>`, the feed added to the kernel's `NuGet.Config`, then `<PackageReference Include="Aura.UI" Version="0.1.0" />`. A copy of the PegasiOS kernel built this way, without the library's sources, boots and draws the skin.

## Using it

```csharp
Canvas screen = Canvas.GetFullScreen();
Gui.Initialize(screen, PCScreenFont.DefaultFont);   // loads the Suave skin and the icons

Button button = new Button("Hello", 40, 120, 320, 80);
button.Click = () => { /* ... */ };
```

Then, once per frame:

```csharp
Gui.Pointer.Update();        // the kernel's mouse; Update(x, y, left, right) for a touchscreen
KeyboardInput.Update();      // the kernel's keys, for TextBox and ListBox
button.Update();             // the host updates the components it shows
Gui.Windows.DrawWindows();   // draws what changed, composes the root components on the screen
Gui.Pointer.DrawCursor();    // for a mouse
screen.Display();
```

Two rules the code relies on:

- Create components after `Gui.Initialize`: a component registers with the window manager in its constructor.
- The window manager draws a changed child into its parent, but a parent whose `Draw` clears it must draw its children again itself (`child.Draw(parent)`). Aura's `Application` does it for a window's controls; `Container` in `src/PegasiOS/Shell/Controls` does it for PegasiOS.

## What changed from Aura's code

Aura's components read the kernel's statics. Here they read `Gui`, which the host fills in once:

| Aura | Aura.UI |
|---|---|
| `Kernel.font` | `Gui.Font` |
| `Kernel.ThemeManager` | `Gui.Theme` |
| `Kernel.ResourceManager` | `Gui.Resources` |
| `Explorer.WindowManager` | `Gui.Windows` |
| `Kernel.MouseManager`, the kernel's `MouseManager.X` and `Y` | `Gui.Pointer` |
| `Input.KeyboardManager.TryGetKey`, `IsShortcut`, `IsShiftHeld` | `KeyboardInput` |
| `Kernel.ScreenWidth`, `Kernel.ScreenHeight`, `Explorer.Screen` | `Gui.ScreenWidth`, `Gui.ScreenHeight`, `Gui.Screen` |
| `Kernel.WhiteColor`, `BlackColor`, `Gray`, `DarkGray`, `SelectionColor` | `GuiColors` |
| `Taskbar.taskbarHeight`, in `DropDown` | `Gui.BottomInset` |
| `WindowManager.FocusedApp`, an `Application` | `WindowManager.FocusedOwner`, an `IKeyOwner` |
| `Kernel.GuiDebug` | `WindowManager.Debug` |
| windows and taskbar faded by type | `Component.Opacity` |

The rest of the differences:

- The pointer is no longer the mouse. `PointerManager.Update(x, y, left, right)` takes a position and buttons from anywhere; `Update()` reads the kernel's mouse. The cursor is drawn by `DrawCursor()`, which the host calls or not.
- `PointerManager.IsLeftButtonReleased` is new: true for the one update in which the button went up, as `IsLeftButtonPressed` is for the one in which it went down. A tap needs it.
- `KeyboardInput.Filter` gives the host each key first, for its global shortcuts. Aura's own (Ctrl+Alt+Del, Alt+F4, the Windows key) stayed in Aura. `Enqueue` types a key from code.
- `ThemeManager.Load(xml, bitmapLoader)` takes a skin from wherever the host keeps it; `LoadDefault()` loads the embedded Suave. Reading the theme's path from Aura's settings stayed in Aura.
- `ResourceManager` holds the kit's own icons. The lookup of a package's icons stayed in Aura. A missing icon throws `KeyNotFoundException` instead of stopping the kernel.
- `WindowManager` has `DrawHostLayers()`, empty, where Aura draws its applications, and `HasDirtyComponents()` for a host that draws a frame only when something changed.
- `ControlNode.MarkPlaced`, `ControlNode.ChangedSincePlaced` and the setters of `Window.MaxWidth` and `MaxHeight` are public: the layout loader that calls them is now in another assembly.
- One fix: a component's absolute position is computed in its constructor. In Aura a root component created away from (0, 0) and never moved keeps an absolute position of (0, 0), so `IsInside` tests the wrong rectangle. PegasiOS's navigation bar, at the bottom of the screen, did not get its taps until this was fixed. Aura's constructor is the same code.

## Known limits

- Some sizes assume Aura's 8x16 font: a `Checkbox` is 13 pixels high and a context menu or drop-down row 20, so a taller font is cut there. `Button`, `Label`, `ListBox` and `TextBox` drew whole with the kernel's 16x32 font.
- A click is sent when the button goes down, as on a desktop. A control for a finger wants the release: PegasiOS's `TouchButton` shows how to build it on `PointerManager.IsLeftPressOn` and `IsLeftButtonReleased`.
- The state is static (`Gui`, `Component.Components`): one interface per kernel.
- The code is not annotated for nullable reference types.
- No test project yet.

## Checked

In the PinePhone virtual machine, on PegasiOS: the Suave skin and the icons load from the embedded resources; `Button`, `Checkbox`, `Slider`, `ListBox`, `DropDown` and `TextBox` draw; a click reaches the button and the check box, a drag moves the slider, the drop-down opens, and keys typed on the virtual keyboard land in the focused text box. `Window`, `Surface`, `RightClick` on a right click, `GridNode` and the clipboard shortcuts were compiled and not exercised.

## Credits and licenses

- The code is Aura's, BSD 3-Clause: [LICENSE.md](LICENSE.md).
- `Parser/NanoXMLParser.cs` is NanoXML, under the Code Project Open License: [Parser/LICENSE.md](Parser/LICENSE.md).
- The Suave skin was put together by Markus Ewald from a Windows XP theme by Mohsin Naqi, who asks to be credited for the art. The header of `Resources/Themes/Suave.skin.xml` has the details.
