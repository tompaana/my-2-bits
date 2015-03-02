Tick Tick
=========

**Description:** A custom text ticker user control: A text component, which
scrolls to display the full text, if the text does not fit on a single line
without wrapping.

**Supported platforms:** Windows Universal (Windows Store Apps and Windows Phone 8.1), Windows Phone Silverlight

![Screenshot](https://raw.githubusercontent.com/tompaana/my-2-bits/master/TickTick/Screenshots/TickTickScreenshotSmall.png)
*Three text ticker controls in `StackPanel`*

I found myself in that rare situation, where nothing else but a text ticker UI
component could fulfil the need in the app I was building. This seldom used
component seemed to be missing from the set of controls offered by Windows
(Phone) framework. *(If the control actual exists in some toolkit, it's too late
now and I don't wanna know.)*

Yes. It's not hard to create a custom text ticker component. But wouldn't it be
nice to have one ready-made and save some time not having to create one from
scratch? Well, here you go, you're welcome. And if you're not satisfied with
this, you can improve it or create a better one.

The control is self-contained and easy to use. Here's an example:

```xml
<local:TextTickerControl
    Foreground="White"
    VisibleWidth="450"
    StartDelayInMilliseconds="4000"
    Text="This is a text that is too long to be displayed on a single line without wrapping." />
```

...where `local` is the namespace where `TextTickerControl` resides.

Only thing I'm not happy with is the fact that you have to define the visible
width. It'd probably be very easy to get rid of this nasty property, but I'm
just too lazy to fix it as the control as-is fulfils my needs. If it bothers
you and you find a neat solution, don't be afraid to share and make a patch.

The (new) properties of the user control are as follows:

* `VisibleWidth` (`double`): Consider this as standard `Width`
* `Text` (`string`)
* `StartDelayInMilliseconds` (`int`): Default value is 8000
* `RestartDelayInMilliseconds` (`int`): Default value is 8000

The inherited properties include the usual `Foreground`, `FontFamily`,
`FontSize` etc.

For more information, see the source code files:

* [TextTickerControl.xaml](https://github.com/tompaana/my-2-bits/blob/master/TickTick/TickTick/TickTick.Shared/TextTickerControl.xaml)
* [TextTickerControl.xaml.cs](https://github.com/tompaana/my-2-bits/blob/master/TickTick/TickTick/TickTick.Shared/TextTickerControl.xaml.cs)
