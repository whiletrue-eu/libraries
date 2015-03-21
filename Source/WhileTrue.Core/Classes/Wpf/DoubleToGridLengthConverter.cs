﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WhileTrue.Classes.Wpf
{
    public class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,object parameter, CultureInfo culture)
        {
            return new GridLength((double)value);
        }

        public object ConvertBack(object value, Type targetType,object parameter, CultureInfo culture)
        {
            return ((GridLength)value).Value;
        }
    }
}