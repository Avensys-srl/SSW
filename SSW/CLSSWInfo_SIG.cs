using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SSW
{
#if _PROFILE_SIG
    public class CLSSWInfo_SIG
        : CLSSWInfo
    {
		public override string Code
		{
			get { return CLSSWProfile.Code; }
		}

        public override string DefaultLanguage
        {
            get { return CLEnvironment.LanguageCode_EN; }
        }

 		public override string SelectionSoftwareTitle
		{
			get { return CLSSWProfile.AssemblyTitle; }
		}

        public override string CustomerInfo
        {
            get
            {
				//if (CLEnvironment.Current.Branch != null
				//	&& CLEnvironment.Current.Branch.Code == "NL" && CLEnvironment.Current.Branch.ShortName == "IN")
				//{
			 //return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Inatherm BV<BR>" +
    //                "Tielenstraat 17<BR>" +
    //                "5145 RC Waalwijk<BR>" +
    //                "Nederland<BR>" +
    //                "Tel: 0416-317830<BR>" +
				//	"<a href=\"mailto:inatherm@sigairhandling.nl\">inatherm@sigairhandling.nl</a><BR>"+
    //                "<A href=\"http://www.inatherm.nl/\">www.inatherm.nl</A></DIV>";
				//}

				if (CLEnvironment.Current.Branch != null
					&& CLEnvironment.Current.Branch.Code == "FR")
				{
					
			 return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Ouest Ventil<BR>" +
                    "Z.I. de la Rangle<BR>" +
                    "27460 Alizay<BR>" +
                    "France<BR>" +
					"Tel: +33 02-32-98-30-00<BR>" +
					"Fax: +33 02-35-23-04-85<BR>" +
                    "<A href=\"http://www.ouestventil.fr/\">www.ouestventil.fr</A></DIV>";
				}

				if (CLEnvironment.Current.Branch != null
					&& CLEnvironment.Current.Branch.Code == "BE")
				{

			 return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Cairox Belgium<BR>" +
                    "Hoogstraat 180<BR>" +
                    "1930 Zaventem<BR>" +
                    "Belgium<BR>" +
                    "Tel: +32 2 725 31 80<BR>" +
                    "<a href=\"mailto:engineering@cairox.be\">engineering@cairox.be</a><BR>" +
                    "<A href=\"http://www.cairox.be/\">www.cairox.be</A></DIV>";		
				}

				if (CLEnvironment.Current.Branch != null
					&& CLEnvironment.Current.Branch.Code == "HU")
				{
			 return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Cairox Hungary Kft.<BR>" +
                    "2040 Budaörs<BR>" +
					"Gyár utca 2<BR>" +
                    "Hungary<BR>" +
                    "Tel: +36 23 444 133<BR>" +
					"Fax: +36 23 444 144<BR>" +
                    "<a href=\"mailto:info@cairox.hu\">info@cairox.hu</a><BR>" +
                    "<A href=\"http://www.cairox.hu/\">www.cairox.hu</A></DIV>";
				
				}

				if (CLEnvironment.Current.Branch != null
					&& CLEnvironment.Current.Branch.Code == "RO")
				{				
			 return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Cairox RO<BR>" +
					"Sos. Odaii 307-309, Sector 1<BR>" +
					"013604 Bucharest<BR>" +
					"România<BR>" +
					"Tel: +40(0)318242111<BR>" +
					"Fax: +40(0)318242112<BR>" +
                    "<a href=\"mailto:office@cairox.ro\">office@cairox.ro</a><BR>" +
                    "<A href=\"http://www.cairox.ro/\">www.cairox.ro</A></DIV>";
				}

				if (CLEnvironment.Current.Branch != null
					&& CLEnvironment.Current.Branch.Code == "BG")
				{
					
             return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Cairox Bulgaria Ltd.<BR>" +
					"301, Tzarigradsko chaussee Blvr.<BR>" +
					"Sofia 1582<BR>" +
					"Bulgaria<BR>" +
					"Tel: +359 (0)2 439 55 55<BR>" + 
					"<A href=\"mailto:atc.bulgaria@airtradecentre.com\">atc.bulgaria@airtradecentre.com</A></DIV>";
				}

				if (CLEnvironment.Current.Branch != null
					&& CLEnvironment.Current.Branch.Code == "PL")
				{

			 return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Cairox Poland Sp. z o.o.<BR>" +
					"ul. Kamieńskiego 51<BR>" +
					"30-644 Kraków<BR>" +
					"Poland<BR>" +
					"Tel: +48 12 254 59 01<BR>" +
                    "<a href=\"mailto:warszawa@sigairhandling.pl\">warszawa@cairox.pl</a><BR>" +
                    "<A href=\"http://www.cairox.pl/\">www.cairox.pl</A></DIV>";
				}

				if (CLEnvironment.Current.Branch != null
					&& CLEnvironment.Current.Branch.Code == "UK")
				{
			 return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">SK Sales<BR>" +
					"<A href=\"http://www.sksales.co.uk/\">www.sksales.co.uk</A></DIV>";
				}

				if (CLEnvironment.Current.Branch != null
					&& CLEnvironment.Current.Branch.Code == "IE")
				{

			 return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Cairox Ireland<BR>" +
					"Turnpike Road<BR>" +
					"Ballymount, Dublin 22<BR>" +
					"D22 P5R7<BR>"+
					"Ireland<BR>" +
					"Tel: 01 8951702<BR>" +
                    "<a href=\"mailto:enquiries@cairox.ie\">enquiries@cairox.ie</a><BR>" +
                    "<A href=\"https://www.cairox.ie/\">www.cairox.ie</A></DIV>";
				}
				
				if (CLEnvironment.Current.Branch != null
					&& CLEnvironment.Current.Branch.Code == "CH")
				{

			 return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Multijoint<BR>" +
					"Route des Jeunes 6<BR>" +
					"1227  Carouge / GE<BR>" +
					"Tel: 022 782 51 44<BR>" +
					"<a href=\"mailto:info@multijoint.ch\">info@multijoint.ch</a><BR>"+
                    "<A href=\"http://www.multijoint.ch/\">www.multijoint.ch</A></DIV>";
				}

                //if (CLEnvironment.Current.Branch != null
                //	&& CLEnvironment.Current.Branch.Code == "NL" )
                //{

                //return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Cairox Centre<BR>" +
                //	"Postbus 59<BR>" +
                //	"2910 AB Nieuwerkerk a/d IJssel<BR>" +
                //	"Tel: 088 0318500<BR>" +
                //	"<a href=\"mailto:info@cairoxcentre.nl\">info@cairoxcentre.nl</a><BR>"+
                //                "<A href=\"https://www.cairoxcentre.nl/\">www.cairoxcentre.nl</A></DIV>";
                //}

                //
                //return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Barcol-Air SIG AH<BR>" +
                //                "Cantekoogweg 10-12<BR>" +
                //                "1442 LG Purmerend<BR>" +
                //                "The Netherlands<BR>" +
                //                "Tel: +31 (0)299 689 300<BR>" +
                //	"<a href=\"mailto:barcol-air@sigairhandling.nl\">barcol-air@sigairhandling.nl</a><BR>"+
                //                "<A href=\"https://www.barcol-air.nl/\">www.barcol-air.nl</A></DIV>";

                // Default
                return "<DIV style=\"font-family: 'MS Sans Serif'; font-size: 9pt;\">Cairox Belgium<BR>" +
                   "Hoogstraat 180<BR>" +
                   "1930 Zaventem<BR>" +
                   "Belgium<BR>" +
                   "Tel: +32 2 725 31 80<BR>" +
                   "<a href=\"mailto:engineering@cairox.be\">engineering@cairox.be</a><BR>" +
                   "<A href=\"http://www.cairox.be/\">www.cairox.be</A></DIV>";

            }
        }

		public override System.Drawing.Icon EmbeddedIcon
		{
			get { return GetResourceIcon( "CAIROX.ico" ); }
		}

        public override void PrepareEnvironment(CLEnvironment environment)
        {
			//environment.AddBranch( new CLCustomerBranch( "NL", "Inatherm SIG AH", "IN" ) );
			environment.AddBranch( new CLCustomerBranch( "FR", "Ouest Ventil", "OV" ) );
			environment.AddBranch( new CLCustomerBranch( "*", "Cairox Belgium", "SIGBE" ) );
			environment.AddBranch( new CLCustomerBranch( "HU", "Cairox Hungary", "SIGHU" ) );
			environment.AddBranch( new CLCustomerBranch( "RO", "Cairox Romania", "SIGRO" ) );
			environment.AddBranch( new CLCustomerBranch( "BG", "Cairox Bulgaria", "SIGBG" ) );
			environment.AddBranch( new CLCustomerBranch( "PL", "Cairox Poland", "SIGPL") );
			environment.AddBranch( new CLCustomerBranch( "IE", "Cairox Ireland", "SIGIR") );
			environment.AddBranch( new CLCustomerBranch( "UK", "SK Sales" , "SIGSK") );
			environment.AddBranch( new CLCustomerBranch( "CH", "Multijoint" , "SIGMJ") );
			//environment.AddBranch( new CLCustomerBranch( "NL", "Cairox Centre" , "SIGCX") );
			//environment.AddBranch( new CLCustomerBranch( "*",  "Barcol-Air SIG AH", "BA" ) );

			string	country		= CultureInfo.CurrentCulture.Name.Split( '-' )[ 1 ];
			string	language	= CultureInfo.CurrentCulture.Name.Split( '-' )[ 0 ];


			if (string.Compare( country, "NL", StringComparison.CurrentCultureIgnoreCase ) == 0)
				environment.Branch	= environment.Branchs.FirstOrDefault( c => c.Code == "NL" );
			else
			if (string.Compare( country, "FR", StringComparison.CurrentCultureIgnoreCase ) == 0)
				environment.Branch	= environment.Branchs.FirstOrDefault( c => c.Code == "FR" );
			else
			if (string.Compare( country, "BE", StringComparison.CurrentCultureIgnoreCase ) == 0)
				environment.Branch	= environment.Branchs.FirstOrDefault( c => c.Code == "BE" );
			else
			if (string.Compare( country, "HU", StringComparison.CurrentCultureIgnoreCase ) == 0)
				environment.Branch	= environment.Branchs.FirstOrDefault( c => c.Code == "HU" );
			else
			if (string.Compare( country, "BG", StringComparison.CurrentCultureIgnoreCase ) == 0)
				environment.Branch	= environment.Branchs.FirstOrDefault( c => c.Code == "BG" );
			else
			if (string.Compare( country, "PL", StringComparison.CurrentCultureIgnoreCase ) == 0)
				environment.Branch	= environment.Branchs.FirstOrDefault( c => c.Code == "PL" );
			else
				environment.Branch	= environment.Branchs.FirstOrDefault( c => c.Code == "*" );
        }
    }
#endif
}
