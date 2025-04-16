using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace SSW
{
	public static class CLSSWProfile
	{
#if _PROFILE_AC
			public const string Code = "036052";
			public const string ShortName = "AC";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_AC);
			public const string AssemblyTitle = "Air Car Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Air Car S.r.l.";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_AL
			public const string Code = "036167";
			public const string ShortName = "AL";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_AL);
			public const string AssemblyTitle = "Allvotech AG Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Allvotech AG";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_CL
			public const string Code = "035889";
			public const string ShortName = "CL";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_CL);
			public const string AssemblyTitle = "Climalombarda Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Climalombarda";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_AV
			public const string Code = "035889";
			public const string ShortName = "AV";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_AV);
			public const string AssemblyTitle = "Avensys Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Avensys";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_CV
			public const string Code = "036071";
			public const string ShortName = "CV";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_CV);
			public const string AssemblyTitle = "Climavent Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Climavent bvba";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_DAN
			public const string Code = "036149";
			public const string ShortName = "DAN";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_DAN);
			public const string AssemblyTitle = "Dan-Poltherm Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Dan-Poltherm";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_FA
			public const string Code = "035498";
			public const string ShortName = "FA";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_FA);
			public const string AssemblyTitle = "France Air Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "France Air";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_FAI
			public const string Code = "035833";
			public const string ShortName = "FAI";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_FAI);
			public const string AssemblyTitle = "France Air Italia Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "France Air Italia";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_FS
			public const string Code = "000139";
			public const string ShortName = "FS";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_FS);
			public const string AssemblyTitle = "Flop System Program Doboru REKU";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Flop System";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_FT
			public const string Code = "035861";
			public const string ShortName = "FT";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_FT);
			public const string AssemblyTitle = "Felsinea Tech Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Felsinea Tech";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_NL
			public const string Code = "000295";
			public const string ShortName = "NL";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_NL);
			public const string AssemblyTitle = "Nordluft Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Nordluft";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_SU
			public const string Code = "036191";
			public const string ShortName = "SU";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_SU);
			public const string AssemblyTitle = "Sunwood Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Sunwood Srl";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_WE
			public const string Code = "036176";
			public const string ShortName = "WE";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_WE);
			public const string AssemblyTitle = "Weger Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Weger";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_SIG
			public const string Code = "036186";
			public const string ShortName = "SIG";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_SIG);
			public const string AssemblyTitle = "CAIROX Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "CAIROX";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#elif _PROFILE_IN
			public const string Code = "036251";
			public const string ShortName = "IN";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_IN);
			public const string AssemblyTitle = "Inatherm BV Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "Inatherm BV";
			public const string AssemblyCopyright = "© 2020 " + AssemblyCompany;
#elif _PROFILE_SKL
			public const string Code = "040001";
			public const string ShortName = "SKL";
			public static Type SSWInfoClassType = typeof(CLSSWInfo_SKL);
			public const string AssemblyTitle = "S-Klima Selection Software";
			public const string AssemblyProduct = AssemblyTitle;
			public const string AssemblyCompany = "S-Klima";
			public const string AssemblyCopyright = "© 2019 " + AssemblyCompany;
#else
#pragma error
#endif
    }
}

namespace SSW
{

	static class CLProgram
	{
		/// <summary>
		/// Punto di ingresso principale dell'applicazione.
		/// </summary>
		[STAThread]
		static int Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				string	sswDCLitePath	= Path.Combine( Path.GetDirectoryName( Application.ExecutablePath ), "data", "DataCentral.sdf" );
			
				if (Debugger.IsAttached)
				{
					if (Environment.MachineName == "ENEA-PC")
						sswDCLitePath	= Path.Combine( @"D:\job\Climalombarda\shared\SSWDCLite",
							string.Format("{0}-SSWDataCentral.sdf", CLSSWProfile.Code));
					else
						sswDCLitePath	= Path.Combine( @"\\server_av\TECHNO\TECHNO_SOFT\App\Shared\SSWDCLite",
							string.Format("{0}-SSWDataCentral.sdf", CLSSWProfile.Code));
				}



				CLEnvironment.Current	= new CLEnvironment(
					sswDCLitePath,
					Activator.CreateInstance( CLSSWProfile.SSWInfoClassType ) as CLSSWInfo );

				Application.Run( new CLMainForm() );
				
				return 0;
			}
			catch (Exception exception)
			{
				StringBuilder	message				= new StringBuilder();
				Exception		currentException	= exception;

				while (currentException != null)
				{
					message.AppendLine( currentException.Message );
					currentException	= currentException.InnerException;
				}
				MessageBox.Show( message.ToString(), "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return -1;
			}
		}

		private static void ExportLanguages( string directory )
		{
			string[]	resourceNames	= Enum.GetNames( typeof(CLMessageResources) )
				.OrderBy( c => c )
				.ToArray();

			foreach (CLLanguage language in CLEnvironment.Current.Languages)
			{
				Assembly		assembly;
				ResourceManager	resourceManager;
				string			csvFilePath;

				assembly		= Assembly.LoadFile( Path.Combine( Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location ), string.Format( @"{0}\{1}.resources.dll", language.Code, "SSWLib" ) ) );
				resourceManager	= new ResourceManager( String.Format( "SSW.Resources.{0}", language.Code ), assembly );
	
				csvFilePath	= Path.Combine( directory, string.Format( "SSW-Language-{0}.csv", language.Code ) );
				if (File.Exists( csvFilePath ))
					File.Delete( csvFilePath );

				using (StreamWriter streamWriter = new StreamWriter( csvFilePath, false, Encoding.UTF8 ))
				{
					streamWriter.WriteLine( "ID;{0}", language.Name );

					foreach (string resourceName in resourceNames)
					{
						object	resource;

						if (resourceName == CLMessageResources.AboutBoxForm_AirflowPressure.ToString()
							|| resourceName == CLMessageResources.AboutBoxForm_Title.ToString()
							|| resourceName == CLMessageResources.AboutBoxForm_ElectricPowerInput.ToString()
							|| resourceName == CLMessageResources.AboutBoxForm_ExternalLeakage.ToString()
							|| resourceName == CLMessageResources.AboutBoxForm_InternalLeakage.ToString()
							|| resourceName == CLMessageResources.AboutBoxForm_NoiseLevel.ToString()
							|| resourceName == CLMessageResources.Glic_Etil.ToString()
							|| resourceName == CLMessageResources.Glic_Prop.ToString()
							|| resourceName == CLMessageResources.Water.ToString())
						{
							continue;
						}

						resource	= resourceManager.GetObject( resourceName );
						streamWriter.WriteLine( "{0};{1}", resourceName, resource != null ? (string) resource : "" );
					}
				}
			}
		}
	}
}
