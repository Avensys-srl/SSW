using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Xml.Serialization;

namespace ClimaLombarda.Common
{
	public class CLLocalizationSupport
	{
		private static Dictionary<string, ResourceManager> m_Resources = new Dictionary<string, ResourceManager>();

		public static string[] CultureCodes
		{
			get { return m_Resources.Keys.ToArray(); }
		}

		public static ResourceManager GetResource( string cultureCode )
		{
			return m_Resources[ cultureCode ];
		}

		public static ResourceManager AddResource( string cultureCode, string assemblyPath, string baseName )
		{
			Assembly		assembly;
			ResourceManager	resourceManager;

			assembly		= Assembly.LoadFile( assemblyPath );
			resourceManager	= new ResourceManager( baseName, assembly );

			m_Resources.Add( cultureCode, resourceManager );

			return resourceManager;
		}

		private static ResourceManager m_PrimaryResource = null;
		public static ResourceManager PrimaryResource
		{
			get { return m_PrimaryResource; }
			set { m_PrimaryResource = value; }
		}

		private static ResourceManager m_SecondaryResource = null;
		public static ResourceManager SecondaryResource
		{
			get { return m_SecondaryResource; }
			set { m_SecondaryResource = value; }
		}

		private static string _GetString( ResourceManager primaryResource,
			ResourceManager secondaryResource,
			string stringId )
		{
			object	resource;
			string	textString;

			resource	= primaryResource.GetObject( stringId );
			if (resource == null)
			{
				if (secondaryResource == null)
					return "";

				textString	= secondaryResource.GetString( stringId );
				if (textString == null)
					textString	= "?";
			}
			else
				textString	= (string) resource;

			while (textString.StartsWith( "@@" ))
			{
				if (textString.Length <= 2)
					return "";

				return _GetString( primaryResource, secondaryResource, textString.Substring( 2 ) );
			}

			return textString;
		}

		public static string GetString( string primaryCultureCode, string stringId )
		{
			ResourceManager	primaryResourceManager; 

			if (!m_Resources.TryGetValue( primaryCultureCode, out primaryResourceManager )
				|| primaryResourceManager == null)
				return string.Format( "Culture '{0}' not found.", primaryCultureCode );

			return _GetString( primaryResourceManager, m_SecondaryResource, stringId );
		}

		public static string GetString( string primaryCultureCode,
			string secondaryCultureCode,
			string stringId )
		{
			ResourceManager	primaryResourceManager; 
			ResourceManager	secondaryResourceManager; 

			if (!m_Resources.TryGetValue( primaryCultureCode, out primaryResourceManager )
				|| primaryResourceManager == null)
				return string.Format( "Culture '{0}' not found.", primaryCultureCode );

			m_Resources.TryGetValue( secondaryCultureCode, out secondaryResourceManager );

			return _GetString( primaryResourceManager, secondaryResourceManager, stringId );
		}

		public static string GetString( string stringId )
		{
			if (m_PrimaryResource == null)
				return "PrimaryCulture not set.";

			return _GetString( m_PrimaryResource, m_SecondaryResource, stringId );
		}
	}
}
