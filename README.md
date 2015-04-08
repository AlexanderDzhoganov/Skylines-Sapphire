# Skylines-Sapphire

Sapphire is a UI reskin framework/ mod for Cities: Skylines. It enables authors to create their own skins for the game using straight-forward XML syntax and publish them as mods on the workshop. 

## Before starting, install ModTools
### Download ModTools from - http://steamcommunity.com/sharedfiles/filedetails/?id=409520576
ModTools a very important part of the Sapphire workflow. It allows you to inspect the UI components in the scene and modify all their properties in real-time. Most of the time you will be looking up stuff in ModTools before adding it to your skin. For UI development it's best to turn off the 'Fields' checkbox in SceneExplorer's expanded menu. All UI objects are under the root `UIView` object.

## Creating a new skin for Sapphire

Making a new skin for Sapphire is a simple and straightforward process. Here is an overview of the steps neccessary to create a skin:

1. Download the template skin
2. Edit SapphireSkin.cs
3. Edit skin.xml
3.1 Define your skin modules
3.2 Define your skin sprites
3.3 Define your skin colors
4. Write a skin module
5. Test and publish your skin

### 1. Download and extract the template skin

#### 1.1 Download the Sapphire master archive from [here](https://github.com/AlexanderDzhoganov/Skylines-Sapphire/archive/master.zip)
#### 1.2 Extract it somewhere and navigate to the `Skins/TemplateSkin` folder.
#### 1.3 Create a new Cities: Skylines mod by creating a new folder for it in the appropriate location e.g. for windows users that would be `C:\Users\<YourUser>\AppData\Local\Colossal Order\Cities_Skylines\Addons\Mods\<YourSkinName>`
#### 1.4 Copy the contents of `Skins/Template` to your new folder. 
#### 1.5 At this point your mod's root folder should contain a folder named `Source` and a folder named `_SapphireSkin`. 

### 2. Edit SapphireSkin.cs
#### 2.1 Open Source/SapphireSkin.cs and put your skin's name and description where it says `<YourSkinName>` and `<YourSkinDescription>`. Make sure *not* to remove the quotes ("") around them.

### 3. Edit skin.xml
#### 3.0 Open _SapphireSkin/skin.xml and edit the first line with your skin name and author name
`<SapphireSkin name="<YOUR SKIN NAME HERE>" author="<YOUR NAME HERE>">`

#### 3.1 Next define all your skin modules using the syntax below. Skin modules are XML files which define how your skin looks. You can have as many of them as you like. Make sure to utilize modules to split your skin cleverly for easier editing. All modules are loaded and applied in the order defined in skin.xml.
To define a module, add a line like:
```
<Module class="MainMenu">Modules/MainMenu.xml</Module>
```
to your skin definition.
The `class` attribute specifies which part of the game's UI your module modifies. Possible values are `MainMenu`, `InGame`, `MapEditor` and `AssetEditor`. Make sure to change it accordingly. The value inside the Module definition is the relative path to your module XML.

#### 3.2 Next you're going to define the sprites your skin uses. You only need to define new sprites if you want to replace any of the game's default ones. Sprites exist inside atlasses. You can define any amount of atlasses you want. Each atlass is 2048x2048 pixels in size, so you will need to create separate atlasses if you have many sprites.
To define a new atlas in your `skin.xml` use the following syntax:
```
<SpriteAtlas name="ExampleAtlas1">
```

#### 3.2.1 Next define any sprites that you wish to include in your atlas
```
<Sprite name="ExampleSprite1" width="30" height="30">Sprites/Example.png</Sprite>
```
*Make sure to put in the correct "width" and "height" of your PNG image.*

#### 3.2.2 And close the tag
```
</SpriteAtlas>
```

#### 3.3 Define the colors of your skin
Sapphire allows you to define named colors that you can use from anywhere in your skin. They are defined in `skin.xml` like this:
```
<Colors>
		<Color name="ExampleHEXColor">#FFFFFF</Color>
		<Color name="ExampleRGBColor">81, 97, 149, 255</Color>
	</Colors>
```
You can use either RGBA notation or HTML hex notation for your colors. 

### 4. Writing a skin module

Skin modules are the core of your skin. They define the UI components and properties that your skin modifies. You can modify any property of any UIComponent in the hierarchy.

#### 4.1 The <UIView> tag

All skin modules are contained within `<UIView>` and `</UIView>` tags. These tags define the root of the UI hierarchy (the in-game `UIView` object). Inside these tags you will define <Component> tags with their respective properties. If it all seems confusing, here is an example:

```
<UIView>
	<Component name="(Library) OptionsPanel">
		<size>512.0, 512.0</size>
		<relativePosition>16.0, 4.0, 0.0</relativePosition>
	</Component>
</UIView>
```

So here is how this works - when this module gets applied, Sapphire will find the UIComponent called `(Library) OptionsPanel` (repesenting the Options menu in the main menu) under the UI root and change it's size to x=512, y=512 and it's position to x=16 and y=4. 
