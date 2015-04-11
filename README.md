# Sapphire

Sapphire is a UI reskin framework/ mod for Cities: Skylines. It enables authors to create their own skins for the game using simple XML syntax and publish them as mods on the workshop. 

### Subscribe to Sapphire on [Steam Workshop](http://steamcommunity.com/sharedfiles/filedetails/?id=421770876)

## Before starting, install ModTools
### Download ModTools from - http://steamcommunity.com/sharedfiles/filedetails/?id=409520576
ModTools a vital part of the Sapphire workflow. It allows you to inspect the UI components in the scene and modify all their properties in real-time. Most of the time you will be looking up stuff in ModTools before adding it to your skin. For UI development it's best to turn off the 'Fields' checkbox in SceneExplorer's expanded menu. All UI objects are under the root `UIView` object.

## Contact me
Find me as `nlight` at `#skylines-modders` at `irc.esper.net` or message me on reddit `/u/nlight` if you have any questions, suggestions, feedback, want to show me what you've done, found bugs, feeling happy, feeling sad.. you get the idea :)

## Note: This document is `Work In Progress` and will be updated with time.

## Creating a new skin for Sapphire

Making a new skin for Sapphire is a straight-forward process. Here is an overview of the steps neccessary to create a skin:

- 1. Download the example skin and the template skin
- 2. Edit SapphireSkin.cs
- 3. Edit skin.xml
  - 3.1 Define your skin modules
  - 3.2 Define your skin sprites
  - 3.3 Define your skin colors
- 4. Writing your first skin module
- 5. Basic workflow
- 6. Publishing your skin on Steam Workshop
- 7. Additional resources

### 1. Download and extract the example and template skins

**1.1** Download and extract [the Sapphire master archive](https://github.com/AlexanderDzhoganov/Skylines-Sapphire/archive/master.zip)

**1.2** Navigate to the `Skins/Emerald` folder. 

***This is a fully-working Sapphire skin that you can test out and modify to learn how the system works.***. 

Copy the contents of this folder to your C:S mods folder.

The mods folder is located in differnt places depending on your OS, here is a list to help:
- Windows - `C:\Users\<YOUR USERNAME>\AppData\Local\Colossal Order\Cities_Skylines\Addons\Mods\`
- Linux - `~/.local/share/Colossal Order/Cities_Skylines/Addons/Mods`
- OSX - `/users/library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods`

Note the folder structure necessary for this to work. Double check that you have the same structure locally.

```
Mods/
	Emerald/
		_SapphireSkin/
			skin.xml
			Modules/
			Sprites/
		Source/
			Source.cs
```

After the last step the skin should be visible in the Sapphire skins list in-game.

After you're done with the example skin go to 1.3.

**1.3** When you're ready to make your own skin, nagivate to the `Skins/TemplateSkin` folder of the master archive.

**1.4** Create a new Cities: Skylines mod by creating a new folder for it in the appropriate location e.g. for windows users that would be `C:\Users\<YourUser>\AppData\Local\Colossal Order\Cities_Skylines\Addons\Mods\<YourSkinName>`

**1.5** Copy the contents of `Skins/Template` to your new folder. 

**1.6** At this point your mod's root folder should contain a folder named `Source` and a folder named `_SapphireSkin`. 

### 2. Edit SapphireSkin.cs

**2.1** Open Source/SapphireSkin.cs and put your skin's name and description where it says `YourSkinName` and `YourSkinDescription`. Make sure to *not* remove the quotes `"` around them.

### 3. Edit skin.xml

Note on comments: You can use XML comments `<!-- this is a comment -->` to comment out parts of your XML.

**3.0** Open _SapphireSkin/skin.xml and edit the first line with your skin name and author name
`<SapphireSkin name="<YOUR SKIN NAME HERE>" author="<YOUR NAME HERE>">`

**3.1** Next define all your skin modules using the syntax below. Skin modules are XML files which define how your skin looks. You can have as many of them as you like. Make sure to utilize modules to split your skin cleverly for easier editing. All modules are loaded and applied in the order defined in skin.xml.

To define a module, add a line like:
```xml
<Module class="MainMenu">Modules/MainMenu.xml</Module>
```
to your skin definition.

The `class` attribute specifies which part of the game's UI your module modifies. Possible values are `MainMenu`, `InGame`, `MapEditor` and `AssetEditor`. Make sure to change it accordingly. The value inside the Module definition is the relative path to your module XML.

**3.2** Next you're going to define the sprites your skin uses. You only need to define new sprites if you want to replace any of the game's default ones. Sprites exist inside atlases. You can define any amount of atlases you want. Each atlas is 2048x2048 pixels in size, so you will need to create separate atlases if you have many sprites.
*Note that each UI component can only have one atlas assigned at a time, which means that sprites used for the same component must be in the same atlas.*

To define a new atlas in your `skin.xml` use the following syntax:

```xml
<SpriteAtlas name="ExampleAtlas1">
```

**3.2.1** Next define any sprites that you wish to include in your atlas. 

```xml
<Sprite name="ExampleSprite1">Sprites/Example.png</Sprite>
```

Here, `ExampleSprite1` is the name of the sprite which you'll use to refer to it from skin modules and `Sprites/Example.png` is the path to the .png image relative to the location of skin.xml.

**3.2.2** Don't forget to close the tag

```xml
</SpriteAtlas>
```

**3.3** Define the colors of your skin

Sapphire allows you to define named colors that you can use from anywhere in your skin. They are defined in `skin.xml` like this:

```xml
<Colors>
	<Color name="ExampleHEXColor">#FFFFFF</Color>
	<Color name="ExampleRGBColor">81, 97, 149, 255</Color>
</Colors>
```
You can use either RGBA notation or HTML hex notation for your colors. 

### 4. Writing a skin module

#### Note: For detailed information on writing skin modules please [refer to this document](https://github.com/AlexanderDzhoganov/Skylines-Sapphire/blob/master/MODULE_REFERENCE.md)

Skin modules are the core of your skin. They define the UI components and properties that your skin modifies. You can modify any property of any UIComponent in the hierarchy.

**4.1** The <UIView> tag

All skin modules are contained within `<UIView>` and `</UIView>` tags.

Note: You can only have one `<UIView>` tag per module, but as many `<Component>` selectors as you wish.

The UIView tag defines the root of the UI hierarchy (the in-game `UIView` object). Inside it you will define <Component> selectors and set their respective properties. This all sounds very confusing at first so here is an example:

```xml
<UIView>
	<Component name="(Library) OptionsPanel">
		<size>512.0, 512.0</size>
		<relativePosition>16.0, 4.0, 0.0</relativePosition>
	</Component>
</UIView>
```

Here is how this works - when this skin module gets applied, Sapphire will find the UIComponent called `(Library) OptionsPanel` (repesenting the Options menu in the main menu) under the UIView root and change its size to x=512, y=512 and its position to x=16 and y=4. 

We call `<Component name="Foo">` a selector. Selectors allow you to "grab" a specific component in the UI and modify its properties. There are several different kinds of selectors in Sapphire, refer to the skin module guide for more information.

Due to the hierachical nature of the UI, Sapphire allows you to nest Component selectors:

```xml
<UIView>
	<Component name="TSBar">
		<size>1920.0, 49.0</size>
		<relativePosition>-5.0, 1031.0, 0.0</relativePosition>
		<color>MyPredefinedColor</color>
		
		<Component name="Sprite">
			<isVisible>false</isVisible>
		</Component>
	</Component>
</UIView>
```

Assuming that the component named `TSBar` exists and it has a child component named `Sprite` the module above will change the size and position of `TSBar` as well as hide (isVisible = false) its `Sprite` child.

**6.1** Testing changes to your skin

Sapphire is designed so that it is very easy and painless to make changes to your skin and test them out live.
- Start C:S and you should see your new skin in the Sapphire skins list.
- Enable your skin and use the 'Reload active skin' button to reload any changes live.

This method allows you to work on your skin without restarting the game. Using the reload button will reload any and all XML and PNG files associated with your skin.

**The ModTools console (opened by pressing F7) is very useful in pinpointing any errors in your XML, as it will print out descriptive error messages when you try to re-load your skin. Make sure that "Use ModTools Console", "Hook Unity's logging" and "Log exceptions to console" are all checked in the ModTools main menu (Ctrl+Q).**

### 5. Basic workflow

At this point you may be familiar with the basics but are still wondering how the basic workflow would look like. Here is an example workflow that you can use and tweak to your liking:

For every component in the UI that you want to re-design in some way:

* Enable "Developer mode" from the checkbox in Sapphire's panel. This mode will paint the boundaries of all UI components as well as allow you to see the properties of a specific component by hovering over it.

* Write down the component's `name` and `parent` properties

* Open ModTools "Scene Explorer" and click on the down-arrow button in the top-left to open the expanded settings panel. **Disable the `Fields` checkbox on the top row.** 

* Type the `name` of your component into the `GameObject.Find` field in Scene Explorer's expanded settings panel and click "Find".

* If the component was found you will see it displayed in the left panel of the SceneExplorer along with an `>` icon to the left of the component's name. Click on `>` to open the component for editing.

* Now you will be able to see all the component's properties on the right side of Scene Explorer's window.

*  It is very necessary to know where a component exists within the whole UI hierarchy (so that you can write the proper Sapphire <Component> selectors). Sometimes this is not trivial as components may be deeply nested within the hierarchy and finding them through browsing with the SceneExplorer may prove tedious. 

ModTools can solve this problem by providing a way to query the full path of a UI component. This is done through the ModTools console. Press F7 to open the console, and in the command- line field on the bottom type the following:

```
GameObjectUtil.Find(GameObject.Find("myUIComponent"));
```

where `myUIComponent` is the name of the UI component you're looking for (the same name you typed into GameObject.Find in SceneExplorer in step 4). If you get a NullReferenceException (red text) that means a component with this name does not exist in the scene.

* Modify any properties using the Scene Explorer you want to achieve your desired look. Write down all changes in your skin's XML.

* Reload your skin to preview your changes. Note that recent Sapphire versions can reload your skin automatically when you modify one of its files ("Reload active skin on file change" checkbox in Sapphire's panel)

* Repeat the process above for all UI components that you wish to modify/ redesign

### 6. Publishing your skin

**Warning: Do not bundle Sapphire.dll with your mod! You must put a link to Sapphire in your mod's description and tell your users to subscribe to it. This ensures all users get bug fixes and updates without you having to update your skin.**

You can publish your skin on Steam's Workshop like any other code mod. Go to Content -> Mods in C:S and use the 'Share' button on your skin's entry in the list. Users who subscribe to your skin will automatically have it visible in their Sapphire skins list. If you want to update your skin, you can do it like other code mods - delete the mod's folder from AppData, subscribe to it in the workshop and then edit it from its workshop folder and use the 'Update' button in-game.

### 7. Additional resources

- [Module reference](https://github.com/AlexanderDzhoganov/Skylines-Sapphire/blob/master/MODULE_REFERENCE.md) - thoroughly describes the syntax of skin modules. Look here for advanced usage instructions.
- [FAQ](https://github.com/AlexanderDzhoganov/Skylines-Sapphire/blob/master/FREQUENTLY_ASKED_QUESTIONS.md)
- [Examples](https://github.com/AlexanderDzhoganov/Skylines-Sapphire/blob/master/EXAMPLES.md)
