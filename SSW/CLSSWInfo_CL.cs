using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace SSW
{
#if _PROFILE_CL
	public class CLSSWInfo_CL
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
				return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Climalombarda s.r.l.<BR>"+ 
					"Strada provinciale 201, km 1<BR>" +
					"26833 Merlino (LO) Italy<BR>" +
					"phone +39 02 269 208 90<BR>" +
					"fax +39 02 700 579 76<BR>" +
				    "email: <A href=\"mailto:info@climalombarda.com\">info@climalombarda.com<A><BR>" + 
				    "<A href=\"http://www.climalombarda.com\">www.climalombarda.com</A></DIV>";
			}
		}

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "CL_icon.ico" ); }
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{

		}
	}
#endif
}
