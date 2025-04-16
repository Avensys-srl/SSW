using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_FA
	public class CLSSWInfo_FA
		: CLSSWInfo
	{
		public override string Code
		{
			get { return CLSSWProfile.Code; }
		}

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "FA_icon.ico" ); }
		}

		public override string DefaultLanguage
		{
			get { return CLEnvironment.LanguageCode_FR; }
		}

 		public override string SelectionSoftwareTitle
		{
			get { return CLSSWProfile.AssemblyTitle; }
		}

		public override string CustomerInfo
		{
			get
			{
				return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">FRANCE AIR BEYNOST<BR>" +
					"Rue des Barronières<BR>" +
					"01794 Beynost FRANCE<BR>" +
					"<A href=\"http://www.france-air.com/\">www.france-air.com</A></DIV>";
			}
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{
		}
	}
#endif
}
