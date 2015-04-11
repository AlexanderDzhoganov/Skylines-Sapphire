### 1. How to select all components in the scene?

- Use `<Component name=".*" name_regex="true" recursive="true">`

For example the XML below will hide all UI components:

```
<Component name=".*" name_regex="true" recursive="true">
  <isVisible>false</isVisible>
</Component>

```

### 2. My skin is broken on different aspect ratios?

You haven't set the proper `<anchor>` tags for your UI components. Make sure that each component is anchored where it should be e.g. the default "Esc" button is on the top-right of the screen and its anchor tag is `<anchor>Top|Right</anchor>`. By using proper `<anchor>`s you can make your skin work on any aspect ratio/ resolution.

### 3. How to find a UI component within the hierarchy if I know only its name?

Open the ModTools console by pressing F7 and type:
```
GameObjectUtil.WhereIs(GameObject.Find("FooBar"));
```

replace `FooBar` with the name of the component you're looking for. Press `enter` to submit the command- line.
