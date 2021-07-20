# BG3 Modder's Multitool
Utility for quickly generating Baldur's Gate 3 mod packs from an unpacked workspace.

#### Features
- Automatically paks, zips, and generates metadata  
- Automatically converts files named like example.lsf.lsx and example.lsb.lsx to example.lsf and example.lsb, respectively. This means you no longer have to manually convert files and then copy them into their respective directories.  
- Supports dependencies and multipaks (multiple mods in the same workspace, and multiple workspaces)  
- Utility for extracting some/all game assets at once. It can take more than an hour for the complete set, but will place all the extracted files in a folder in the same directory as the application  
- Unpacked file indexer  
- Index search functionality  
- File previews  
- Open (and automatically convert) files from index searcher  
- UUID and TranslatedString handle generator  
- Launch game directly  
- GameObject Explorer (stats, attributes, icon, model, and model files)  
- Mass LSX converter  
- Colada model conversion  

![Main window](https://i.imgur.com/ZkNE25B.png)

#### Indexing/Searching
By clicking on the "Index" button, you will be able to create an index of all unpacked .pak files you have in your UnpackedData directory. Double-click the Index button to begin indexing. After indexing, open an Index Search window, type your keywords into the search bar, and hit Enter (or "Search Files") to get results within a matter of seconds. Double click a listing to open it; hover for a file preview (matching lines only).

![Indexing and searching, multiple simultaneous windows possible](https://i.imgur.com/fTID9zq.png)

Don't hit the "Index Files" button more than once unless you want to reset your index and start from scratch. The old index has to be completely deleted to generate a new one. Unpacking all files and indexing them should result in an index around 450 MB, and will take roughly 10 minutes to generate. Audio, video, image, and model files do not have their contents indexed, only their file names.

####GameObject Exploration
Clicking the GameObject Explorer button will open a new window allowing to look at GameObjects such as characters and weapons in a more human friendly manner. As long as you have the necessary game .paks unpacked, it will automatically generate connections between things such as stats, icons, and translations in a hierarchal format. It is possible to search for GameObjects on multiple fields as well as within individual objects through the Stats tab property grid.

![GameObject Stats](https://i.imgur.com/3LvsDtE.png)
![GameObject attributes](https://i.imgur.com/T49A0Ox.png)

The model viewer tab allows you to view the model associated with a given object (characters, items, scenery, and TileConstructions). This process automatically converts the .GR2 file to a .dae collada file in the same directory. The Model Files tab will list the files used to generate the model; hovering over the name of a model will provide you with the full pak file path and clicking it will open both the containing folder to the file as well as the default program associated .dae files on your system ie. Notepad++.

![GameObject render](https://i.imgur.com/fJOHBVE.png)

#### Making mod paks
- Run bg3-mod-packer.exe  
- Select the location of your [LSLib 1.15.8 (or higher)](https://github.com/Norbyte/lslib) divine.exe  
- Drag and drop your unpacked workspace into the large blue box to generate an info.json file and .pak with the same name as your workspace.

[Padme4000](https://github.com/Padme4000) has created a tutorial [here](https://www.youtube.com/watch?v=frgJdEibMNA) on how to use this tool to create mods in more depth.

[![YT Tutorial](https://img.youtube.com/vi/frgJdEibMNA/0.jpg)](https://www.youtube.com/watch?v=frgJdEibMNA)



meta.lsx template:
```
<?xml version="1.0" encoding="UTF-8"?>
<save>
    <version major="4" minor="0" revision="0" build="49"/>
    <region id="Config">
        <node id="root">
            <children>
                <node id="Dependencies"/>
                <node id="ModuleInfo">
                    <attribute id="Author" type="LSWString" value="ModderName"/>
                    <attribute id="CharacterCreationLevelName" type="FixedString" value=""/>
                    <attribute id="Description" type="LSWString" value="Some description text"/>
                    <attribute id="Folder" type="LSWString" value="ModFolder"/>
                    <attribute id="GMTemplate" type="FixedString" value=""/>
                    <attribute id="LobbyLevelName" type="FixedString" value=""/>
                    <attribute id="MD5" type="LSString" value=""/>
                    <attribute id="MainMenuBackgroundVideo" type="FixedString" value=""/>
                    <attribute id="MenuLevelName" type="FixedString" value=""/>
                    <attribute id="Name" type="FixedString" value="Mod Name"/>
                    <attribute id="NumPlayers" type="uint8" value="4"/>
                    <attribute id="PhotoBooth" type="FixedString" value=""/>
                    <attribute id="StartupLevelName" type="FixedString" value=""/>
                    <attribute id="Tags" type="LSWString" value=""/>
                    <attribute id="Type" type="FixedString" value="Add-on"/>
                    <!-- Get new UUID from https://www.uuidgenerator.net/ -->
		    <attribute id="UUID" type="FixedString" value="00000000-0000-0000-0000-000000000000"/>
		    <attribute id="Version" type="int32" value="1"/>
                    <children>
                        <node id="PublishVersion">
                            <attribute id="Version" type="int32" value="268435456"/>
                        </node>
                        <node id="Scripts"/>
                        <node id="TargetModes">
                            <children>
                                <node id="Target">
                                    <attribute id="Object" type="FixedString" value="Story"/>
                                </node>
                            </children>
                        </node>
                    </children>
                </node>
            </children>
        </node>
    </region>
</save>

```

Correct folder structure of mod workspace:

```
ModFolder
|->Mods
  |->ModFolder
    |->meta.lsx
|->Public
  |->ModFolder
    |->Folder1
      |->File.lsx
    |->Folder2
      |->File.lsb
    |->Folder3
      |->File.lsf
```

Playable Minotaur example:
![Minotaur workspace](https://i.imgur.com/nz0SIMd.png)

If you do everything correctly, you will get a .zip of the same name as your mod directory immediately next to it, containing your pak(s) and info.json metadata file. It is compatible with [ShadowChild](https://github.com/ShadowChild)'s [Candor Mod Manager](https://github.com/ShadowChild/BaldursGate3/releases)

### Baldur's Gate 3 Modder's Multitool is unofficial fan content, not approved/endorsed by Larian Studios. Portions of the materials used are property of Wizards of the Coast LLC and Larian Studios Games ltd.
