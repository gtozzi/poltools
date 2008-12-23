/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace Controls
{
    public partial class Texture : UserControl
    {
        public Texture()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refMarker = this;
        }

        private static Texture refMarker = null;
        private bool Loaded = false;
        public void Reload()
        {
            if (Loaded)
                OnLoad(this, EventArgs.Empty);
        }

        public static bool SearchGraphic(int graphic)
        {
            int index = 0;

            for (int i = index; i < refMarker.listView1.Items.Count; i++)
            {
                ListViewItem item = refMarker.listView1.Items[i];
                if ((int)item.Tag == graphic)
                {
                    if (refMarker.listView1.SelectedItems.Count == 1)
                        refMarker.listView1.SelectedItems[0].Selected = false;
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    return true;
                }
            }
            return false;
        }

        private void drawitem(object sender, DrawListViewItemEventArgs e)
        {
            int i = (int)e.Item.Tag;

            Bitmap bmp = Textures.GetTexture(i);

            if (bmp != null)
            {
                int width = bmp.Width;
                int height = bmp.Height;

                if (width >= e.Bounds.Width)
                    width = e.Bounds.Width - 2;

                if (height >= e.Bounds.Height)
                    height = e.Bounds.Height - 2;

                e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, width, height));

                if (listView1.SelectedItems.Contains(e.Item))
                    e.DrawFocusRectangle();
                else
                    e.Graphics.DrawRectangle(Pens.Gray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
        }

        private void listView_SelectedIndexChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
                Label.Text = String.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", (int)listView1.SelectedItems[0].Tag,Textures.GetTexture((int)listView1.SelectedItems[0].Tag).Width);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.Cursor = Cursors.AppStarting;
            Loaded = true;
            listView1.BeginUpdate();
            listView1.Clear();
            ListViewItem item;

            for (int i = 0; i < 0x1000; i++)
            {
                if (Textures.TestTexture(i))
                {
                    item = new ListViewItem(i.ToString(), 0);
                    item.Tag = i;
                    listView1.Items.Add(item);
                }
            }

            listView1.TileSize = new Size(64, 64);
            listView1.EndUpdate();
            this.Cursor = Cursors.Default;
        }

        private TextureSearch showform = null;
        private void OnClickSearch(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
            {
                showform = new TextureSearch();
                showform.TopMost = true;
                showform.Show();
            }
        }

        private void onClickSave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Textures.Save(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
            this.Cursor = Cursors.Default;
            MessageBox.Show(
                String.Format("Saved to {0}", AppDomain.CurrentDomain.SetupInformation.ApplicationBase),
                "Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void onClickExport(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            int i = (int)listView1.SelectedItems[0].Tag;
            string FileName = Path.Combine(path, String.Format("Texture {0}.tiff", i));
            Bitmap bmp = Textures.GetTexture(i);
            bmp.Save(FileName, ImageFormat.Tiff);
            MessageBox.Show(
                String.Format("Texture saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void onClickFindNext(object sender, EventArgs e)
        {
            int id = (int)listView1.SelectedItems[0].Tag;
            id++;
            for (int i = listView1.SelectedItems[0].Index + 1; i < listView1.Items.Count; i++)
            {
                if (id < (int)listView1.Items[i].Tag)
                {
                    ListViewItem item = listView1.Items[i];
                    if (listView1.SelectedItems.Count == 1)
                        listView1.SelectedItems[0].Selected = false;
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    break;
                }
                id++;
            }
        }

        private void onClickRemove(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove 0x{0:X}", i),
                        "Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Textures.Remove(i);
                i = listView1.SelectedItems[0].Index;
                listView1.SelectedItems[0].Selected = false;
                listView1.Items.RemoveAt(i);
                listView1.Invalidate();
            }
        }

        private void onClickReplace(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "image file (*.tiff)|*.tiff";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bmp = new Bitmap(dialog.FileName);
                    if (((bmp.Width == 64) && (bmp.Height == 64)) || ((bmp.Width == 128) && (bmp.Height == 128)))
                    {
                        int i = (int)listView1.SelectedItems[0].Tag;
                        Textures.Replace(i, bmp);
                        listView1.Invalidate();
                        listView_SelectedIndexChanged(this, (ListViewItemSelectionChangedEventArgs)null);
                    }
                    else
                        MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void onTextChangedInsert(object sender, EventArgs e)
        {
            int index;
            bool candone;
            if (InsertText.Text.Contains("0x"))
            {
                string convert = InsertText.Text.Replace("0x", "");
                candone = int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out index);
            }
            else
                candone = int.TryParse(InsertText.Text, System.Globalization.NumberStyles.Integer, null, out index);

            if (index > 0xFFF)
                candone = false;
            if (candone)
            {
                if (Textures.TestTexture(index))
                    InsertText.ForeColor = Color.Red;
                else
                    InsertText.ForeColor = Color.Black;
            }
            else
                InsertText.ForeColor = Color.Red;
        }

        private void onKeyDownInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                bool candone;
                if (InsertText.Text.Contains("0x"))
                {
                    string convert = InsertText.Text.Replace("0x", "");
                    candone = int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out index);
                }
                else
                    candone = int.TryParse(InsertText.Text, System.Globalization.NumberStyles.Integer, null, out index);
                if (index > 0xFFF)
                    candone = false;
                if (candone)
                {
                    if (Textures.TestTexture(index))
                        return;
                    contextMenuStrip1.Close();
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Multiselect = false;
                    dialog.Title = String.Format("Choose image file to insert at 0x{0:X}", index);
                    dialog.CheckFileExists = true;
                    dialog.Filter = "image file (*.tiff)|*.tiff";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(dialog.FileName);
                        if (((bmp.Width == 64) && (bmp.Height == 64)) || ((bmp.Width == 128) && (bmp.Height == 128)))
                        {
                            Textures.Replace(index, bmp);
                            //bug in listview always added to end so...
                            listView1.BeginUpdate();
                            listView1.Items.Clear();
                            for (int i = 0; i < 0x1000; i++)
                            {
                                if (Textures.TestTexture(i))
                                {
                                    ListViewItem item = new ListViewItem(i.ToString(), 0);
                                    item.Tag = i;
                                    listView1.Items.Add(item);
                                }
                            }
                            listView1.EndUpdate();
                            SearchGraphic(index);
                        }
                        else
                            MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }
            }
        }
    }
}