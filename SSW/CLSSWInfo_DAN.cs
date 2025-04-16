using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_DAN
	public class CLSSWInfo_DAN
		: CLSSWInfo
	{
		public override string Code
		{
			get { return CLSSWProfile.Code; }
		}

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "DAN_icon.ico" ); }
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
				return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Dan-Poltherm<BR>" +
					"Rusocin, ul. Gdańska 12<BR>" +
					"83-031 Łęgowo<BR>" +
					"<A href=\"http://www.dan-poltherm.pl/\">www.dan-poltherm.pl</A></DIV>";
			}
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{
		}
	}
#endif
}
