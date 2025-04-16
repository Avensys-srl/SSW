using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSW;
using System.Reflection;
using System.Drawing;

namespace SSW
{
#if _PROFILE_AC
	public class CLSSWInfo_AC
		: CLSSWInfo
	{
		public override string Code
		{
			get { return CLSSWProfile.Code; }
		}

		public override string DefaultLanguage
		{
			get { return CLEnvironment.LanguageCode_IT; }
		}

		public override string SelectionSoftwareTitle
		{
			get { return CLSSWProfile.AssemblyTitle; }
		}

		public override string CustomerInfo
		{
			get
			{
				return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Air Car S.r.l.<BR>"+ 
					"Strada Piozzo, 5<BR>" +
					"12061 Carrù (CN) ITALIA<BR>" +
					"phone +39 0173 75 09 42<BR>" +
					"fax +39 0173 75 90 35<BR>" +
				    "email: <A href=\"mailto:info@aircar.it\">info@aircar.it<A><BR>" + 
				    "<A href=\"http://www.aircar.it/\">www.aircar.it</A></DIV>";
			}
		}

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "AC_icon.ico" ); }
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{
		}
	}
#endif
}
