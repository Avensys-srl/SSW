using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_CV
	public class CLSSWInfo_CV
		: CLSSWInfo
	{
		public override string Code
		{
			get { return CLSSWProfile.Code; }
		}

		public override string DefaultLanguage
		{
			get { return CLEnvironment.LanguageCode_NL; }
		}

 		public override string SelectionSoftwareTitle
		{
			get { return CLSSWProfile.AssemblyTitle; }
		}

		public override string CustomerInfo
		{
			get
			{
				return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Climavent bvba<BR>" +
					"Dirk Martensstraat 2/10<BR>" +
					"8200 Brugge <BR>" +
					"phone 050 32 30 05<BR>" +
					"fax 050 31 30 06<BR>" +
					"email: <A href=\"mailto:info@climavent.be\">info@climavent.be<A><BR>" +
					"<A href=\"http://climavent.be/\">climavent.be</A></DIV>";
			}
		}

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "CV_icon.ico" ); }
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{
		}
	}
#endif
}
