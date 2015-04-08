## Saphire - Skin module reference

This document defines the syntax of skin modules.

### Table of contents
- 1.0 Overview of a module
- 1.1 List of common component properties
- 1.2 List of supported component types
- 1.3 Matching components with regular expressions

### 1.0 Overview of a module

A skin module is any XML text file with correct Sapphire structure. The structure is described below.

Every skin module is enclosed in `<UIView>`/ `</UIView>` tags.
e.g.
```
<UIView>
  .. module contents ..
</UIView>
```

At the root of every module are one or more `<Component>` definitions. A `<Component>` can reference any UI component
such as panels, labels, sliders, scrollviews etc. Components are uniquely identified by their name and their place in the hierarchy.
For example, to define a `<Component>` with name `MyTestPanel` we'll do something like:
```
<UIView>
  <Component name="MyTestPanel">
    
  </Component>
</UIView>
```

Inside each `<Component>` tag you can specify one or more component properties to be changed. A descriptive list of the properties
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

#### Shared by all components

This is a list of properties that all components have:
- `isEnabled` - bool - example: `<isEnabled>true</isVisible>`
- `isVisible` - bool - example: `<isVisible>false</isVisible>`
- `size` - Vector2 - example: `<size>16.0, 12.0</size>`
- `relativePosition` - Vector3 - example: `<relativePosition>2.0, 2.0, 0.0</relativePosition>`

#### UIPanel
- `backgroundSprite` - Sprite - example: `<backgroundSprite>mySprite</backgroundSprite>`


### 1.2 List of supported component types

So far the following types can be set through Sapphire:
- All built-in types - `int`, `uint`, `float`, `string`, `bool`, etc
- `UITextureAtlas`
- `Vector2`, `Vector3` and `Vector4`
- `Rect`
- `Color` and `Color32`

### 1.3 Matching components with regular expressions

It's possible to match several components at once using regular expressions. Here is an example

```xml
<Component name=".*" name_regex="true">
  <textScale optional="true">1.2</textScale>
</Component>
```

This will match a component with any name (and hence all components at the specific hierarchy level) and set it's `textScale` property (if it exists, note the `optional` attribute).
 
It is also possible to recursively match components at all hierarchy level with the `recursive` attribute.
Example:
```xml
<Component name=".*" name_regex="true" recursive="true">
  <textScale optional="true">1.2</textScale>
</Component>
```

Will match all child components recursively and set their `textScale` to 1.2.

For learning regular expressions use one of the many guides on the internet or [specifically this one](http://regexone.com/).
