using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ClimaLombarda.Common.UI
{
	public enum CLPanelTitleType
	{
		None,
		Box
	}

	public partial class CLPanel
		: Panel
	{
		public CLPanel()
		{
			InitializeComponent();
		}

		protected override void OnPaint( PaintEventArgs eventArgs )
		{
			if (BorderStyle == BorderStyle.None)
			{
				Rectangle	rectangle	= ClientRectangle;

			    ControlPaint.DrawBorder3D( eventArgs.Graphics, rectangle, m_Border3DStyle, Border3DSide.All );
				if (m_Border3DStyle != Border3DStyle.Adjust)
					rectangle.Inflate( -SystemInformation.Border3DSize.Width, -SystemInformation.Border3DSize.Height );
				eventArgs.Graphics.FillRectangle( new SolidBrush( BackColor ), rectangle );
			}
			base.OnPaint( eventArgs );
		}

		private Border3DStyle m_Border3DStyle;
		public Border3DStyle Border3DStyle
		{
			get { return m_Border3DStyle; }
			set
			{
				m_Border3DStyle	= value;
				Invalidate();
			}
		}

		public override Rectangle DisplayRectangle
		{
			get
			{
				if (BorderStyle != BorderStyle.None || (BorderStyle == BorderStyle.None && 
					m_Border3DStyle == Border3DStyle.Adjust))
				{
					return base.DisplayRectangle;
				}

				Rectangle	displayRectangle	= base.DisplayRectangle;
				displayRectangle.Inflate( -SystemInformation.Border3DSize.Width, -SystemInformation.Border3DSize.Height );
				return displayRectangle;
			}
		}

	}
}
