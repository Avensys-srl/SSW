using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_FT
	public class CLSSWInfo_FT
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

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "FT_icon.ico" ); }
		}

		public override void PrepareEnvironment( CLEnvironment environment )
		{
		}

		public override string CustomerInfo
		{
			get
			{
				return "Felsinea Tech";
			}
		}
	}
#endif
}
