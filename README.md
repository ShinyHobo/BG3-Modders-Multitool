# bg3-mod-packer
Utility for quickly generating Baldur's Gate 3 mod packs from an unpacked workspace.

To use:
* Simply drop the bg3-mod-packer.bat file into your LSLib root directory where it can access divine.exe. 
* Drag and drop your unpacked workspace into the .bat to generate an info.json file and .pak with the same name as your workspace. 
  * The workspace name should be the same as your modpack name.

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

Upon working correctly, you will get a .zip of the same name as your mod directory immediately next to it. It is compatible with ShadowChild's <a href='https://github.com/ShadowChild/BaldursGate3/releases'>Candor Mod Manager</a>
