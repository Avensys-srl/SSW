using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_NL
	public class CLSSWInfo_NL
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
			get { return GetResourceIcon( "NL_icon.ico" ); }
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{
		}

		public override string CustomerInfo
		{
			get
			{
				return "<DIV style=\"font-family: Poppy, 'MS Sans Serif'; font-size: 14pt;\">nordluft</div>" +
				    "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Wärme-und Lüftungstechnik" +
				    "GmbH&Co KG<BR>" + 
				    "Robert Bosch Str. 5<BR>" + 
				    "49393 Lohne<BR>" + 
				    "tel. +49 (0) 4442 889 0<BR>" + 
				    "fax +49 (0) 4442 889 59<BR>" + 
				    "email: <A href=\"mailto:info@nordluft.com\">info@nordluft.com<A><BR>" + 
				    "<A href=\"http://www.nordluft.com\">www.nordluft.com</A></DIV>";

			}
		}
	}
#endif
}
