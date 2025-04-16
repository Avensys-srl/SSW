using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_WE
    public class CLSSWInfo_WE
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
                return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Weger Walter GmbH/Srl<BR>" +
                    "Handwerkerzone 5/Zona Artigianale 5<BR>" +
                    "39030 Kiens/Ehrenburg - Chienes/Casteldarne<BR>" +
                    "0474 565253<BR>" +
                    "<A href=\"http://www.weger.it/\">www.weger.it</A></DIV>";
            }
        }

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "WE_icon.ico" ); }
		}

        public override void PrepareEnvironment(CLEnvironment environment)
        {
        }
    }
#endif
}
