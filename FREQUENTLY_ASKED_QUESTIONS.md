### 1. My skin is broken on different aspect ratios. How do I fix it?

If your skin is changing/ breaking on different aspect ratios that means you haven't set the proper `<anchor>` tags for your UI components. Make sure that each component is anchored where it should be e.g. the default "Esc" button is on the top-right of the screen and its anchor tag is `<anchor>Top|Right</anchor>`. By using proper `<anchor>`s you can make your skin work on any aspect ratio/ resolution.

### 2. How to find a UI component within the hierarchy if I know only its name?

Open the ModTools console by pressing F7 and type:
```
GameObjectUtil.WhereIs(GameObject.Find("FooBar"));
```

replace `FooBar` with the name of the component you're looking for. Press `enter` to submit the command- line and you should see something like:
```
UIView.SomePanel.SomethingElse.FooBar
```
where each `.` represents a level in the hierarchy i.e. `SomePanel` is a child of the root UIView, `SomethingElse` is a child of `SomePanel` and `FooBar` is a child of `SomethingElse`. 

*Note: if you don't see the above output but instead a big red `NullReferenceException` that means a component with this name was not found in the scene.*

You can then write a Sapphire selector for this component which would look like:

```xml
<UIView>

..
  
  <Component name="SomePanel">
    <Component name="SomethingElse">
      <Component name="FooBar">
        <size>128.0, 256.0</size>
      </Component>
    </Component>
  </Component>
  
..

</UIView>
```

The example above will set the size of `FooBar` to 128, 256.

### 3. How can I create new UI components using Sapphire syntax? Why don't you make it possible to do so?

Creating new components using similar syntax to the current one is possible but left out as a design decision. The reason behind that is it will introduce a huge amount of complexity e.g. you add a button - but now you need a way to code a behaviour for it (as a button that does nothing is absolutely useless). In practice, introducing this to the Sapphire syntax will gradually turn it into a full-blown programming language. This is very undesirable as C# is already a much better programming language than any XML- based syntax could ever be and it also goes in an orthogonal direction to Sapphire's goal of allowing "skinning" not designing of UIs. For the proper way to handle creation of new elements see (4).

### 4. Can I change the behaviour of UI components and/ or re-parent them? Can I bundle code with my Sapphire skin?

Yes! Absolutely. You can include code with your Sapphire skin by putting it in the "Source/" folder of your mod's root directory or by compiling a .DLL yourself (by e.g. Visual Studio or MonoDevelop) and putting it in your mod's root folder. This allows extreme changes to the game's interface up to a 100% complete rewrite. Any component that you create via your code is skinnable with Sapphire so you should not bother with setting up any properties through code - just create the component, name it and then skin it using a Sapphire skin module.

### 5. Two or more components which share the same parent have identical names. How can I select only one of them?

This is mostly the case with modders who don't name their UI components so they remain with the default names like "UIButton" or "UILabel. When several such "UIButton" components have the same parent it is impossible to select one using name matching. This is where the `hash` attribute comes in. Each component has a hash value which you can see by hovering over it in 'developer mode'. This hash value can be used to differentiate between two or more components which have the same name. Here is an example:

```xml
<Component name="UIButton" hash="1B0CDBFE9" optional="true">
	<relativePosition>1700.0, 1012.0</relativePosition>
</Component>

<Component name="UIButton" hash="180411189" optional="true">
	<relativePosition>1700.0, 1044.0</relativePosition>
</Component>
```

This specific example comes from the "No Pillars" mod which creates two "unnamed" buttons with the same parent. By looking up the hash values in 'developer mode' and specifying them in the `hash` attribute we can select only one of the two buttons. This works for any number of components sharing the same name.

**Warning: Component hashes are not globally unique! They are to be used only to select between components which _share the same parent_. The `name` attribute is still mandatory**

### 6. Where can I find the vanilla UI sprites?

There is a dump of all sprites that are in the vanilla game here - http://docs.skylinesmodding.com/en/master/resources/UI-Sprites-1.html
