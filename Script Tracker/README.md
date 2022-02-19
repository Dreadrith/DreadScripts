# Script Tracker

<a href="https://github.com/Dreadrith/DreadScripts/releases/download/Scripts/ScriptTracker.unitypackage">Download Here</a>  
Found in DreadTools > Scripts Settings > Script Tracker

![Flag](https://cdn.discordapp.com/attachments/898498998527197224/898544716478574722/unknown.png)

![Settings](https://cdn.discordapp.com/attachments/898498998527197224/944407784546512906/scriptSettings.png)

This script is for security purposes. It will attempt to protect you against scripts that have the potential to be malicious.
 For any imported Script, this script will read it preemptively and warn you if it contains any "Keywords" defined in the script's settings. 
 When warned, you are prompted to Allow, Delete or Revise the file. Revising the file turns it into a txt file, which will stop it from running and allow you to open it to read it.

You can also make it prompt for all Scripts, in case you want to import a package without worrying about what Scripts it contains.
This will also prompt for imported DLLs, the script cannot read the DLL so it will prompt for all or none of them, and you can only allow or delete them.
Once imported, it is recommended to open DreadTools > Scripts Settings > Script Tracker then pressing "Allow Current Project Scripts and DLLs".

**This is not foolproof!**  
It is still possible to cause damage through other script means and this may not cover all possibilities, so be cautious of what you're importing!
