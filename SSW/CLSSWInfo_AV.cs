using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace SSW
{
#if _PROFILE_AV
	public class CLSSWInfo_AV
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
				return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Avensys s.r.l.<BR>"+ 
					"via Primo Maggio 10/12<BR>" +
					"26858 Sordio (LO) Italy<BR>" +
					"phone +39 02 4971 2802<BR>" +
				    "email: <A href=\"mailto:info@avensys-srl.com\">info@avensys-srl.com<A><BR>" + 
				    "<A href=\"http://www.avensys-solutions.com\">www.avensys-solutions.com</A></DIV>";
			}
		}

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "AV_icon.ico" ); }
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{

		}
	}
#endif
}
