Sliding Panel Sample
====================

**Description:** A self-contained, custom user control, a panel that can be
dragged and animated. Provides a performance optimization trick for Windows
Phone Silverlight. The trick is not needed on newer platforms.

**Supported platforms:** Windows Universal (Windows Store Apps and Windows Phone 8.1), Windows Phone Silverlight

![Screenshot 1](https://raw.githubusercontent.com/tompaana/my-2-bits/master/SlidingPanelSample/Screenshots/SlidingPanelSampleSLScreenshot1Small.png)&nbsp;
![Screenshot 2](https://raw.githubusercontent.com/tompaana/my-2-bits/master/SlidingPanelSample/Screenshots/SlidingPanelSampleSLScreenshot2Small.png)&nbsp;
![Screenshot 3](https://raw.githubusercontent.com/tompaana/my-2-bits/master/SlidingPanelSample/Screenshots/SlidingPanelSampleSLScreenshot3Small.png)


I was working on a project and one of the tasks assigned to me was to create a
panel, which had all kinds of controls (images, text blocks, slider, buttons,
you name it) and which should also be able to move out of the way to expose a
view beneath - user could slide it by dragging or simply tap an icon to minimize
the panel.

An obvious approach is to use [TranslateTranform class](https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.media.translatetransform.aspx)
to modify the control position as it is designed for this very purpose and
provides optimization out-of-the-box. However, due to various reasons, the
project I was working on was targeting Windows Phone 8.1 *Silverlight*, and I
quickly learned that unlike in Windows Universal Apps, the performance in
Silverlight was far from satisfactory. The more complex the layout on the panel
grew, the worse the performance got. After experimenting and discussing with
colleagues, it came apparent that the good-old, commonly used method of
rendering the layout to a snapshot was the solution. So, before animating the
panel, it is rendered to a bitmap:

```cs
bitmap = new WriteableBitmap((int)PanelWidth, (int)FullscreenPanelHeight);
bitmap.Render(fullscreenPanel, null); // fullscreenPanel is the name of the layout
bitmap.Invalidate();
fullscreenPanelSnapshot.Source = bitmap;
```

...and the bitmap is then animated instead of the layout - `fullscreenPanel` is
hidden and `fullscreenPanelSnapshot` is shown instead. After the animation is
complete, the snapshots are hidden and the actual layout shown again.

As said, this trick is only needed in Silverlight as the performance is superb
in Universal apps, when animating the layout regardless of its complexity.
Fortunately so, because rendering a layout to a bitmap is not straightforward
in Universal apps at all.

This sample provides a version of the panel for both Universal apps and
Silverlight. Even with the optimization trick in Silverlight, the performance is
way better in Universal app solution. The code-behind part of the sliding panel
control is shared by the two solutions, but for obvious reasons, the XAML files
are different. See the source code files:

* [SlidingPanel.xaml.cs (shared by all solutions)](https://github.com/tompaana/my-2-bits/blob/master/SlidingPanelSample/SlidingPanelSampleUniversal/SlidingPanelSample.Shared/SlidingPanel.xaml.cs)
* [SlidingPanel.xaml (Universal apps)](https://github.com/tompaana/my-2-bits/blob/master/SlidingPanelSample/SlidingPanelSampleUniversal/SlidingPanelSample.Shared/SlidingPanel.xaml)
* [SlidingPanelSilverlight.xaml (Silverlight)](https://github.com/tompaana/my-2-bits/blob/master/SlidingPanelSample/SlidingPanelSampleSilverlight/SlidingPanelSilverlight.xaml)
