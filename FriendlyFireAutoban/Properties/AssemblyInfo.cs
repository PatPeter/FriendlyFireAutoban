using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FriendlyFireAutoban;
using System.Linq;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(FriendlyFireAutoban.AssemblyInfo.Name + "_" + FriendlyFireAutoban.AssemblyInfo.Version)]
[assembly: AssemblyDescription(FriendlyFireAutoban.AssemblyInfo.Description)]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Universal Gaming Alliance")]
[assembly: AssemblyProduct(FriendlyFireAutoban.AssemblyInfo.Name)]
[assembly: AssemblyCopyright("Copyright © 2018 PatPeter")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ffa5d116-1a96-409f-b74f-6150e65bd59d")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
//(typeof(FriendlyFireAutobanPlugin).GetCustomAttributes(typeof(PluginDetails), true).FirstOrDefault() as PluginDetails).version
[assembly: AssemblyVersion(FriendlyFireAutoban.AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(FriendlyFireAutoban.AssemblyInfo.Version)]

namespace FriendlyFireAutoban
{
	static internal class AssemblyInfo
	{
		internal const string Author = "PatPeter";
		internal const string Name = "Friendly Fire Autoban";
		internal const string Description = "Plugin that autobans players for friendly firing.";
		internal const string Id = "patpeter.friendly.fire.autoban";
		internal const string ConfigPrefix = "friendly_fire_autoban";

		/// <summary>
		/// The AssemblyFileVersion of this web part
		/// </summary>
		internal const string Version = "6.3.0";
	}
}
