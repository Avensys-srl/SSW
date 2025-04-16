using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClimaLombarda.Common.UI
{

	public class CLWrapperComboBox<ValueType>
	{
		public CLWrapperComboBox( ValueType value, string text )
		{
			m_Value	= value;
			m_Text	= text;
		}

		private ValueType m_Value;
		public ValueType Value
		{
			get { return m_Value; }
		}

		private string m_Text;
		public string Text
		{
			get { return m_Text; }
		}

		public override string ToString()
		{
			return m_Text;
		}
	}
}
