using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace SSW
{
#if _PROFILE_SKL
	public class CLSSWInfo_SKL
		: CLSSWInfo
	{
		public override string Code
		{
			get { return CLSSWProfile.Code; }
		}

		public override string DefaultLanguage
		{
			get { return CLEnvironment.LanguageCode_DE; }
		}

 		public override string SelectionSoftwareTitle
		{
			get { return CLSSWProfile.AssemblyTitle; }
		}

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "SKL_icon.ico" ); }
		}

		public override string CustomerInfo
		{
			get
			{
				return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">S-Klima<BR>"+ 
						"Holsteiner Chaussee 283<BR>" +
						"22457 Hamburg, Germania<BR>" +
						"phone +49 40 55850<BR>" + 
						"<A href=\"https://www.s-klima.de\">www.s-klima.de</A></DIV>";
			}
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{
		}
	}
#endif
}
