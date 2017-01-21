using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Text.RegularExpressions;
 
public static class PostBuildTrigger
{
 	private static string buildPath;
	//private static string buildName;
 	
  	//private static DirectoryInfo projectRoot;
	
	private static char divider;
	
	const string iphoneInfoFile = "Info.plist";

	const string iphoneOverwriteSrcDir = "overwrite";

	const string iphoneOverwriteDest= "Classes";
 
	[PostProcessBuild(198)] 
	public static void OnPostProcessBuildFirst(BuildTarget target, string path)
	{
		// Get Required Paths
		buildPath 	= path;
		if(target.CompareTo(BuildTarget.iOS) == 0)
		{
			RenameXCodeProject();
		}
        //projectRoot = Directory.GetParent(Application.dataPath);
        //buildName 	= Path.GetFileNameWithoutExtension(path);
	}
	
    /// Processbuild Function
    [PostProcessBuild(199)] // <- this is where the magic happens
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
       	if(target.CompareTo(BuildTarget.iOS) == 0)
		{
			//iphoneUpdateInfoPlist();
			iphoneOverwriteUnityGenFiles();
			//iphoneUpdateOptimizationLevel();
			iphoneInjectCustomMonoExcetionReporter();
			iphoneCopyHelpshiftFiles();
			iphoneEnableHealthKit();
			//iphoneAddFabricFramework();
			//iphoneCopyFrameworksFiles();
			//iphoneUpdateFraworks();
			//addIOSResources();
		}
    }
	private static void RenameXCodeProject()
	{
		//if(target.CompareTo(BuildTarget.iPhone) == 0)
		{
			string xcAssetsFolder = buildPath + Path.DirectorySeparatorChar + "Unity-iPhone";// + Path.DirectorySeparatorChar + "project.pbxproj";
			string newXcAssetsFolder = buildPath + Path.DirectorySeparatorChar + PlayerSettings.productName;
			if(Directory.Exists(xcAssetsFolder)){
				Directory.Move(xcAssetsFolder,newXcAssetsFolder);
				string testsFolder = xcAssetsFolder+" Tests";
				string newTestsFolder = newXcAssetsFolder+" Tests";
				if(Directory.Exists(testsFolder))
					Directory.Move(testsFolder,newTestsFolder);
			}
			
			string projectFolder = buildPath + Path.DirectorySeparatorChar + "Unity-iPhone.xcodeproj";// + Path.DirectorySeparatorChar + "project.pbxproj";
			string newProjectFolder = buildPath + Path.DirectorySeparatorChar + PlayerSettings.productName+".xcodeproj";
			if(Directory.Exists(projectFolder))
			{
				Directory.Move(projectFolder,newProjectFolder);
			}
			string pbxFile = newProjectFolder+Path.DirectorySeparatorChar+"project.pbxproj";
			string txtContent= File.ReadAllText(pbxFile);
			txtContent = txtContent.Replace("Unity-iPhone",PlayerSettings.productName);
			File.WriteAllText(pbxFile,txtContent);

			string xcSharedSchemeFolder = buildPath + Path.DirectorySeparatorChar + PlayerSettings.productName+".xcodeproj"+Path.DirectorySeparatorChar+"xcshareddata"+Path.DirectorySeparatorChar+"xcschemes";
			string scSharedSchemeFile = xcSharedSchemeFolder +Path.DirectorySeparatorChar+"Unity-iPhone.xcscheme";
			string newSharedSchemeFile= xcSharedSchemeFolder +Path.DirectorySeparatorChar+PlayerSettings.productName+".xcscheme";
			if(Directory.Exists(xcSharedSchemeFolder))
			{
				File.Move(scSharedSchemeFile,newSharedSchemeFile);
				//string schemeNamagementFile= scSharedSchemeFile +Path.PathSeparator+"xcschememanagement.plist";
				string content = File.ReadAllText(newSharedSchemeFile);
				content = content.Replace("Unity-iPhone",PlayerSettings.productName);
				File.WriteAllText(newSharedSchemeFile,content);
			}else
				Debug.LogError("Can't find scheme folder:"+xcSharedSchemeFolder);
		}
	}
 
//	private static void iphoneUpdateInfoPlist()
//	{
//		string infoFilePath = buildPath + Path.DirectorySeparatorChar + iphoneInfoFile;
//		if (File.Exists(infoFilePath)) 
//		{
//			string elementsToAdd =
//					"<key>CFBundleURLTypes</key>" +
//						"<array>" +
//							"<dict>" +
//								"<key>CFBundleURLSchemes</key>" +
//						  			  "<array>" +
//									  		"<string>fb224835454235725</string>" +
//									  "</array>" +
//							"</dict>" +
//						"</array> " +
//					"<key>NSLocationAlwaysUsageDescription</key>" +
//					"<string>Fit Ops needs your location to track your everyday activity.</string>" +
//					"<key>NSLocationWhenInUseUsageDescription</key>" +
//					"<string>Fit Ops needs your location to track your everyday activity.</string>" +
//					"<key>UIBackgroundModes</key>" +
//						"<array>" +
//							//"<string>bluetooth-central</string>" +
//							"<string>location</string>" +
//						"</array>" +
//					"<key>NSAppTransportSecurity</key>" +
//					"<dict>" +
//						"<key>NSExceptionDomains</key>" +
//						"<dict>" +
//        					"<key>facebook.com</key>" +
//        					"<dict>" +
//            					"<key>NSIncludesSubdomains</key>" +
//            					"<true/>" +
//            					"<key>NSThirdPartyExceptionRequiresForwardSecrecy</key>" +
//            					"<false/>" +
//        					"</dict>" +
//        					"<key>fbcdn.net</key>" +
//        					"<dict>" +
//            					"<key>NSIncludesSubdomains</key>" +
//            					"<true/>" +
//            					"<key>NSThirdPartyExceptionRequiresForwardSecrecy</key>" +
//            					"<false/>" +
//        					"</dict>" +
//        					"<key>akamaihd.net</key>" +
//        					"<dict>" +
//            					"<key>NSIncludesSubdomains</key>" +
//            					"<true/>" +
//            					"<key>NSThirdPartyExceptionRequiresForwardSecrecy</key>" +
//            					"<false/>" +
//        					"</dict>" +
//    					"</dict>" +
//						"<key>NSAllowsArbitraryLoads</key>" +
//						"<true/>" +
//					"</dict>"+
//					"<key>LSApplicationQueriesSchemes</key>" +
//					"<array>" +
//						"<string>fbapi</string>" +
//						"<string>fbapi20130214</string>" +
//						"<string>fbapi20130410</string>" +
//						"<string>fbapi20130702</string>" +
//						"<string>fbapi20131010</string>" +
//						"<string>fbapi20131219</string>" +
//						"<string>fbapi20140410</string>" +
//						"<string>fbapi20140116</string>" +
//						"<string>fbapi20150313</string>" +
//						"<string>fbapi20150629</string>" +
//						"<string>fbauth</string>" +
//						"<string>fbauth2</string>" +
//						"<string>fb-messenger-api20140430</string>" +
//						"<string>fb-messenger-api</string>" +
//						"<string>fbauth2</string>" +
//						"<string>fbshareextension</string>" +
//					"</array>";
//			
//			
//			string content = File.ReadAllText(infoFilePath);
//			int idx = content.IndexOf("<key>");
//			if(idx > 0){
//				content = content.Insert(idx,elementsToAdd);
//			}else{
//				Debug.LogError("PostProcessBuild script can't update Info.plist file.");
//			}
//			File.WriteAllText(infoFilePath,content);
//		}else{
//			Debug.LogError("PostProcessBuild script can't update Info.plist file.");
//		}
//	}

	private static void iphoneCopyHelpshiftFiles(){
		DirectoryInfo srcDir = new DirectoryInfo (Application.dataPath + "/Helpshift/Plugins/iOS/HSResources");
		DirectoryInfo destDir = new DirectoryInfo (buildPath+"/Libraries/Helpshift/HSResources");

		copyAllFilesFromSrcToDest (srcDir, destDir);

		srcDir = new DirectoryInfo (Application.dataPath + "/Helpshift/Plugins/iOS/HSLocalization/en.lproj");
		destDir = new DirectoryInfo (buildPath+"/Libraries/Helpshift/HSLocalization/en.lproj");

		copyAllFilesFromSrcToDest (srcDir, destDir);

		srcDir = new DirectoryInfo (Application.dataPath + "/Helpshift/Plugins/iOS/HSThemes");
		destDir = new DirectoryInfo (buildPath+"/Libraries/Helpshift/HSThemes");

		copyAllFilesFromSrcToDest (srcDir, destDir);

		srcDir = new DirectoryInfo (Application.dataPath + "/Helpshift/Plugins/iOS");
		destDir = new DirectoryInfo (buildPath+"/Libraries/Helpshift");

		foreach (FileInfo fi in srcDir.GetFiles("*.json")) {
			fi.CopyTo(Path.Combine(destDir.ToString(), fi.Name), true);
		}
	}

	private static void iphoneCopyFrameworksFiles()
	{
		DirectoryInfo srcDir = new DirectoryInfo (Application.dataPath + "/../XCodeProjMods" + Path.DirectorySeparatorChar + "Frameworks" + Path.DirectorySeparatorChar);
		DirectoryInfo destDir = new DirectoryInfo (buildPath);
		
		copyAllFilesFromSrcToDest (srcDir, destDir);
	}
	
	public static void copyAllFilesFromSrcToDest(DirectoryInfo source, DirectoryInfo target)
	{
		// Check if the target directory exists, if not, create it.
		if (Directory.Exists(target.FullName) == false)
		{
			Directory.CreateDirectory(target.FullName);
		}
		
		// Copy each file into it’s new directory.
		foreach (FileInfo fi in source.GetFiles())
		{
			fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
		}
		
		// Copy each subdirectory using recursion.
		foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
		{
			DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
			copyAllFilesFromSrcToDest(diSourceSubDir, nextTargetSubDir);
		}
	}

	private static void iphoneEnableHealthKit(){
		string projectFile = buildPath + Path.DirectorySeparatorChar + PlayerSettings.productName+ ".xcodeproj" + Path.DirectorySeparatorChar + "project.pbxproj";

		if (File.Exists(projectFile)) {
			string content = File.ReadAllText(projectFile);
//			bool changed = false;

			string targetAttrib = 	"\t\t\t\t\t1D6058900D05DD3D006BFB54 = {\n\t\t\t\t\t\tDevelopmentTeam = 56458FQAD9;\n\t\t\t\t\t\tSystemCapabilities = {\n\t\t\t\t\t\t\tcom.apple.HealthKit = {\n\t\t\t\t\t\t\t\tenabled = 1;\n\t\t\t\t\t\t\t};\n\t\t\t\t\t\t};\n\t\t\t\t\t};\n\t\t\t\t\t5623C57217FDCB0800090B9E = {\n";
			string refKey = "5623C57217FDCB0800090B9E /* Fitkin Tests */ = {";

			if(content.IndexOf(refKey) > 0){
				content = content.Replace(refKey,targetAttrib);
				File.WriteAllText(projectFile,content);
			}
		}
	}
	
	private static void iphoneOverwriteUnityGenFiles()
	{
		DirectoryInfo srcDir  = new DirectoryInfo(Application.dataPath + "/../XCodeProjMods" + Path.DirectorySeparatorChar + iphoneOverwriteSrcDir);
		DirectoryInfo destDir = new DirectoryInfo(buildPath + Path.DirectorySeparatorChar + iphoneOverwriteDest);
		
		copySourceFilesFromSrcToDest(srcDir, destDir);
	}
	
	private static void  copySourceFilesFromSrcToDest(DirectoryInfo srcDir, DirectoryInfo destDir)
	{
		if (Directory.Exists(srcDir.FullName) && Directory.Exists(destDir.FullName))
		{
			foreach (FileInfo fi in srcDir.GetFiles()){
				if(fi.Extension.CompareTo(".h") == 0 
				|| fi.Extension.CompareTo(".m") == 0 
				|| fi.Extension.CompareTo(".mm") == 0
				|| fi.Extension.CompareTo(".entitlements") == 0)
				{
					fi.CopyTo(Path.Combine(destDir.ToString(), fi.Name), true);
				}
	        }
			
			foreach(DirectoryInfo di in srcDir.GetDirectories()){
				DirectoryInfo srcSubDir = new DirectoryInfo(srcDir.ToString() + Path.DirectorySeparatorChar + di.Name);
				DirectoryInfo destSubDir  = new DirectoryInfo(destDir.ToString() + Path.DirectorySeparatorChar + di.Name);
				
				copySourceFilesFromSrcToDest(srcSubDir, destSubDir);
			}	
		}
	}

	/*
	 private static void iphoneAddFabricFramework(){
		string projectFile = buildPath + Path.DirectorySeparatorChar + PlayerSettings.productName+ ".xcodeproj" + Path.DirectorySeparatorChar + "project.pbxproj";
		
		if (File.Exists(projectFile)) {
			string content = File.ReadAllText(projectFile);

			string shellScript = "C3FFEC1C1C5BE63700319F78 /* ShellScript * / = {\n" +
								 "\t\t\tisa = PBXShellScriptBuildPhase;\n" +
								 "\t\t\tbuildActionMask = 2147483647;\n" +
								 "\t\t\tfiles = (\n" +
								 "\t\t\t);\n" +
								 "\t\t\tinputPaths = (\n" +
								 "\t\t\t);\n" +
								 "\t\t\toutputPaths = (\n" +
								 "\t\t\t);\n" +
								 "\t\t\trunOnlyForDeploymentPostprocessing = 0;\n" +
								 "\t\t\tshellPath = /bin/sh;\n" +
								 "\t\t\tshellScript = \"./Frameworks/Plugins/iOS/fabric/Fabric.framework/run 40177459a1aff2fe4876d91b25c31e220afd679e ef21122971eac44c5a92c038d65dc8042b2345a1f6c18f9f93bc4752a5dc9925\";\n" +
								 "\t\t};";
			string shellScriptRefKey = "/* End PBXShellScriptBuildPhase section * /";

			bool changed = false;

			if(content.IndexOf(shellScript) < 0)
			{
				int index = content.IndexOf(shellScriptRefKey);
				if(index >= 0){
					content = content.Insert(index,shellScript);
					changed = true;
				}

				string shelScriptTarget = "C3FFEC1C1C5BE63700319F78 /* ShellScript * /,";
				string shelScriptTargetRefKey = "033966F41B18B03000ECD701 /* ShellScript * /,";
				index = content.IndexOf(shelScriptTargetRefKey);
				if(index >= 0){
					content = content.Insert(index,shelScriptTarget);
					changed = true;
				}
			}

			if(changed)
				File.WriteAllText(projectFile,content);
		}
	}*/

	private static void iphoneInjectCustomMonoExcetionReporter(){
		string unityEngineFile = buildPath + Path.DirectorySeparatorChar +"Classes" + Path.DirectorySeparatorChar + "Native" + Path.DirectorySeparatorChar + "Bulk_UnityEngine_3.cpp";
		if (File.Exists(unityEngineFile)) {
			string content = File.ReadAllText(unityEngineFile);

			string PRINT_METHOD_DEF = "extern \"C\"  void UnhandledExceptionHandler_PrintException_";
			string DEBUG_LOG_DEF = "Debug_LogError_m4127342994(NULL /*static, unused*/, L_3, /*hidden argument*/NULL);";

			if(content.IndexOf(PRINT_METHOD_DEF) > 0){
				content = content.Insert(content.IndexOf(PRINT_METHOD_DEF),"\nextern \"C\"  void HandleException(char* str);\n");
				if(content.IndexOf(DEBUG_LOG_DEF) > 0){
					content = content.Insert(content.IndexOf(DEBUG_LOG_DEF),"\n\t\tHandleException(il2cpp_codegen_marshal_string(L_3));\n");
					File.WriteAllText( unityEngineFile, content);
				}else
					Debug.LogError("Error adding mono exception handler.Can't find insert location");
			}else
				Debug.LogError("Error adding mono exception handler.Can't find method UnhandledExceptionHandler_PrintException");
		}else 
			Debug.LogError("Error adding mono exception handler.Cna't find file:" + unityEngineFile);
	}

	private static void iphoneUpdateOptimizationLevel(){
		string projectFile = buildPath + Path.DirectorySeparatorChar + PlayerSettings.productName+ ".xcodeproj" + Path.DirectorySeparatorChar + "project.pbxproj";

		if (File.Exists(projectFile)) {
			string content = File.ReadAllText(projectFile);
			bool changed = false;

			string optimizationLevel = "\n\t\t\t\tGCC_OPTIMIZATION_LEVEL = 1;";
			string refKey = "1D6058950D05DD3E006BFB54 /* Release */ = {\n\t\t\tisa = XCBuildConfiguration;\n\t\t\tbuildSettings = {";

			if(content.IndexOf(refKey + optimizationLevel) < 0)
			{
				int index = content.IndexOf(refKey);
				if(index >= 0){
					content = content.Insert(index + refKey.Length,optimizationLevel);
					changed = true;
				}
			}

			refKey = "C01FCF5008A954540054247B /* Release */ = {\n\t\t\tisa = XCBuildConfiguration;\n\t\t\tbuildSettings = {";
			if(content.IndexOf(refKey + optimizationLevel) < 0)
			{
				int index = content.IndexOf(refKey);
				if(index >= 0){
					content = content.Insert(index + refKey.Length,optimizationLevel);
					changed = true;
				}
			}
			if(changed)
				File.WriteAllText(projectFile,content);
		}
	}

//	private static void iphoneUpdateFraworks()
//	{
//
//		string projectFile = buildPath + Path.DirectorySeparatorChar + PlayerSettings.productName+ ".xcodeproj" + Path.DirectorySeparatorChar + "project.pbxproj";
//		
//		if (File.Exists(projectFile)) {
//			string content = File.ReadAllText(projectFile);
//			
//			string frameworks = "4433F7CD17C63EBA0064878C /* Twitter.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = 4433F7CC17C63EBA0064878C /* Twitter.framework */; };" +
//								"4433F7D117C63EC20064878C /* Social.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = 4433F7D017C63EC20064878C /* Social.framework */; settings = {ATTRIBUTES = (Weak, ); }; };" +
//								//"445296361861E2B00035198D /* FacebookSDK.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = 445296351861E2B00035198D /* FacebookSDK.framework */; };" +
//								"4433F7CF17C63EBE0064878C /* Accounts.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = 4433F7CE17C63EBE0064878C /* Accounts.framework */; };" +
//								"C344C30516667873003F3C18 /* CoreBluetooth.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = C344C30416667873003F3C18 /* CoreBluetooth.framework */; };" +
//								"C344C3071666787A003F3C18 /* libxml2.dylib in Frameworks */ = {isa = PBXBuildFile; fileRef = C344C3061666787A003F3C18 /* libxml2.dylib */; };"+
//								"C344C3091666787F003F3C18 /* libsqlite3.dylib in Frameworks */ = {isa = PBXBuildFile; fileRef = C344C3081666787F003F3C18 /* libsqlite3.dylib */; };"+
//								"C37F76F016C28D57000B4AFC /* AddressBook.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = C37F76EF16C28D57000B4AFC /* AddressBook.framework */; };" +
//								"448DFCEF18323A2F00428DE4 /* MobileCoreServices.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = 448DFCEE18323A2F00428DE4 /* MobileCoreServices.framework */; };" +
//								"7196A27D199B77C400339C7C /* CoreTelephony.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = 7196A27A199B77C400339C7C /* CoreTelephony.framework */; };" +
//								"7196A27E199B77C400339C7C /* CoreText.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = 7196A27B199B77C400339C7C /* CoreText.framework */; };" +
//								"7196A27F199B77C400339C7C /* libz.dylib in Frameworks */ = {isa = PBXBuildFile; fileRef = 7196A27C199B77C400339C7C /* libz.dylib */; };"+
//								"C3CE5CDE1B387F5600CE32D0 /* Photos.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = C3CE5CDD1B387F5600CE32D0 /* Photos.framework */; };" +
//								"C385518F1B84B7D400456781 /* UXCam.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = C385518E1B84B7D400456781 /* UXCam.framework */; };";
//			
//			string refString = "1D60589F0D05DD5A006BFB54 /* Foundation.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = 1D30AB110D05D00D00671497 /* Foundation.framework */; };";
//			
//			if(content.IndexOf(frameworks) < 0)
//			{
//				int index = content.IndexOf(refString);
//				if(index >= 0){
//					content = content.Insert(index,frameworks);
//				}
//				
//				string frameworks2 ="4433F7CC17C63EBA0064878C /* Twitter.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = Twitter.framework; path = System/Library/Frameworks/Twitter.framework; sourceTree = SDKROOT; };" +
//									"4433F7CE17C63EBE0064878C /* Accounts.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = Accounts.framework; path = System/Library/Frameworks/Accounts.framework; sourceTree = SDKROOT; };" +
//									"4433F7D017C63EC20064878C /* Social.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = Social.framework; path = System/Library/Frameworks/Social.framework; sourceTree = SDKROOT; };" +
//									//"445296351861E2B00035198D /* FacebookSDK.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = FacebookSDK.framework; path = ../../Assets/Plugins/iOS/facebook/FacebookSDK.framework; sourceTree = \"<group>\"; };" +
//									"C344C30416667873003F3C18 /* CoreBluetooth.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = CoreBluetooth.framework; path = System/Library/Frameworks/CoreBluetooth.framework; sourceTree = SDKROOT; };"+
//									"C344C3061666787A003F3C18 /* libxml2.dylib */ = {isa = PBXFileReference; lastKnownFileType = \"compiled.mach-o.dylib\"; name = libxml2.dylib; path = usr/lib/libxml2.dylib; sourceTree = SDKROOT; };"+
//									"C344C3081666787F003F3C18 /* libsqlite3.dylib */ = {isa = PBXFileReference; lastKnownFileType = \"compiled.mach-o.dylib\"; name = libsqlite3.dylib; path = usr/lib/libsqlite3.dylib; sourceTree = SDKROOT; };"+
//									"C37F76EF16C28D57000B4AFC /* AddressBook.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = AddressBook.framework; path = System/Library/Frameworks/AddressBook.framework; sourceTree = SDKROOT; };"+
//									"448DFCEE18323A2F00428DE4 /* MobileCoreServices.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = MobileCoreServices.framework; path = System/Library/Frameworks/MobileCoreServices.framework; sourceTree = SDKROOT; };" +
//									"7196A27A199B77C400339C7C /* CoreTelephony.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = CoreTelephony.framework; path = System/Library/Frameworks/CoreTelephony.framework; sourceTree = SDKROOT; };" +
//									"7196A27B199B77C400339C7C /* CoreText.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = CoreText.framework; path = System/Library/Frameworks/CoreText.framework; sourceTree = SDKROOT; };" +
//									"7196A27C199B77C400339C7C /* libz.dylib */ = {isa = PBXFileReference; lastKnownFileType = \"compiled.mach-o.dylib\"; name = libz.dylib; path = usr/lib/libz.dylib; sourceTree = SDKROOT; };"+
//									"C3CE5CDD1B387F5600CE32D0 /* Photos.framework */ = {isa = PBXFileReference; lastKnownFileType = ilsplaswrapper.framework; name = Photos.framework; path = System/Library/Frameworks/Photos.framework; sourceTree = SDKROOT; };" + 
//									"C385518E1B84B7D400456781 /* UXCam.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; path = UXCam.framework; sourceTree = \"<group>\"; };";
//
//
//
//				string refString2  = "1D30AB110D05D00D00671497 /* Foundation.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = Foundation.framework; path = System/Library/Frameworks/Foundation.framework; sourceTree = SDKROOT; };";
//				
//				index = content.IndexOf(refString2);
//				if(index >= 0){
//					content = content.Insert(index,frameworks2);
//				}
//				
//				string frameworks3 ="C344C3091666787F003F3C18 /* libsqlite3.dylib in Frameworks */," +
//									"C344C3071666787A003F3C18 /* libxml2.dylib in Frameworks */," +
//									"4433F7D117C63EC20064878C /* Social.framework in Frameworks */," +
//									//"445296361861E2B00035198D /* FacebookSDK.framework in Frameworks */," +
//									"4433F7CF17C63EBE0064878C /* Accounts.framework in Frameworks */," +
//									"4433F7CD17C63EBA0064878C /* Twitter.framework in Frameworks */," +
//									"C344C30516667873003F3C18 /* CoreBluetooth.framework in Frameworks */," +
//									"C37F76F016C28D57000B4AFC /* AddressBook.framework in Frameworks */," +
//									"448DFCEF18323A2F00428DE4 /* MobileCoreServices.framework in Frameworks */," +
//									"7196A27D199B77C400339C7C /* CoreTelephony.framework in Frameworks */," +
//									"7196A27E199B77C400339C7C /* CoreText.framework in Frameworks */," +
//									"7196A27F199B77C400339C7C /* libz.dylib in Frameworks */,"+
//									"C3CE5CDE1B387F5600CE32D0 /* Photos.framework in Frameworks */," +
//									"C385518F1B84B7D400456781 /* UXCam.framework in Frameworks */,";
//
//				string refString3  = "1D60589F0D05DD5A006BFB54 /* Foundation.framework in Frameworks */,";
//				
//				index = content.IndexOf(refString3);
//				if(index >= 0){
//					content = content.Insert(index,frameworks3);
//				}
//				
//				string frameworks4 ="C344C3081666787F003F3C18 /* libsqlite3.dylib */,"+
//									"C344C3061666787A003F3C18 /* libxml2.dylib */,"+
//									"4433F7D017C63EC20064878C /* Social.framework */," +
//									"4433F7CE17C63EBE0064878C /* Accounts.framework */," +
//									"4433F7CC17C63EBA0064878C /* Twitter.framework */," +
//									//"445296351861E2B00035198D /* FacebookSDK.framework */," +
//									"C344C30416667873003F3C18 /* CoreBluetooth.framework */," +
//									"C37F76EF16C28D57000B4AFC /* AddressBook.framework */," +
//									"448DFCEE18323A2F00428DE4 /* MobileCoreServices.framework */," +
//									"7196A27A199B77C400339C7C /* CoreTelephony.framework */," +
//									"7196A27B199B77C400339C7C /* CoreText.framework */," +
//									"7196A27C199B77C400339C7C /* libz.dylib */,"+
//									"C3CE5CDD1B387F5600CE32D0 /* Photos.framework */," +
//									"C385518E1B84B7D400456781 /* UXCam.framework */,";
//				
//				string refString4  = "56BCBA380FCF049A0030C3B2 /* SystemConfiguration.framework */,";
//				
//				index = content.IndexOf(refString4);
//				if(index >= 0){
//					content = content.Insert(index,frameworks4);
//				}
//			}
//			
//			File.WriteAllText(projectFile,content);
//		}
//	}

//	private static void addIOSResources()
//	{
//		string projectFile = buildPath + Path.DirectorySeparatorChar + PlayerSettings.productName+".xcodeproj" + Path.DirectorySeparatorChar + "project.pbxproj";
//		
//		if (File.Exists(projectFile)) {
//			string content = File.ReadAllText(projectFile);
//			
//			string refString = "1D60589F0D05DD5A006BFB54 /* Foundation.framework in Frameworks */ = {isa = PBXBuildFile; fileRef = 1D30AB110D05D00D00671497 /* Foundation.framework */; };";
//			int index = content.IndexOf(refString);
//			if (index >= 0) {
//				string PBXBuildFile_section ="C3ECAD8E19BA058500A24E82 /* splash_ph5_1136x640.png in Resources */ = {isa = PBXBuildFile; fileRef = C3ECAD8D19BA058500A24E82 /* splash_ph5_1136x640.png */; };"+
//						"C352D0E51AF154360085A962 /* 25.wav in Resources */ = {isa = PBXBuildFile; fileRef = C352D0DF1AF154350085A962 /* 25.wav */; };" +
//						"C352D0E61AF154360085A962 /* 50.wav in Resources */ = {isa = PBXBuildFile; fileRef = C352D0E01AF154350085A962 /* 50.wav */; };" +
//						"C352D0E71AF154360085A962 /* 75.wav in Resources */ = {isa = PBXBuildFile; fileRef = C352D0E11AF154350085A962 /* 75.wav */; };" +
//						"C352D0E81AF154360085A962 /* complete.wav in Resources */ = {isa = PBXBuildFile; fileRef = C352D0E21AF154350085A962 /* complete.wav */; };" +
//						"C352D0E91AF154360085A962 /* fail.wav in Resources */ = {isa = PBXBuildFile; fileRef = C352D0E31AF154350085A962 /* fail.wav */; };" +
//						"C352D0EA1AF154360085A962 /* move.wav in Resources */ = {isa = PBXBuildFile; fileRef = C352D0E41AF154350085A962 /* move.wav */; };"+
//						"C3CE5CDC1B387DA800CE32D0 /* run.wav in Resources */ = {isa = PBXBuildFile; fileRef = C3CE5CDB1B387DA800CE32D0 /* run.wav */; };";
//				content = content.Insert(index, PBXBuildFile_section);
//			}
//			
//			refString = "1D30AB110D05D00D00671497 /* Foundation.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = Foundation.framework; path = System/Library/Frameworks/Foundation.framework; sourceTree = SDKROOT; };";
//			index = content.IndexOf(refString);
//			if (index >= 0) {
//				string PBXFileReference_section =	"C3ECAD8D19BA058500A24E82 /* splash_ph5_1136x640.png */ = {isa = PBXFileReference; lastKnownFileType = image.png; name = splash_ph5_1136x640.png; path = Data/Raw/splash_ph5_1136x640.png; sourceTree = \"<group>\"; };"+
//													"C352D0DF1AF154350085A962 /* 25.wav */ = {isa = PBXFileReference; lastKnownFileType = audio.wav; name = 25.wav; path = Data/Raw/25.wav; sourceTree = \"<group>\"; };"+
//													"C352D0E01AF154350085A962 /* 50.wav */ = {isa = PBXFileReference; lastKnownFileType = file; name = 50.wav; path = Data/Raw/50.wav; sourceTree = \"<group>\"; };"+
//													"C352D0E11AF154350085A962 /* 75.wav */ = {isa = PBXFileReference; lastKnownFileType = file; name = 75.wav; path = Data/Raw/75.wav; sourceTree = \"<group>\"; };"+
//													"C352D0E21AF154350085A962 /* complete.wav */ = {isa = PBXFileReference; lastKnownFileType = file; name = complete.wav; path = Data/Raw/complete.wav; sourceTree = \"<group>\"; };"+
//													"C352D0E31AF154350085A962 /* fail.wav */ = {isa = PBXFileReference; lastKnownFileType = file; name = fail.wav; path = Data/Raw/fail.wav; sourceTree = \"<group>\"; };"+
//													"C352D0E41AF154350085A962 /* move.wav */ = {isa = PBXFileReference; lastKnownFileType = file; name = move.wav; path = Data/Raw/move.wav; sourceTree = \"<group>\"; };" +
//													"C357BDFD1B832DC700991E10 /* fitsy_config.h */ = {isa = PBXFileReference; fileEncoding = 4; lastKnownFileType = sourcecode.c.h; path = fitsy_config.h; sourceTree = \"<group>\"; };"+
//													"C3CE5CDB1B387DA800CE32D0 /* run.wav */ = {isa = PBXFileReference; lastKnownFileType = audio.wav; name = run.wav; path = Data/Raw/run.wav; sourceTree = \"<group>\"; };" +
//													"BD3ADBBC1B53B9B10013B4EB /* UXCam.framework */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; path = UXCam.framework; sourceTree = \"<group>\"; };";
//													
//				content = content.Insert(index, PBXFileReference_section);
//			}
//			
//			refString = "29B97323FDCFA39411CA2CEA /* Frameworks */,";
//			index = content.IndexOf(refString);
//			if (index >= 0) {
//				string CustomTemplate_section = "C3ECAD8D19BA058500A24E82 /* splash_ph5_1136x640.png */," + 
//												"BD3ADBBC1B53B9B10013B4EB /* UXCam.framework */,";
//				content = content.Insert(index, CustomTemplate_section);
//			}
//			
//			refString = "56C56C9817D6015200616839 /* Images.xcassets in Resources */,";
//			index = content.IndexOf(refString); 
//			if (index >= 0) {
//				string PBXResourcesBuildPhase_section = "C3ECAD8E19BA058500A24E82 /* splash_ph5_1136x640.png in Resources */,"+"C352D0E51AF154360085A962 /* 25.wav in Resources */,"+
//						"C352D0E61AF154360085A962 /* 50.wav in Resources */,"+
//						"C352D0E71AF154360085A962 /* 75.wav in Resources */,"+
//						"C352D0E81AF154360085A962 /* complete.wav in Resources */,"+	
//						"C352D0E91AF154360085A962 /* fail.wav in Resources */,"+
//						"C352D0EA1AF154360085A962 /* move.wav in Resources */,"+
//						"C3CE5CDC1B387DA800CE32D0 /* run.wav in Resources */,";
//				content = content.Insert(index, PBXResourcesBuildPhase_section);
//			}
//
//			refString = "8A5C148F174E662D0006EB36 /* PluginBase */,";
//			index = content.IndexOf(refString); 
//			if (index >= 0) {
//				string PBX_Classes = "C357BDFD1B832DC700991E10 /* fitsy_config.h */,";
//				content = content.Insert(index, PBX_Classes);
//			}
//		
//			refString = "COPY_PHASE_STRIP = ";
//			index = content.IndexOf(refString); 
//
//			while (index >= 0) {
//				String refString2 = "FRAMEWORK_SEARCH_PATHS = (";
//				int index2 = content.IndexOf(refString2,index); 
//
//				if(index2 - index > 100){
//					int insertIndex = content.IndexOf(";",index);
//					content = content.Insert(insertIndex + 1 , "\n\t\t\t\tFRAMEWORK_SEARCH_PATHS = (\n\t\t\t\t\t\"$(inherited)\",\n\t\t\t\t\t\"$(PROJECT_DIR)\",\n\t\t\t\t);");
//				}
//				index = content.IndexOf(refString,index + refString.Length);
//			}
//
//			File.WriteAllText(projectFile, content);
//		}
//	}

}