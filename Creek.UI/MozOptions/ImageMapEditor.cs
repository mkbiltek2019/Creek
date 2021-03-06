using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Creek.UI.MozOptions
{
    /// <summary>
    /// Summary description for ImageEditor.
    /// </summary>
    public class ImageMapEditor : UITypeEditor
    {
        #region properties

        private ImageListPanel m_imagePanel;
        private int m_selectedIndex = -1;
        private IWindowsFormsEditorService wfes;

        #endregion

        #region constructor

        #endregion

        #region Methods

        protected virtual ImageList GetImageList(object component)
        {
            if (component is MozItem.ImageCollection)
            {
                return ((MozItem.ImageCollection) component).GetImageList();
            }

            return null;
        }

        #endregion

        #region overrides

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            wfes = (IWindowsFormsEditorService) provider.GetService(typeof (IWindowsFormsEditorService));
            if ((wfes == null) || (context == null))
                return null;

            ImageList imageList = GetImageList(context.Instance);
            if ((imageList == null) || (imageList.Images.Count == 0))
                return -1;

            m_imagePanel = new ImageListPanel();

            m_imagePanel.BackgroundColor = Color.FromArgb(241, 241, 241);
            m_imagePanel.BackgroundOverColor = Color.FromArgb(102, 154, 204);
            m_imagePanel.HLinesColor = Color.FromArgb(182, 189, 210);
            m_imagePanel.VLinesColor = Color.FromArgb(182, 189, 210);
            m_imagePanel.BorderColor = Color.FromArgb(0, 0, 0);
            m_imagePanel.EnableDragDrop = true;
            m_imagePanel.Init(imageList, 12, 12, 6, (int) value);

            // add listner for event
            m_imagePanel.ItemClick += OnItemClicked;

            // set m_selectedIndex to -1 in case the dropdown is closed without selection
            m_selectedIndex = -1;
            // show the popup as a drop-down
            wfes.DropDownControl(m_imagePanel);

            // return the selection (or the original value if none selected)
            return (m_selectedIndex != -1) ? m_selectedIndex : (int) value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context != null && context.Instance != null)
            {
                return UITypeEditorEditStyle.DropDown;
            }
            return base.GetEditStyle(context);
        }


        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs pe)
        {
            int imageIndex = -1;
            // value is the image index
            if (pe.Value != null)
            {
                try
                {
                    imageIndex = Convert.ToUInt16(pe.Value.ToString());
                }
                catch
                {
                }
            }
            // no instance, or the instance represents an undefined image
            if ((pe.Context.Instance == null) || (imageIndex < 0))
                return;
            // get the image set
            ImageList imageList = GetImageList(pe.Context.Instance);
            // make sure everything is valid
            if ((imageList == null) || (imageList.Images.Count == 0) || (imageIndex >= imageList.Images.Count))
                return;
            // Draw the preview image
            pe.Graphics.DrawImage(imageList.Images[imageIndex], pe.Bounds);
        }

        #endregion

        #region EventHandlers

        public void OnItemClicked(object sender, ImageListPanelEventArgs e)
        {
            m_selectedIndex = (e).SelectedItem;

            //remove listner
            m_imagePanel.ItemClick -= OnItemClicked;

            // close the drop-dwon, we are done
            wfes.CloseDropDown();

            m_imagePanel.Dispose();
            m_imagePanel = null;
        }

        #endregion
    }
}