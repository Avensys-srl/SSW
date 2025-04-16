using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_AL
	public class CLSSWInfo_AL
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

        public override string CustomerInfo
        {
            get
            {
                return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Allvotech AG<BR>" +
                    "Grossmattstrasse 8<BR>" +
                    "8964 Rudolfstetten<BR>" +
                    "056 418 35 35<BR>" +
                    "<A href=\"mailto:info@allvotech.ch\">info@allvotech.ch<A><BR>" +
                    "<A href=\"http://www.allvotech.ch/\">www.allvotech.ch</A></DIV>";
            }
        }

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "AL_icon.ico" ); }
		}

        public override void PrepareEnvironment(CLEnvironment environment)
        {
        }
    }
#endif
}
