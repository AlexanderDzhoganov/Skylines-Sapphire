## Sapphire - Skin module reference

This document defines the syntax of skin modules.

### Table of contents
- 1.0 Overview of a module
- 1.1 List of common component properties
- 1.2 List of supported component types
- 1.3 Matching components with regular expressions
- 1.4 Optional matching
- 1.5 Sticky properties
- 1.6 Raw colors
- 1.7 Working with UIMultiStateButtons

### 1.0 Overview of a module

A skin module is any XML text file with correct Sapphire structure. The structure is described below.

Every skin module is enclosed in `<UIView>`/ `</UIView>` tags.
e.g.
```
<UIView>
  .. module contents ..
</UIView>
```

At the root of every module are one or more `<Component>` selectors. A `<Component>` selector can reference any UI component
such as panels, labels, sliders, scrollviews etc. Components are uniquely identified by their name and their place in the hierarchy.
For example, to define a `<Component>` selector with name `MyTestPanel` we'll do something like:
```
<UIView>
  <Component name="MyTestPanel">
    
  </Component>
</UIView>
```

Inside each `<Component>` selector you can specify one or more component properties to be changed. A descriptive list of the properties
of each component can be found by decompiling `ColossalManaged.dll` or by using ModTools. 

Component properties are defined like this:
```
<property>value</property>
```
e.g.
```
<UIView>
  <Component name="MyTestPanel">
    <size>12.0, 16.0</size>
  </Component>
</UIView>
```
will set `MyTestPanel`'s size to x = 12.0 and y = 16.0.

Components in the UI have sub-components. You access them by defining nested `<Component>` tags like this:
```
<UIView>
  <Component name="MyTestPanel">
    <Component name="MyTestChild">
    
    </Component>
  </Component>
</UIView>
```

Where `MyTestChild` is a child component of `MyTestPanel`.

### 1.1 List of common component properties

#### WORK IN PROGRESS - A LOT OF INFO FROM THIS SECTION IS STILL MISSING

#### Shared by all components

This is a list of properties that all components have:
- `isEnabled` - bool - example: `<isEnabled>true</isVisible>`
- `isVisible` - bool - example: `<isVisible>false</isVisible>`
- `isInteractive`
- `autoSize`
- `clipChildren`
- `anchor` 
- `opacity`
- `color`
- `disabledColor`
- `area`
- `limits`
- `size` - Vector2 - example: `<size>16.0, 12.0</size>`
- `relativePosition` - Vector3 - example: `<relativePosition>2.0, 2.0, 0.0</relativePosition>`
- `pivot`
- `arbitraryPivotOffset`
- `zOrder`
- `width`, `height`
- `minimumSize`, `maximumSize`
- `tooltipAnchor`
- `bringTooltipToFront`
- `tooltip`

#### UIPanel
- `atlas`
- `flip`
- `backgroundSprite` - Sprite - example: `<backgroundSprite>mySprite</backgroundSprite>`
- `padding`
- `autoLayout`
- `autoFitChildrenHorizontally`, `autoFitChildrenVertically`
- `wrapLayout`
- `autoLayoutDirection`
- `autoLayoutStart`
- `useCenter`
- `autoLayoutPadding`

### UIButton
- `buttonState`
- `wordWrap`
- `normalBgSprite`, `hoveredBgSprite`, `focusedBgSprite`, `pressedBgSprite`, `disabledBgSprite`
- `normalFgSprite`, `hoveredFgSprite`, `focusedFgSprite`, `pressedFgSprite`, `disabledFgSprite`
- `disabledBottomColor`
- `tabStrip`
- `autoSize`
- `horizontalAlignment`, `verticalAlignment`
- `textHorizontalAlignment`, `textVerticalAlignment`
- `textPadding`
- `textColor`, `hoveredTextColor`, `pressedTextColor`, `focusedTextColor`
- `color`, `hoveredColor`, `pressedColor`, `focusedColor`

### UILabel
- `atlas`
- `backgroundSprite`
- `prefix`
- `suffix`
- `text`
- `textColor`
- `autoSize`
- `autoHeight`
- `wordWrap`
- `textAlignment`
- `verticalAlignment`
- `padding`
- `tabSize`

### UIScrollablePanel
- `useScrollMomentum`
- `useTouchMouseScroll`
- `scrollWithArrowKeys`
- `freeScroll`
- `atlas`
- `backgroundSprite`
- `autoReset`
- `scrollPadding`
- `autoLayout`
- `wrapLayout`
- `autoLayoutDirection`
- `autoLayoutStart`
- `useCenter`
- `autoLayoutPadding`
- `scrollPosition`
- `scrollWheelAmount`
- `scrollWheelDirection`

### UIMultiStateButton
- See `1.7 Working with UIMultiStateButtons`

and many more. You can look up properties using ModTools or by opening `C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data\Managed\ColossalManaged.dll` with [ILSpy](http://ilspy.net/) and looking at the list of `ColossalManaged.UI` classes and their properties.

### 1.2 List of supported component types

So far the following types can be set through Sapphire:
- All built-in types - `int`, `uint`, `float`, `string`, `bool`, etc
- `UITextureAtlas`
- `Vector2`, `Vector3` and `Vector4`
- `Rect`
- `Color` and `Color32`
- Enums

### 1.3 Matching components with regular expressions

It's possible to select several components at once using regular expressions. Here is an example

```xml
<Component name=".*" name_regex="true">
  <textScale optional="true">1.2</textScale>
</Component>
```

This will select all components whose names match `.*` (i.e. all components) and set their `textScale` property (if it exists, note the `optional` attribute).
 
It is also possible to recursively match components at all hierarchy levels with the `recursive` attribute.
Example:
```xml
<Component name=".*" name_regex="true" recursive="true">
  <textScale optional="true">1.2</textScale>
</Component>
```

Will match all child components recursively and set their `textScale` to 1.2.

For learning regular expressions use one of the many guides on the internet or [specifically this one](http://regexone.com/).

### 1.4 Optional matching

It's possible to optionally match components and properties (meaning, don't produce an error if the component/ property doesn't exist, rather continue applying the skin). This is done using the `optional` attribute. Use `optional` if you are not certain that the component you are selecting will actually exist in the scene (e.g. a component created by a mod that may or may not be installed/ enabled).

For example:

```xml
<Component name="FooBar" optional="true">
  <color>Black</color>
</Component>
```

Will match component `FooBar` if it exists, and silently continue if it doesn't. The same thing is possible for properties:

```xml
<UIView>
  <Component name=".*" name_regex="true" recursive="true">
    <backgroundSprite optional="true">mySprite</backgroundSprite>
  </Component>
</UIView>
```
Will set the `backgroundSprite` of all components to `mySprite`, but only for components which actually have the `backgroundSprite` property to begin with.

### 1.5 Sticky properties

Sometimes you want some component property to "stick". Meaning that you want to keep the property at a certain value even if any code changes it after your skin is applied. This is very useful for `<zOrder>` where some menu operations may leave your UI components in a badly ordered state. In these cases you can use the `sticky` attribute to have the property get re-set every frame by the framework.
Example:

```xml
<Component name="MyComponent">
  <zOrder sticky="true">9999</zOrder>
</Component>
```

Forces `MyComponent` to be on top of all other components.

Note that while you can make each and every property `sticky` - it would be a **very bad idea** to do so. Sticky properties have a non-trivial performance- cost per frame, so use them sparingly!

### 1.6 Raw colors

It is possible to assign a color property directly without having to define a named colors.
Use the "raw" attribute to tell the parser the you are passing raw color values. Only the RGBA format is supported

Example
```xml
<Component name="FooBar">
  <textColor raw="true">255, 0, 255, 255</textColor>
</Component>
```
will set `FooBar`'s `textColor` to magenta.

### 1.7 Working with UIMultiStateButtons

There are several UI components which use the `UIMultiStateButton` class. This type of control represents a button with several different states - and therefore different normal, hovered, pressed, etc. sprites for each state. It is not possible to set these button's sprites the usual way (e.g. by setting `<normalBgSprite>` or `<hoveredFgSprite>`). Here is an example of how to work with multi-state buttons:

```xml
<Component name="Play"> <!-- note: "Play" is assumed to be a UIMultiStateButton -->
	<atlas>PlayButtonAtlas</atlas>
	
<!-- Use the </SpriteState> tag to define the sprites used for a specific state.
Index is the zero- based index of the state. Type is one of "foreground" or "background" -->
	<SpriteState index="0" type="foreground">
		<normal>ButtonPlay</normal>
		<hovered>ButtonPlayHovered</hovered>
		<focused>ButtonPlayFocused</focused>
		<pressed>ButtonPlayPressed</pressed>
	</SpriteState>
	
	<SpriteState index="0" type="background">
		<normal>ButtonTimeLeft</normal>
		<hovered>ButtonTimeLeftHovered</hovered>
		<focused>ButtonTimeLeft</focused>
		<pressed>ButtonTimeLeftPressed</pressed>
	</SpriteState>
	
	<SpriteState index="1" type="foreground">
		<normal>ButtonPause</normal>
		<hovered>ButtonPauseHovered</hovered>
		<focused>ButtonPauseFocused</focused>
		<pressed>ButtonPausePressed</pressed>
	</SpriteState>
	
	<SpriteState index="1" type="background">
		<normal>ButtonTimeLeft</normal>
		<hovered>ButtonTimeLeftHovered</hovered>
		<focused>ButtonTimeLeft</focused>
		<pressed>ButtonTimeLeftPressed</pressed>
	</SpriteState>
</Component>
```
