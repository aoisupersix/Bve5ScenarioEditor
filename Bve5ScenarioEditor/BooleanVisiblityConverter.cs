using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows;

namespace Bve5ScenarioEditor
{
    /// <summary>
    /// リストビューのStyleに応じて表示を切り替える変換クラス
    /// </summary>
    public class BooleanVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool param = this.GetConverterParameter(parameter);
            bool selected = (bool)value;

            return param == selected ? Visibility.Visible : Visibility.Collapsed;
        }

        //---------------------------------------------------------------------------------------------
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Not Implemented");
        }

        //---------------------------------------------------------------------------------------------
        private bool GetConverterParameter(object parameter)
        {
            bool result = false;

            try
            {
                if (parameter != null)
                    result = System.Convert.ToBoolean(parameter);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return result;
        }
    }
}
