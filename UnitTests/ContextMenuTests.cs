﻿using System.Globalization;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using GraphViewTests = Terminal.Gui.Views.GraphViewTests;

namespace Terminal.Gui.Core {
	public class ContextMenuTests {
		readonly ITestOutputHelper output;

		public ContextMenuTests (ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		[AutoInitShutdown]
		public void ContextMenu_Constructors ()
		{
			var cm = new ContextMenu ();
			Assert.Equal (new Point (0, 0), cm.Position);
			Assert.Empty (cm.MenuItems.Children);
			Assert.Null (cm.Host);
			cm.Position = new Point (20, 10);
			cm.MenuItems = new MenuBarItem (new MenuItem [] {
				new MenuItem ("First", "", null)
			});
			Assert.Equal (new Point (20, 10), cm.Position);
			Assert.Single (cm.MenuItems.Children);

			cm = new ContextMenu (5, 10,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);
			Assert.Equal (new Point (5, 10), cm.Position);
			Assert.Equal (2, cm.MenuItems.Children.Length);
			Assert.Null (cm.Host);

			cm = new ContextMenu (new View () { X = 5, Y = 10 },
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);
			Assert.Equal (new Point (5, 10), cm.Position);
			Assert.Equal (2, cm.MenuItems.Children.Length);
			Assert.NotNull (cm.Host);
		}

		[Fact]
		[AutoInitShutdown]
		public void Show_Hide_IsShow ()
		{
			var cm = new ContextMenu (10, 5,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			cm.Show ();
			Assert.True (ContextMenu.IsShow);

			Application.Begin (Application.Top);

			var expected = @"
          ┌──────┐
          │ One  │
          │ Two  │
          └──────┘
";

			GraphViewTests.AssertDriverContentsAre (expected, output);

			cm.Hide ();
			Assert.False (ContextMenu.IsShow);

			Application.Refresh ();

			expected = "";

			GraphViewTests.AssertDriverContentsAre (expected, output);
		}

		[Fact]
		[AutoInitShutdown]
		public void Position_Changing ()
		{
			var cm = new ContextMenu (10, 5,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			cm.Show ();
			Application.Begin (Application.Top);

			var expected = @"
          ┌──────┐
          │ One  │
          │ Two  │
          └──────┘
";

			GraphViewTests.AssertDriverContentsAre (expected, output);

			cm.Position = new Point (5, 10);

			cm.Show ();
			Application.Refresh ();

			expected = @"
     ┌──────┐
     │ One  │
     │ Two  │
     └──────┘
";

			GraphViewTests.AssertDriverContentsAre (expected, output);

		}

		[Fact]
		[AutoInitShutdown]
		public void MenuItens_Changing ()
		{
			var cm = new ContextMenu (10, 5,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			cm.Show ();
			Application.Begin (Application.Top);

			var expected = @"
          ┌──────┐
          │ One  │
          │ Two  │
          └──────┘
";

			GraphViewTests.AssertDriverContentsAre (expected, output);

			cm.MenuItems = new MenuBarItem (new MenuItem [] {
				new MenuItem ("First", "", null),
				new MenuItem ("Second", "", null),
				new MenuItem ("Third", "", null)
			});


			cm.Show ();
			Application.Refresh ();

			expected = @"
          ┌─────────┐
          │ First   │
          │ Second  │
          │ Third   │
          └─────────┘
";

			GraphViewTests.AssertDriverContentsAre (expected, output);

		}

		[Fact, AutoInitShutdown]
		public void Key_Changing ()
		{
			var lbl = new Label ("Original");

			var cm = new ContextMenu ();

			lbl.KeyPress += (e) => {
				if (e.KeyEvent.Key == cm.Key) {
					lbl.Text = "Replaced";
					e.Handled = true;
				}
			};

			var top = Application.Top;
			top.Add (lbl);
			Application.Begin (top);

			Assert.True (lbl.ProcessKey (new KeyEvent (cm.Key, new KeyModifiers ())));
			Assert.Equal ("Replaced", lbl.Text);

			lbl.Text = "Original";
			cm.Key = Key.Space | Key.CtrlMask;
			Assert.True (lbl.ProcessKey (new KeyEvent (cm.Key, new KeyModifiers ())));
			Assert.Equal ("Replaced", lbl.Text);
		}

		[Fact, AutoInitShutdown]
		public void MouseFlags_Changing ()
		{
			var lbl = new Label ("Original");

			var cm = new ContextMenu ();

			lbl.MouseClick += (e) => {
				if (e.MouseEvent.Flags == cm.MouseFlags) {
					lbl.Text = "Replaced";
					e.Handled = true;
				}
			};

			var top = Application.Top;
			top.Add (lbl);
			Application.Begin (top);

			Assert.True (lbl.OnMouseEvent (new MouseEvent () { Flags = cm.MouseFlags }));
			Assert.Equal ("Replaced", lbl.Text);

			lbl.Text = "Original";
			cm.MouseFlags = MouseFlags.Button2Clicked;
			Assert.True (lbl.OnMouseEvent (new MouseEvent () { Flags = cm.MouseFlags }));
			Assert.Equal ("Replaced", lbl.Text);
		}

		[Fact, AutoInitShutdown]
		public void KeyChanged_Event ()
		{
			var oldKey = Key.Null;
			var cm = new ContextMenu ();

			cm.KeyChanged += (e) => oldKey = e;

			cm.Key = Key.Space | Key.CtrlMask;
			Assert.Equal (Key.Space | Key.CtrlMask, cm.Key);
			Assert.Equal (Key.F10 | Key.ShiftMask, oldKey);
		}

		[Fact, AutoInitShutdown]
		public void MouseFlagsChanged_Event ()
		{
			var oldMouseFlags = new MouseFlags ();
			var cm = new ContextMenu ();

			cm.MouseFlagsChanged += (e) => oldMouseFlags = e;

			cm.MouseFlags = MouseFlags.Button2Clicked;
			Assert.Equal (MouseFlags.Button2Clicked, cm.MouseFlags);
			Assert.Equal (MouseFlags.Button3Clicked, oldMouseFlags);
		}

		[Fact, AutoInitShutdown]
		public void Show_Ensures_Display_Inside_The_Container_But_Preserves_Position ()
		{
			var cm = new ContextMenu (80, 25,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			Assert.Equal (new Point (80, 25), cm.Position);

			cm.Show ();
			Assert.Equal (new Point (80, 25), cm.Position);
			Application.Begin (Application.Top);

			var expected = @"
                                                                        ┌──────┐
                                                                        │ One  │
                                                                        │ Two  │
                                                                        └──────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (72, 21, 80, 4), pos);

			cm.Hide ();
			Assert.Equal (new Point (80, 25), cm.Position);
		}

		[Fact, AutoInitShutdown]
		public void Show_Ensures_Display_Inside_The_Container_Without_Overlap_The_Host ()
		{
			var view = new View ("View") {
				X = Pos.AnchorEnd (10),
				Y = Pos.AnchorEnd (1),
				Width = 10,
				Height = 1
			};
			var cm = new ContextMenu (view,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			Application.Top.Add (view);
			Application.Begin (Application.Top);

			Assert.Equal (new Rect (70, 24, 10, 1), view.Frame);
			Assert.Equal (new Point (0, 0), cm.Position);

			cm.Show ();
			Assert.Equal (new Point (70, 24), cm.Position);
			Application.Top.Redraw (Application.Top.Bounds);

			var expected = @"
                                                                      ┌──────┐
                                                                      │ One  │
                                                                      │ Two  │
                                                                      └──────┘
                                                                      View
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (70, 20, 78, 5), pos);

			cm.Hide ();
			Assert.Equal (new Point (70, 24), cm.Position);
		}

		[Fact, AutoInitShutdown]
		public void Show_Display_Below_The_Bottom_Host_If_Has_Enough_Space ()
		{
			var view = new View ("View") { X = 10, Y = 5, Width = 10, Height = 1 };
			var cm = new ContextMenu (view,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			Application.Top.Add (view);
			Application.Begin (Application.Top);

			Assert.Equal (new Point (10, 5), cm.Position);

			cm.Show ();
			Application.Top.Redraw (Application.Top.Bounds);
			Assert.Equal (new Point (10, 5), cm.Position);

			var expected = @"
          View
          ┌──────┐
          │ One  │
          │ Two  │
          └──────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (10, 5, 18, 5), pos);

			cm.Hide ();
			Assert.Equal (new Point (10, 5), cm.Position);
			cm.Host.X = 5;
			cm.Host.Y = 10;
			cm.Host.Height = 3;

			cm.Show ();
			Application.Top.Redraw (Application.Top.Bounds);
			Assert.Equal (new Point (5, 12), cm.Position);

			expected = @"
     View


     ┌──────┐
     │ One  │
     │ Two  │
     └──────┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (5, 10, 13, 7), pos);

			cm.Hide ();
			Assert.Equal (new Point (5, 12), cm.Position);
		}

		[Fact, AutoInitShutdown]
		public void Show_Display_At_Zero_If_The_Toplevel_Width_Is_Less_Than_The_Menu_Width ()
		{
			var cm = new ContextMenu (0, 0,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			Assert.Equal (new Point (0, 0), cm.Position);

			cm.Show ();
			Assert.Equal (new Point (0, 0), cm.Position);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (5, 25);

			var expected = @"
┌────
│ One
│ Two
└────
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 1, 5, 4), pos);

			cm.Hide ();
			Assert.Equal (new Point (0, 0), cm.Position);
		}

		[Fact, AutoInitShutdown]
		public void Show_Display_At_Zero_If_The_Toplevel_Height_Is_Less_Than_The_Menu_Height ()
		{
			var cm = new ContextMenu (0, 0,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			Assert.Equal (new Point (0, 0), cm.Position);

			cm.Show ();
			Assert.Equal (new Point (0, 0), cm.Position);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (80, 4);

			var expected = @"
┌──────┐
│ One  │
│ Two  │
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 1, 8, 3), pos);

			cm.Hide ();
			Assert.Equal (new Point (0, 0), cm.Position);
		}

		[Fact, AutoInitShutdown]
		public void Hide_Is_Invoke_At_Container_Closing ()
		{
			var cm = new ContextMenu (80, 25,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			var top = Application.Top;
			Application.Begin (top);
			top.Running = true;

			Assert.False (ContextMenu.IsShow);

			cm.Show ();
			Assert.True (ContextMenu.IsShow);

			top.RequestStop ();
			Assert.False (ContextMenu.IsShow);
		}

		[Fact, AutoInitShutdown]
		public void ForceMinimumPosToZero_True_False ()
		{
			var cm = new ContextMenu (-1, -2,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			Assert.Equal (new Point (-1, -2), cm.Position);

			cm.Show ();
			Assert.Equal (new Point (-1, -2), cm.Position);
			Application.Begin (Application.Top);

			var expected = @"
┌──────┐
│ One  │
│ Two  │
└──────┘
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (0, 1, 8, 4), pos);

			cm.ForceMinimumPosToZero = false;
			cm.Show ();
			Assert.Equal (new Point (-1, -2), cm.Position);
			Application.Refresh ();

			expected = @"
 One  │
 Two  │
──────┘
";

			pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (1, 0, 7, 3), pos);
		}

		[Fact, AutoInitShutdown]
		public void ContextMenu_Is_Closed_If_Another_MenuBar_Is_Open_Or_Vice_Versa ()
		{
			var cm = new ContextMenu (10, 5,
				new MenuBarItem (new MenuItem [] {
					new MenuItem ("One", "", null),
					new MenuItem ("Two", "", null)
				})
			);

			var menu = new MenuBar (new MenuBarItem [] {
					new MenuBarItem ("File", "", null),
					new MenuBarItem ("Edit", "", null)
				});

			Application.Top.Add (menu);

			Assert.Null (Application.mouseGrabView);

			cm.Show ();
			Assert.True (ContextMenu.IsShow);
			Assert.Equal (cm.MenuBar, Application.mouseGrabView);
			Assert.False (menu.IsMenuOpen);
			Assert.True (menu.ProcessHotKey (new KeyEvent (Key.F9, new KeyModifiers ())));
			Assert.False (ContextMenu.IsShow);
			Assert.Equal (menu, Application.mouseGrabView);
			Assert.True (menu.IsMenuOpen);

			cm.Show ();
			Assert.True (ContextMenu.IsShow);
			Assert.Equal (cm.MenuBar, Application.mouseGrabView);
			Assert.False (menu.IsMenuOpen);
			Assert.False (menu.OnKeyDown (new KeyEvent (Key.Null, new KeyModifiers () { Alt = true })));
			Assert.True (menu.OnKeyUp (new KeyEvent (Key.Null, new KeyModifiers () { Alt = true })));
			Assert.False (ContextMenu.IsShow);
			Assert.Equal (menu, Application.mouseGrabView);
			Assert.True (menu.IsMenuOpen);

			cm.Show ();
			Assert.True (ContextMenu.IsShow);
			Assert.Equal (cm.MenuBar, Application.mouseGrabView);
			Assert.False (menu.IsMenuOpen);
			Assert.False (menu.MouseEvent (new MouseEvent () { X = 1, Flags = MouseFlags.ReportMousePosition, View = menu }));
			Assert.True (ContextMenu.IsShow);
			Assert.Equal (cm.MenuBar, Application.mouseGrabView);
			Assert.False (menu.IsMenuOpen);
			Assert.True (menu.MouseEvent (new MouseEvent () { X = 1, Flags = MouseFlags.Button1Clicked, View = menu }));
			Assert.False (ContextMenu.IsShow);
			Assert.Equal (menu, Application.mouseGrabView);
			Assert.True (menu.IsMenuOpen);
		}

		[Fact, AutoInitShutdown]
		public void ContextMenu_On_Toplevel_With_A_MenuBar_TextField_StatusBar ()
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("en-US");

			var menu = new MenuBar (new MenuBarItem [] {
				new MenuBarItem ("File", "", null),
				new MenuBarItem ("Edit", "", null)
			});

			var label = new Label ("Label:") {
				X = 2,
				Y = 3
			};

			var tf = new TextField ("TextField") {
				X = Pos.Right (label) + 1,
				Y = Pos.Top (label),
				Width = 20
			};

			var statusBar = new StatusBar (new StatusItem [] {
				new StatusItem(Key.F1, "~F1~ Help", null),
				new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", null)
			});

			Application.Top.Add (menu, label, tf, statusBar);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (45, 17);

			Assert.Equal (new Rect (9, 3, 20, 1), tf.Frame);
			Assert.True (tf.HasFocus);

			tf.ContextMenu.Show ();
			Assert.True (ContextMenu.IsShow);
			Assert.Equal (new Point (9, 3), tf.ContextMenu.Position);
			Application.Top.Redraw (Application.Top.Bounds);
			var expected = @"
  File   Edit


  Label: TextField
         ┌────────────────────────────┐
         │ Select All          Ctrl+T │
         │ Delete All    Ctrl+Shift+D │
         │ Copy                Ctrl+C │
         │ Cut                 Ctrl+X │
         │ Paste               Ctrl+V │
         │ Undo                Ctrl+Z │
         │ Redo                Ctrl+Y │
         └────────────────────────────┘



 F1 Help │ ^Q Quit
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (2, 0, 39, 17), pos);
		}

		[Fact, AutoInitShutdown]
		public void ContextMenu_On_Toplevel_With_A_MenuBar_Window_TextField_StatusBar ()
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo ("en-US");

			var menu = new MenuBar (new MenuBarItem [] {
				new MenuBarItem ("File", "", null),
				new MenuBarItem ("Edit", "", null)
			});

			var label = new Label ("Label:") {
				X = 2,
				Y = 3
			};

			var tf = new TextField ("TextField") {
				X = Pos.Right (label) + 1,
				Y = Pos.Top (label),
				Width = 20
			};

			var win = new Window ("Window");
			win.Add (label, tf);

			var statusBar = new StatusBar (new StatusItem [] {
				new StatusItem(Key.F1, "~F1~ Help", null),
				new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", null)
			});

			Application.Top.Add (menu, win, statusBar);
			Application.Begin (Application.Top);
			((FakeDriver)Application.Driver).SetBufferSize (45, 17);


			Assert.Equal (new Rect (9, 3, 20, 1), tf.Frame);
			Assert.True (tf.HasFocus);

			tf.ContextMenu.Show ();
			Assert.True (ContextMenu.IsShow);
			Assert.Equal (new Point (10, 5), tf.ContextMenu.Position);
			Application.Top.Redraw (Application.Top.Bounds);
			var expected = @"
  File   Edit
┌ Window ───────────────────────────────────┐
│                                           │
│                                           │
│                                           │
│  Label: TextField                         │
│         ┌────────────────────────────┐    │
│         │ Select All          Ctrl+T │    │
│         │ Delete All    Ctrl+Shift+D │    │
│         │ Copy                Ctrl+C │    │
│         │ Cut                 Ctrl+X │    │
│         │ Paste               Ctrl+V │    │
│         │ Undo                Ctrl+Z │    │
│         │ Redo                Ctrl+Y │    │
│         └────────────────────────────┘    │
└───────────────────────────────────────────┘
 F1 Help │ ^Q Quit
";

			var pos = GraphViewTests.AssertDriverContentsWithFrameAre (expected, output);
			Assert.Equal (new Rect (2, 0, 45, 17), pos);
		}
	}
}
