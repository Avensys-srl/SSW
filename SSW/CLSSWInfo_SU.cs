using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_SU
    public class CLSSWInfo_SU
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
                return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Sunwood Srl<BR>" +
                    "Viale del lavoro, 18<BR>" +
                    "37069 Villafranca di Verona (VR)<BR>" +
                    "Tel. 045 790 3582<BR>" +
                    "<A href=\"http://www.sunwoodsrl.it/\">www.sunwoodsrl.it</A></DIV>";
            }
        }

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "SU_icon.ico" ); }
		}

        public override void PrepareEnvironment(CLEnvironment environment)
        {
        }
    }
#endif
}
