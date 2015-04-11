## Example 1 - Completely replacing the sprites for the "Roads" button on the main toolstrip

1. In your skin.xml

```xml
<SpriteAtlas name="IconsAtlas">
  ..
  <Sprite name="RoadsNormal">Sprites/RoadsNormal.png</Sprite>
  <Sprite name="RoadsHovered">Sprites/RoadsHovered.png</Sprite>
  <Sprite name="RoadsFocused">Sprites/RoadsFocused.png</Sprite>
  <Sprite name="RoadsPressed">Sprites/RoadsPressed.png</Sprite>
  <Sprite name="RoadsDisabled">Sprites/RoadsDisabled.png</Sprite>
  
  <Sprite name="RoadsBgNormal">Sprites/RoadsBgNormal.png</Sprite>
  <Sprite name="RoadsBgHovered">Sprites/RoadsBgHovered.png</Sprite>
  <Sprite name="RoadsBgFocused">Sprites/RoadsBgFocused.png</Sprite>
  <Sprite name="RoadsBgPressed">Sprites/RoadsBgPressed.png</Sprite>
  <Sprite name="RoadsBgDisabled">Sprites/RoadsBgDisabled.png</Sprite>
</SpriteAtlas>

```

2. In your skin module:

```xml
<Component name="TSBar">

  ..

  <Component name="MainToolstrip">
    <Component name="Roads">
      <atlas>IconsAtlas</atlas>
      
      <!-- these are the foreground sprites -->
      <normalFgSprite>RoadsNormal</normalFgSprite>
      <hoveredFgSprite>RoadsHovered<hoveredFgSprite>
      <focusedFgSprite>RoadsFocused<focusedFgSprite>
      <pressedFgSprite>RoadsPressed</pressedFgSprite>
      <disabledFgSprite>RoadsDisabled</disabledFgSprite>
      
      <!-- these are the background sprites -->
      <normalBgSprite>RoadsBgNormal</normalBgSprite>
      <hoveredBgSprite>RoadsBgHovered<hoveredBgSprite>
      <focusedBgSprite>RoadsBgFocused<focusedBgSprite>
      <pressedBgSprite>RoadsBgPressed</pressedBgSprite>
      <disabledBgSprite>RoadsBgDisabled</disabledBgSprite>
      
    </Component>
    
    ..
  </Component>
 
</Component>
```
