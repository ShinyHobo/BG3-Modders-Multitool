# BG3 Modder's Multitool
Utility for quickly generating Baldur's Gate 3 mod packs from an unpacked workspace.

To use:
- Run bg3-mod-packer.exe  
- Select the location of your LSLib 1.15.2 (or higher) divine.exe  
- Drag and drop your unpacked workspace into the large blue box to generate an info.json file and .pak with the same name as your workspace.  

Extras:
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

![](https://imgur.com/ZkNE25B)

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

Upon working correctly, you will get a .zip of the same name as your mod directory immediately next to it. It is compatible with ShadowChild's [Candor Mod Manager](https://github.com/ShadowChild/BaldursGate3/releases)

Baldur's Gate 3 Modder's Multitool is unofficial fan content, not approved/endorsed by Larian Studios. Portions of the materials used are property of Wizards of the Coast LLC and Larian Studios Games ltd.
