### 1. How to select all components in the scene?

- Use `<Component name=".*" name_regex="true" recursive="true">`

For example the XML below will hide all UI components:

```
<Component name=".*" name_regex="true" recursive="true">
  <isVisible>false</isVisible>
</Component>

```
