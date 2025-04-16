using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CLCommon.CLSWRepositoryService;

namespace ClimaLombarda.Updater
{
	public class CLSWUpdater
	{
		private CLSWProductInfo m_SWProductInfo = null;
		public CLSWProductInfo SWProductInfo
		{
			get { return m_SWProductInfo; }
		}

		public static CLSWProductInfo GetSWProductInfo( string swProductId )
		{
			CLSWRepositoryServiceClient	service	= null;

			try
			{
				service	= new CLSWRepositoryServiceClient();
				service.Open();

				return service.GetProductInfo( swProductId );
			}
			finally
			{
				if (service != null && service.State == System.ServiceModel.CommunicationState.Opened)
					service.Close();
			}
		}

		public static CLSWProductPackage GetSWProductPackage( string swProductId )
		{
			CLSWRepositoryServiceClient	service	= null;

			try
			{
				service	= new CLSWRepositoryServiceClient();
				service.Open();

				return service.GetProductPackage( swProductId );
			}
			finally
			{
				if (service != null && service.State == System.ServiceModel.CommunicationState.Opened)
					service.Close();
			}
		}

		public bool IsNewVersionAvailable( string swProductId, Version version )
		{
			try
			{
				m_SWProductInfo	= GetSWProductInfo( swProductId );

				return (m_SWProductInfo != null && m_SWProductInfo.Version > version);
			}
			catch
			{
				m_SWProductInfo	= null;
				throw;
			}
		}

		public void Update()
		{
			CLSWProductPackage	swProductPackage	= null;

			if (m_SWProductInfo == null)
				return;

			swProductPackage	= GetSWProductPackage( m_SWProductInfo.Id );

			if (swProductPackage == null)
				return;
		}
	}
}
