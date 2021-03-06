﻿/**
 * MetroFramework - Modern UI for WinForms
 * 
 * The MIT License (MIT)
 * Copyright (c) 2011 Sven Walter, http://github.com/viperneo
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in the 
 * Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Creek.UI.Metro.Components;
using Creek.UI.Metro.Drawing;
using Creek.UI.Metro.Drawing.Html;
using Creek.UI.Metro.Interfaces;
using MetroFramework;

namespace Creek.UI.Metro.Controls
{

    #region Enums

    public enum MetroTilePartContentType
    {
        Text,
        Image,
        Html
    }

    #endregion

    [ToolboxItem(false)]
    public class MetroTilePart : Control, IMetroControl
    {
        #region Interface

        private MetroColorStyle metroStyle = MetroColorStyle.Default;
        private MetroThemeStyle metroTheme = MetroThemeStyle.Default;

        public event EventHandler<MetroPaintEventArgs> CustomPaintBackground;
        public event EventHandler<MetroPaintEventArgs> CustomPaint;
        public event EventHandler<MetroPaintEventArgs> CustomPaintForeground;

        [Category(MetroDefaults.PropertyCategory.Appearance)]
        [DefaultValue(MetroColorStyle.Default)]
        public MetroColorStyle Style
        {
            get
            {
                if (DesignMode || metroStyle != MetroColorStyle.Default)
                {
                    return metroStyle;
                }

                if (StyleManager != null && metroStyle == MetroColorStyle.Default)
                {
                    return StyleManager.Style;
                }
                if (StyleManager == null && metroStyle == MetroColorStyle.Default)
                {
                    return MetroDefaults.Style;
                }

                return metroStyle;
            }
            set { metroStyle = value; }
        }

        [Category(MetroDefaults.PropertyCategory.Appearance)]
        [DefaultValue(MetroThemeStyle.Default)]
        public MetroThemeStyle Theme
        {
            get
            {
                if (DesignMode || metroTheme != MetroThemeStyle.Default)
                {
                    return metroTheme;
                }

                if (StyleManager != null && metroTheme == MetroThemeStyle.Default)
                {
                    return StyleManager.Theme;
                }
                if (StyleManager == null && metroTheme == MetroThemeStyle.Default)
                {
                    return MetroDefaults.Theme;
                }

                return metroTheme;
            }
            set { metroTheme = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MetroStyleManager StyleManager { get; set; }

        public bool UseCustomBackColor { get; set; }
        public bool UseCustomForeColor { get; set; }
        public bool UseStyleColors { get; set; }
        public bool UseSelectable { get; set; }

        #endregion

        #region Fields

        private MetroTilePartContentType partContentType = MetroTilePartContentType.Text;

        private string partHtmlContent = "";

        [DefaultValue(MetroTilePartContentType.Text)]
        [Category(MetroDefaults.PropertyCategory.Behaviour)]
        public MetroTilePartContentType ContentType
        {
            get { return partContentType; }
            set { partContentType = value; }
        }

        [DefaultValue("")]
        [Category(MetroDefaults.PropertyCategory.Appearance)]
        public string HtmlContent
        {
            get { return partHtmlContent; }
            set { partHtmlContent = value; }
        }

        #endregion

        #region Constructor

        public MetroTilePart()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Dock = DockStyle.Fill;
        }

        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (partContentType == MetroTilePartContentType.Html)
            {
                HtmlRenderer.Render(e.Graphics, partHtmlContent, ClientRectangle, true);
            }
        }

        #endregion
    }
}