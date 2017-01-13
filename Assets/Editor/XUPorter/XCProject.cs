using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityEditor.XCodeEditor
{
	public partial class XCProject : System.IDisposable
	{
		private PBXDictionary _datastore;
		public PBXDictionary _objects;
		//private PBXDictionary _configurations;
		
		private PBXGroup _rootGroup;
		//private string _defaultConfigurationName;
		private string _rootObjectKey;
	
		public string projectRootPath { get; private set; }
		private FileInfo projectFileInfo;
		
		public string filePath { get; private set; }
		//private string sourcePathRoot;
		private bool modified = false;
		
		#region Data
		
		// Objects
		private PBXSortedDictionary<PBXBuildFile> _buildFiles;
		private PBXSortedDictionary<PBXGroup> _groups;
		private PBXSortedDictionary<PBXFileReference> _fileReferences;
		private PBXDictionary<PBXNativeTarget> _nativeTargets;
		
		private PBXDictionary<PBXFrameworksBuildPhase> _frameworkBuildPhases;
		private PBXDictionary<PBXResourcesBuildPhase> _resourcesBuildPhases;
		private PBXDictionary<PBXShellScriptBuildPhase> _shellScriptBuildPhases;
		private PBXDictionary<PBXSourcesBuildPhase> _sourcesBuildPhases;
		private PBXDictionary<PBXCopyFilesBuildPhase> _copyBuildPhases;
				
		private PBXDictionary<PBXVariantGroup> _variantGroups;
		private PBXDictionary<XCBuildConfiguration> _buildConfigurations;
		private PBXSortedDictionary<XCConfigurationList> _configurationLists;
		
		private PBXProject _project;
		
		#endregion


		#region Constructor
		
		public XCProject()
		{
			
		}
		
		public XCProject( string filePath ) : this()
		{
			if( !System.IO.Directory.Exists( filePath ) ) {
				Debug.LogWarning( "XCode project path does not exist: " + filePath );
				return;
			}
			
			if( filePath.EndsWith( ".xcodeproj" ) ) {
				Debug.Log( "Opening project " + filePath );
				this.projectRootPath = Path.GetDirectoryName( filePath );
				this.filePath = filePath;
			} else {
				Debug.Log( "Looking for xcodeproj files in " + filePath );
				string[] projects = System.IO.Directory.GetDirectories( filePath, "*.xcodeproj" );
				if( projects.Length == 0 ) {
					Debug.LogWarning( "Error: missing xcodeproj file" );
					return;
				}
				
				this.projectRootPath = filePath;
				//if the path is relative to the project, we need to make it absolute
				if (!System.IO.Path.IsPathRooted(projectRootPath))
					projectRootPath = Application.dataPath.Replace("Assets", "") + projectRootPath;
				//Debug.Log ("projectRootPath adjusted to " + projectRootPath);
				this.filePath = projects[ 0 ];
			}
			
			projectFileInfo = new FileInfo( Path.Combine( this.filePath, "project.pbxproj" ) );
			string contents = projectFileInfo.OpenText().ReadToEnd();
			
			PBXParser parser = new PBXParser();
			_datastore = parser.Decode( contents );
			if( _datastore == null ) {
				throw new System.Exception( "Project file not found at file path " + filePath );
			}

			if( !_datastore.ContainsKey( "objects" ) ) {
				Debug.Log( "Errore " + _datastore.Count );
				return;
			}
			
			_objects = (PBXDictionary)_datastore["objects"];
			modified = false;
			
			_rootObjectKey = (string)_datastore["rootObject"];
			if( !string.IsNullOrEmpty( _rootObjectKey ) ) {
				_project = new PBXProject( _rootObjectKey, (PBXDictionary)_objects[ _rootObjectKey ] );
				_rootGroup = new PBXGroup( _rootObjectKey, (PBXDictionary)_objects[ _project.mainGroupID ] );
			}
			else {
				Debug.LogWarning( "error: project has no root object" );
				_project = null;
				_rootGroup = null;
			}

		}
		
		#endregion


		#region Properties
		
		public PBXProject project {
			get {
				return _project;
			}
		}
		
		public PBXGroup rootGroup {
			get {
				return _rootGroup;
			}
		}
		
		public PBXSortedDictionary<PBXBuildFile> buildFiles {
			get {
				if( _buildFiles == null ) {
					_buildFiles = new PBXSortedDictionary<PBXBuildFile>( _objects );
				}
				return _buildFiles;
			}
		}
		
		public PBXSortedDictionary<PBXGroup> groups {
			get {
				if( _groups == null ) {
					_groups = new PBXSortedDictionary<PBXGroup>( _objects );
				}
				return _groups;
			}
		}
		
		public PBXSortedDictionary<PBXFileReference> fileReferences {
			get {
				if( _fileReferences == null ) {
					_fileReferences = new PBXSortedDictionary<PBXFileReference>( _objects );
				}
				return _fileReferences;
			}
		}
		
		public PBXDictionary<PBXVariantGroup> variantGroups {
			get {
				if( _variantGroups == null ) {
					_variantGroups = new PBXDictionary<PBXVariantGroup>( _objects );
				}
				return _variantGroups;
			}
		}
		
		public PBXDictionary<PBXNativeTarget> nativeTargets {
			get {
				if( _nativeTargets == null ) {
					_nativeTargets = new PBXDictionary<PBXNativeTarget>( _objects );
				}
				return _nativeTargets;
			}
		}
		
		public PBXDictionary<XCBuildConfiguration> buildConfigurations {
			get {
				if( _buildConfigurations == null ) {
					_buildConfigurations = new PBXDictionary<XCBuildConfiguration>( _objects );
				}
				return _buildConfigurations;
			}
		}
		
		public PBXSortedDictionary<XCConfigurationList> configurationLists {
			get {
				if( _configurationLists == null ) {
					_configurationLists = new PBXSortedDictionary<XCConfigurationList>( _objects );
				}
				return _configurationLists;
			}
		}
		
		public PBXDictionary<PBXFrameworksBuildPhase> frameworkBuildPhases {
			get {
				if( _frameworkBuildPhases == null ) {
					_frameworkBuildPhases = new PBXDictionary<PBXFrameworksBuildPhase>( _objects );
				}
				return _frameworkBuildPhases;
			}
		}
	
		public PBXDictionary<PBXResourcesBuildPhase> resourcesBuildPhases {
			get {
				if( _resourcesBuildPhases == null ) {
					_resourcesBuildPhases = new PBXDictionary<PBXResourcesBuildPhase>( _objects );
				}
				return _resourcesBuildPhases;
			}
		}
	
		public PBXDictionary<PBXShellScriptBuildPhase> shellScriptBuildPhases {
			get {
				if( _shellScriptBuildPhases == null ) {
					_shellScriptBuildPhases = new PBXDictionary<PBXShellScriptBuildPhase>( _objects );
				}
				return _shellScriptBuildPhases;
			}
		}
	
		public PBXDictionary<PBXSourcesBuildPhase> sourcesBuildPhases {
			get {
				if( _sourcesBuildPhases == null ) {
					_sourcesBuildPhases = new PBXDictionary<PBXSourcesBuildPhase>( _objects );
				}
				return _sourcesBuildPhases;
			}
		}
	
		public PBXDictionary<PBXCopyFilesBuildPhase> copyBuildPhases {
			get {
				if( _copyBuildPhases == null ) {
					_copyBuildPhases = new PBXDictionary<PBXCopyFilesBuildPhase>( _objects );
				}
				return _copyBuildPhases;
			}
		}

		#endregion


		#region PBXMOD
		
		public bool AddOtherCFlags( string flag )
		{
			return AddOtherCFlags( new PBXList( flag ) ); 
		}
		
		public bool AddOtherCFlags( PBXList flags )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddOtherCFlags( flags );
			}
			modified = true;
			return modified;	
		}

		public bool AddOtherLinkerFlags( string flag )
		{
			return AddOtherLinkerFlags( new PBXList( flag ) ); 
		}
		
		public bool AddOtherLinkerFlags( PBXList flags )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddOtherLinkerFlags( flags );
			}
			modified = true;
			return modified;	
		}
		
		public bool overwriteBuildSetting( string settingName, string newValue, string buildConfigName = "all") {
			Debug.Log("overwriteBuildSetting " + settingName + " " + newValue + " " + buildConfigName);
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				//Debug.Log ("build config " + buildConfig);
				XCBuildConfiguration b = buildConfig.Value;
				if ( (string)b.data["name"] == buildConfigName || (string)buildConfigName == "all") {
					//Debug.Log ("found " + b.data["name"] + " config");
					buildConfig.Value.overwriteBuildSetting(settingName, newValue);
					modified = true;
				} else {
					//Debug.LogWarning ("skipping " + buildConfigName + " config " + (string)b.data["name"]);
				}
			}
			return modified;
		}

		public bool AddHeaderSearchPaths( string path )
		{
			return AddHeaderSearchPaths( new PBXList( path ) );
		}
		
		public bool AddHeaderSearchPaths( PBXList paths )
		{
			Debug.Log ("AddHeaderSearchPaths " + paths);
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddHeaderSearchPaths( paths );
			}
			modified = true;
			return modified;
		}
		
		public bool AddLibrarySearchPaths( string path )
		{
			return AddLibrarySearchPaths( new PBXList( path ) );
		}
		
		public bool AddLibrarySearchPaths( PBXList paths )
		{
			Debug.Log ("AddLibrarySearchPaths " + paths);
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddLibrarySearchPaths( paths );
			}
			modified = true;
			return modified;
		}
		
		public bool AddFrameworkSearchPaths( string path )
		{
			return AddFrameworkSearchPaths( new PBXList( path ) );
		}

		public bool AddFrameworkSearchPaths( PBXList paths )
		{
			foreach( KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations ) {
				buildConfig.Value.AddFrameworkSearchPaths( paths );
			}
			modified = true;
			return modified;
		}
		
		public object GetObject( string guid )
		{
			return _objects[guid];
		}
		
		public PBXDictionary AddFile( string filePath, PBXGroup parent = null, TreeEnum tree = TreeEnum.SDKROOT, bool createBuildFiles = true, bool weak = false )
		{
			Debug.Log("AddFile " + filePath + ", " + parent + ", " + tree + ", " + (createBuildFiles? "TRUE":"FALSE") + ", " + (weak? "TRUE":"FALSE") ); 
			
			PBXDictionary results = new PBXDictionary();
			if (filePath == null) {
				Debug.LogError ("AddFile called with null filePath");
				return results;
			}

			string absPath = string.Empty;
			
			if( Path.IsPathRooted( filePath ) ) {
				Debug.Log( "Path is Rooted" );
				absPath = filePath;
			}
			else if(tree == TreeEnum.SOURCE_ROOT){
				absPath = Path.Combine(projectRootPath,filePath);
			}else if( tree != TreeEnum.SDKROOT) {
				absPath = Path.Combine( Application.dataPath, filePath );
				Debug.LogError ("AddFile absolute path:"+absPath);
			}
			
			if( !( File.Exists( absPath ) || Directory.Exists( absPath ) ) && tree != TreeEnum.SDKROOT) {
				Debug.Log( "Missing file: " + filePath );
				return results;
			}
			else if( tree == TreeEnum.SOURCE_ROOT) {
				Debug.Log( "Source Root File" );
				System.Uri fileURI = new System.Uri( absPath );
				System.Uri rootURI = new System.Uri( ( projectRootPath + "/." ) );
				filePath = rootURI.MakeRelativeUri( fileURI ).ToString();
			}
			else if( tree == TreeEnum.GROUP) {
				Debug.Log( "Group File" );
				filePath = System.IO.Path.GetFileName( filePath );
			}

			if( parent == null ) {
				parent = _rootGroup;
			}
			
			//Check if there is already a file
			PBXFileReference fileReference = GetFile( System.IO.Path.GetFileName( filePath ) );	
			if( fileReference != null && !filePath.EndsWith(".strings") ) {
				Debug.Log("File already exists: " + filePath); //not a warning, because this is normal for most builds!
				return null;
			}
			
			fileReference = new PBXFileReference( filePath, tree);
			parent.AddChild( fileReference );
			fileReferences.Add( fileReference );
			results.Add( fileReference.guid, fileReference );
			
			//Create a build file for reference
			if( !string.IsNullOrEmpty( fileReference.buildPhase ) && createBuildFiles ) {
				
				switch( fileReference.buildPhase ) {
					case "PBXFrameworksBuildPhase":
						foreach( KeyValuePair<string, PBXFrameworksBuildPhase> currentObject in frameworkBuildPhases ) {
							BuildAddFile(fileReference,currentObject,weak);
						}
						if ( !string.IsNullOrEmpty( absPath ) && tree == TreeEnum.SOURCE_ROOT) {
							string libraryPath = Path.Combine( "$(SRCROOT)", Path.GetDirectoryName( filePath ) );
							if (File.Exists(absPath)) {
								this.AddLibrarySearchPaths( new PBXList( libraryPath ) ); 
							} else {
								this.AddFrameworkSearchPaths( new PBXList( libraryPath ) );  
							}
						}
						break;
					case "PBXResourcesBuildPhase":
						foreach( KeyValuePair<string, PBXResourcesBuildPhase> currentObject in resourcesBuildPhases ) {
							Debug.Log( "Adding Resources Build File" );
							BuildAddFile(fileReference,currentObject,weak);
						}
						break;
					case "PBXShellScriptBuildPhase":
						foreach( KeyValuePair<string, PBXShellScriptBuildPhase> currentObject in shellScriptBuildPhases ) {
							Debug.Log( "Adding Script Build File" );
							BuildAddFile(fileReference,currentObject,weak);
						}
						break;
					case "PBXSourcesBuildPhase":
						foreach( KeyValuePair<string, PBXSourcesBuildPhase> currentObject in sourcesBuildPhases ) {
							Debug.Log( "Adding Source Build File" );
							BuildAddFile(fileReference,currentObject,weak);
						}
						break;
					case "PBXCopyFilesBuildPhase":
						foreach( KeyValuePair<string, PBXCopyFilesBuildPhase> currentObject in copyBuildPhases ) {
							Debug.Log( "Adding Copy Files Build Phase" );
							BuildAddFile(fileReference,currentObject,weak);
						}
						break;
					case null:
						Debug.LogWarning( "File Not Supported: " + filePath );
						break;
					default:
						Debug.LogWarning( "File Not Supported." );
						return null;
				}
			}
			return results;
		}

		public PBXNativeTarget GetNativeTarget( string name )
		{
			PBXNativeTarget naviTarget = null;
			foreach( KeyValuePair<string, PBXNativeTarget> currentObject in nativeTargets ) {
				string targetName = (string)currentObject.Value.data["name"];
				if (targetName == name) {
					naviTarget = currentObject.Value;
					break;
				}
			}
			return naviTarget;
		}

		public int GetBuildActionMask()
		{
			int buildActionMask = 0;
			foreach( var currentObject in copyBuildPhases ) 
			{
				buildActionMask = (int)currentObject.Value.data["buildActionMask"];
				break;
			}
			return buildActionMask;
		}

		public PBXCopyFilesBuildPhase AddEmbedFrameworkBuildPhase()
		{
			PBXCopyFilesBuildPhase phase = null;

			PBXNativeTarget naviTarget = GetNativeTarget("Unity-iPhone");
			if (naviTarget == null)
			{
				Debug.Log("Not found Correct NativeTarget.");
				return phase;
			}

			//check if embed framework buildPhase exist
			foreach( var currentObject in copyBuildPhases ) 
			{
				object nameObj = null;
				if (currentObject.Value.data.TryGetValue("name", out nameObj))
				{
					string name = (string)nameObj;
					if (name == "Embed Frameworks")
						return currentObject.Value;
				}
			}

			int buildActionMask = this.GetBuildActionMask();
			phase = new PBXCopyFilesBuildPhase(buildActionMask);
			var buildPhases = (ArrayList)naviTarget.data["buildPhases"];
			buildPhases.Add(phase.guid);//add build phase
			copyBuildPhases.Add(phase);
			return phase;
		}

		public void AddEmbedFramework( string fileName)
		{
			Debug.Log( "Add Embed Framework: " + fileName );

			//Check if there is already a file
			PBXFileReference fileReference = GetFile( System.IO.Path.GetFileName( fileName ) );	
			if( fileReference == null ) {
				Debug.Log("Embed Framework must added already: " + fileName);
				return;
			}

			var embedPhase = this.AddEmbedFrameworkBuildPhase();
			if (embedPhase == null)
			{
				Debug.Log("AddEmbedFrameworkBuildPhase Failed.");
				return;
			}

			//create a build file
			PBXBuildFile buildFile = new PBXBuildFile( fileReference );
			buildFile.AddCodeSignOnCopy();
			buildFiles.Add( buildFile );

			embedPhase.AddBuildFile(buildFile);
		}

		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXFrameworksBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXResourcesBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXShellScriptBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXSourcesBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		private void BuildAddFile (PBXFileReference fileReference, KeyValuePair<string, PBXCopyFilesBuildPhase> currentObject,bool weak)
		{
			PBXBuildFile buildFile = new PBXBuildFile( fileReference, weak );
			buildFiles.Add( buildFile );
			currentObject.Value.AddBuildFile( buildFile );
		}
		
		public bool AddFolder( string folderPath, PBXGroup parent = null, string[] exclude = null, bool recursive = true, bool createBuildFile = true )
		{
			Debug.Log("Folder PATH: "+folderPath);
			if( !Directory.Exists( folderPath ) ){
				Debug.Log("Directory doesn't exist?");
				return false;
			}

			if (folderPath.EndsWith(".lproj")){
				Debug.Log("Ended with .lproj");
				return AddLocFolder(folderPath, parent, exclude, createBuildFile);
			}

 			DirectoryInfo sourceDirectoryInfo = new DirectoryInfo( folderPath );

 			if( exclude == null ){
				Debug.Log("Exclude was null");
 				exclude = new string[] {};
			}
 			
 			if( parent == null ){
				Debug.Log("Parent was null");
 				parent = rootGroup;
			}
			
			// Create group
			PBXGroup newGroup = GetGroup( sourceDirectoryInfo.Name, null /*relative path*/, parent );
			Debug.Log("New Group created");

			foreach( string directory in Directory.GetDirectories( folderPath ) ) {
				Debug.Log( "DIR: " + directory );
				if( directory.EndsWith( ".bundle" ) ) {
					// Treat it like a file and copy even if not recursive
					// TODO also for .xcdatamodeld?
					Debug.LogWarning( "This is a special folder: " + directory );
					AddFile( directory, newGroup, TreeEnum.SOURCE_ROOT, createBuildFile );
					continue;
				}
				
				if( recursive ) {
					Debug.Log( "recursive" );
					AddFolder( directory, newGroup, exclude, recursive, createBuildFile );
				}
			}
			
			// Adding files.
			string regexExclude = string.Format( @"{0}", string.Join( "|", exclude ) );
			foreach( string file in Directory.GetFiles( folderPath ) ) {
				if( Regex.IsMatch( file, regexExclude ) ) {
					continue;
				}
				Debug.Log("Adding Files for Folder");
				AddFile( file, newGroup, TreeEnum.SOURCE_ROOT, createBuildFile );
			}
			
			modified = true;
			return modified;
		}

		// We support neither recursing into nor bundles contained inside loc folders
		public bool AddLocFolder( string folderPath, PBXGroup parent = null, string[] exclude = null, bool createBuildFile = true)
		{
			DirectoryInfo sourceDirectoryInfo = new DirectoryInfo( folderPath );

			if( exclude == null )
				exclude = new string[] {};
			
			if( parent == null )
				parent = rootGroup;

			// Create group as needed
			System.Uri projectFolderURI = new System.Uri( projectFileInfo.DirectoryName );
			System.Uri locFolderURI = new System.Uri( folderPath );
			var relativePath = projectFolderURI.MakeRelativeUri( locFolderURI ).ToString();
			PBXGroup newGroup = GetGroup( sourceDirectoryInfo.Name, relativePath, parent );

			// Add loc region to project
			string nom = sourceDirectoryInfo.Name;
			string region = nom.Substring(0, nom.Length - ".lproj".Length);
			project.AddRegion(region);
			
			// Adding files.
			string regexExclude = string.Format( @"{0}", string.Join( "|", exclude ) );
			foreach( string file in Directory.GetFiles( folderPath ) ) {
				if( Regex.IsMatch( file, regexExclude ) ) {
					continue;
				}

				// Add a variant group for the language as well
				var variant = new PBXVariantGroup(System.IO.Path.GetFileName( file ), null, "GROUP");
				variantGroups.Add(variant);

				// The group gets a reference to the variant, not to the file itself
				newGroup.AddChild(variant);

				AddFile( file, variant,TreeEnum.GROUP, createBuildFile );
			}
			
			modified = true;
			return modified;
		}		
		#endregion

		#region Getters
		public PBXFileReference GetFile( string name )
		{
			if( string.IsNullOrEmpty( name ) ) {
				return null;
			}
			
			foreach( KeyValuePair<string, PBXFileReference> current in fileReferences ) {
				if( !string.IsNullOrEmpty( current.Value.name ) && current.Value.name.CompareTo( name ) == 0 ) {
					return current.Value;
				}
			}
			
			return null;
		}
		
		public PBXGroup GetGroup( string name, string path = null, PBXGroup parent = null )
		{
			if( string.IsNullOrEmpty( name ) )
				return null;
			
			if( parent == null ) parent = rootGroup;
			
			foreach( KeyValuePair<string, PBXGroup> current in groups ) {
				if( string.IsNullOrEmpty( current.Value.name ) ) { 
					if( current.Value.path.CompareTo( name ) == 0 && parent.HasChild( current.Key ) ) {
						return current.Value;
					}
				} else if( current.Value.name.CompareTo( name ) == 0 && parent.HasChild( current.Key ) ) {
					return current.Value;
				}
			}

			PBXGroup result = new PBXGroup( name, path );
			groups.Add( result );
			parent.AddChild( result );
			
			modified = true;
			return result;
		}
			
		#endregion

		#region Mods		
		public void ApplyMod( string pbxmod )
		{
			XCMod mod = new XCMod( pbxmod );
			foreach(var lib in mod.libs){
				Debug.Log("Library: "+lib);
			}
			ApplyMod( mod );
		}

		struct FileRef{
			public string 	path;
			public TreeEnum	tree;
		}

		private FileRef GetFileRef(string path){
			FileRef xref;
			if(path.Contains("$UNITY_PROJ")){
				xref.tree = TreeEnum.ABSOLUTE;
				xref.path = path.Replace("$UNITY_PROJ",Application.dataPath);
			}else if(path.Contains("$XCODE_PROJ")){
				xref.tree = TreeEnum.SOURCE_ROOT;
				xref.path = path.Replace("$XCODE_PROJ/","");
			}else{
				xref.path = path;
				xref.tree = TreeEnum.NONE;
			}
			return xref;
		}
		
		public void ApplyMod( XCMod mod )
		{	
			PBXGroup modGroup = this.GetGroup( mod.group );
			if(mod.libs != null)
			{
				Debug.Log( "Adding libraries..." );
				foreach( XCModFile libRef in mod.libs ) {
					FileRef xref = GetFileRef(libRef.filePath);
					if(xref.tree == TreeEnum.NONE){
						string directory = System.IO.Path.GetDirectoryName(libRef.filePath);
						if(string.IsNullOrEmpty(directory)){
							xref.tree = TreeEnum.SDKROOT;
							xref.path = System.IO.Path.Combine( "usr/lib", libRef.filePath );
						}else{
							xref.tree = TreeEnum.ABSOLUTE;
							xref.path = System.IO.Path.Combine( Application.dataPath, libRef.filePath );
						}
					}
					Debug.Log ("Adding library " + xref.path+" tree:"+xref.tree);
					this.AddFile( xref.path, modGroup, xref.tree, true, libRef.isWeak );
				}
			}

			if(mod.frameworks != null)
			{
				Debug.Log( "Adding frameworks..." );
				PBXGroup frameworkGroup = this.GetGroup( "Frameworks" );
				foreach( string framework in mod.frameworks ) {
					string[] filename = framework.Split( ':' );
					bool isWeak = ( filename.Length > 1 ) ? true : false;

					FileRef xref = GetFileRef(filename[0]);
					if(xref.tree == TreeEnum.NONE){
						xref.tree = TreeEnum.SDKROOT;
						if(xref.path.EndsWith(".framework"))
							xref.path = System.IO.Path.Combine( "System/Library/Frameworks", filename[0] );
						else
							xref.path = System.IO.Path.Combine( "usr/lib", filename[0]);
					}
					this.AddFile( xref.path, frameworkGroup, xref.tree, true, isWeak );
				}
			}
			if(mod.files != null)
			{
				Debug.Log( "Adding files..." );
				foreach( string filePath in mod.files ) {
					FileRef xref = GetFileRef(filePath);
					if(xref.tree == TreeEnum.NONE){
						xref.tree = TreeEnum.ABSOLUTE;
					}
					this.AddFile( xref.path, modGroup,xref.tree);
				}
			}

			if(mod.supported_archs != null){
				//ex. (arm64,armv7,)
				overwriteBuildSetting("ARCHS", mod.supported_archs);
			}

			if(mod.code_sign_identity != null){
				//ex.CODE_SIGN_IDENTITY = "iPhone Developer: Florin Serban (BAYRZH8UCR)";
				overwriteBuildSetting("CODE_SIGN_IDENTITY", mod.code_sign_identity );
			}

			if(mod.code_sign_entitlements != null){
				//ex.CODE_SIGN_ENTITLEMENTS = Classes/fitkin.entitlements;
				overwriteBuildSetting("CODE_SIGN_ENTITLEMENTS", mod.code_sign_entitlements );
			}

			if(mod.debug_information_format != null){
				//ex.DEBUG_INFORMATION_FORMAT = dwarf;
				overwriteBuildSetting("DEBUG_INFORMATION_FORMAT", mod.debug_information_format );
			}

			if (mod.embed_binaries != null)
			{
				Debug.Log( "Adding embed binaries..." );
				//1. Add LD_RUNPATH_SEARCH_PATHS for embed framework
				this.overwriteBuildSetting("LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks", "Release");
				this.overwriteBuildSetting("LD_RUNPATH_SEARCH_PATHS", "$(inherited) @executable_path/Frameworks", "Debug");

				foreach( string binary in mod.embed_binaries ) {
					string absoluteFilePath = System.IO.Path.Combine( Application.dataPath/* mod.path*/, binary );
					this.AddEmbedFramework(absoluteFilePath);
				}
			}

			if(mod.folders != null)
			{
				Debug.Log( "Adding folders..." );
				foreach( string folderPath in mod.folders ) {
					FileRef xref = GetFileRef(folderPath);
					if(xref.tree == TreeEnum.NONE){
						xref.tree = TreeEnum.ABSOLUTE;
					}else if(xref.tree == TreeEnum.SOURCE_ROOT){
						xref.path = System.IO.Path.Combine(projectRootPath,xref.path);
					}
					//string absoluteFolderPath = System.IO.Path.Combine( Application.dataPath, folderPath );
					Debug.Log ("Adding folder " + xref.path);
					this.AddFolder( xref.path, modGroup, (string[])mod.excludes.ToArray( typeof(string) ) );
				}
			}

			if(mod.headerpaths != null)
			{
				Debug.Log( "Adding headerpaths..." );

				foreach( string headerpath in mod.headerpaths ) {
					if (headerpath.Contains("$(inherited)")) {
						Debug.Log ("not prepending a path to " + headerpath);
						this.AddHeaderSearchPaths( headerpath );
					} else {
						FileRef xref = GetFileRef(headerpath);
						if(xref.tree == TreeEnum.NONE){
							xref.tree = TreeEnum.ABSOLUTE;
						}
						//string absoluteHeaderPath = System.IO.Path.Combine( Application.dataPath/* mod.path*/, headerpath );
						this.AddHeaderSearchPaths( xref.path );
					}
				}
			}

			if(mod.compiler_flags != null)
			{
				Debug.Log( "Adding compiler flags...:"+mod+" comp flags:" + mod.compiler_flags);
				foreach( string flag in mod.compiler_flags ) {
					this.AddOtherCFlags( flag );
				}
			}

			if(mod.linker_flags != null)
			{
				Debug.Log( "Adding linker flags..." );
				foreach( string flag in mod.linker_flags ) {
					this.AddOtherLinkerFlags( flag );
				}
			}
			if(mod.plist != null)
			{
				Debug.Log ("Adding plist items...");
				string infoFilePath = this.projectRootPath + "/Info.plist";
				if (File.Exists(infoFilePath)) 
				{	
					XCPlist plist = new XCPlist (infoFilePath);
					plist.Process(mod.plist);
				}
			}
			this.Consolidate();
		}
		
		#endregion

		#region Savings	
		public void Consolidate()
		{
			PBXDictionary consolidated = new PBXDictionary();
			consolidated.Append<PBXBuildFile>( this.buildFiles );//sort!
			consolidated.Append<PBXCopyFilesBuildPhase>( this.copyBuildPhases );
			consolidated.Append<PBXFileReference>( this.fileReferences );//sort!
			consolidated.Append<PBXFrameworksBuildPhase>( this.frameworkBuildPhases );
			consolidated.Append<PBXGroup>( this.groups );//sort!
			consolidated.Append<PBXNativeTarget>( this.nativeTargets );
			consolidated.Add( project.guid, project.data );//TODO this should be named PBXProject?
			consolidated.Append<PBXResourcesBuildPhase>( this.resourcesBuildPhases );
			consolidated.Append<PBXShellScriptBuildPhase>( this.shellScriptBuildPhases );
			consolidated.Append<PBXSourcesBuildPhase>( this.sourcesBuildPhases );
			consolidated.Append<PBXVariantGroup>( this.variantGroups );
			consolidated.Append<XCBuildConfiguration>( this.buildConfigurations );
			consolidated.Append<XCConfigurationList>( this.configurationLists );
			_objects = consolidated;
			consolidated = null;
		}

		public void Backup()
		{
			string backupPath = Path.Combine( this.filePath, "project.backup.pbxproj" );
			
			// Delete previous backup file
			if( File.Exists( backupPath ) )
				File.Delete( backupPath );
			
			// Backup original pbxproj file first
			File.Copy( System.IO.Path.Combine( this.filePath, "project.pbxproj" ), backupPath );
		}

		private void DeleteExisting(string path)
		{
            // Delete old project file
            if( File.Exists( path ))
                File.Delete( path );
		}

		private void CreateNewProject(PBXDictionary result, string path) 
		{
			PBXParser parser = new PBXParser();
			StreamWriter saveFile = File.CreateText( path );
			saveFile.Write( parser.Encode( result, true ) );
			saveFile.Close();
		}
		
		/// <summary>
		/// Saves a project after editing.
		/// </summary>
		public void Save()
		{
			PBXDictionary result = new PBXDictionary();
			result.Add( "archiveVersion", 1 );
			result.Add( "classes", new PBXDictionary() );
			result.Add( "objectVersion", 46 );
			
			Consolidate();
			result.Add( "objects", _objects );
			
			result.Add( "rootObject", _rootObjectKey );
			
			string projectPath = Path.Combine( this.filePath, "project.pbxproj" );

			// Delete old project file, in case of an IOException 'Sharing violation on path Error'
			DeleteExisting(projectPath);

			// Parse result object directly into file
			CreateNewProject(result,projectPath);
		}
		
		/**
		* Raw project data.
		*/
		public Dictionary<string, object> objects {
			get {
				return null;
			}
		}
		#endregion

		public void Dispose()
   		{
   		
	   	}
	}
}
