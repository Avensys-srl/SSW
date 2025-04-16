using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_FS
	public class CLSSWInfo_FS
		: CLSSWInfo
	{
		public override string Code
		{
			get { return CLSSWProfile.Code; }
		}

		public override string DefaultLanguage
		{
			get { return CLEnvironment.LanguageCode_PL; }
		}

 		public override string SelectionSoftwareTitle
		{
			get { return CLSSWProfile.AssemblyTitle; }
		}
		
		public override string CustomerInfo
		{
			get
			{
				return "<DIV style=\"font-family: 'Arial'; font-size: 9pt;\">FLOP SYSTEM sp. z o.o.<BR>"+ 
					"ul. Kiełczowska 64<BR>" +
					"51-315 Wrocław<BR>" +
					"tel./faks: +48 71 325 14 20<BR>" +
				    "email: <A href=\"mailto:biuro@flopsystem.pl\">biuro@flopsystem.pl<A><BR>" + 
				    "<A href=\"http://www.flopsystem.pl\">www.flopsystem.pl</A></DIV>";
			}
		}

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "FS_icon.ico" ); }
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{

			// Di default disabilitiamo tutte le lingue
			environment.DisableAllLanguages();

			// Abilitiamo solo le lingue richieste
			environment.FindLanguage( CLEnvironment.LanguageCode_EN ).Enabled	= true;
			environment.FindLanguage( CLEnvironment.LanguageCode_PL ).Enabled	= true;
		}
	}
#endif
}
