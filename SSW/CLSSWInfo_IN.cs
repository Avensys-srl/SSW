using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace SSW
{
#if _PROFILE_IN
	public class CLSSWInfo_IN
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
                return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Inatherm BV<BR>" +
                                "Tielenstraat 17<BR>" +
                                "5145 RC Waalwijk<BR>" +
                                "Nederland<BR>" +
                                "Tel: 0416-317830<BR>" +
                    "<a href=\"mailto:inatherm@sigairhandling.nl\">inatherm@sigairhandling.nl</a><BR>" +
                                "<A href=\"http://www.inatherm.nl/\">www.inatherm.nl</A></DIV>";
            }
        }

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "IN_icon.ico" ); }
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{

		}
	}
#endif
}
