using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Drawing.Imaging;

namespace Lightening
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        static extern int ShowCursor(bool bShow);
        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_XDOWN = 0x0080;
        const uint MOUSEEVENTF_XUP = 0x0100;
        const uint MOUSEEVENTF_WHEEL = 0x0800;
        const uint MOUSEEVENTF_HWHEEL = 0x01000;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            List<System.Drawing.Point> points = new List<System.Drawing.Point>();
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document document = uIDocument.Document;
            #region 截图区域
            while (points.Count < 1)
            {
                if (System.Windows.Forms.Control.MouseButtons == MouseButtons.Left)
                {
                    points.Add(Cursor.Position);
                }
                Application.DoEvents();
            }
            MessageBox.Show(points[0].X + ":" + points[0].Y);
            while (points.Count < 2)
            {
                if (System.Windows.Forms.Control.MouseButtons == MouseButtons.Left)
                {
                    points.Add(Cursor.Position);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, (UIntPtr)0);
                }
                Application.DoEvents();
            }
            MessageBox.Show(points[1].X + ":" + points[1].Y);
            #endregion

            HashAlgorithm hash = HashAlgorithm.Create();

            #region 原始截图
            IntPtr revit_handle= Process.GetCurrentProcess().MainWindowHandle;
            Screen revit_Screen = Screen.FromHandle(revit_handle);
            Bitmap btm = new Bitmap(points[1].X-points[0].X, points[1].Y-points[0].Y);
            Graphics ori_g = Graphics.FromImage(btm);
            ori_g.CopyFromScreen(points[0].X, points[0].Y, 0, 0, revit_Screen.Bounds.Size);
            Form1 form1 = new Form1();
            form1.Width = btm.Width;
            form1.Height = btm.Height;
            form1.BackgroundImage = btm;
            form1.Show();
            MemoryStream memoryStream = new MemoryStream();
            btm.Save(memoryStream,ImageFormat.Jpeg);
            byte[] hash_b1 = hash.ComputeHash(memoryStream.GetBuffer());
            string ori_str = BitConverter.ToString(hash_b1);
            #endregion
            ori_g.CopyFromScreen(points[0].X, points[0].Y, 0, 0, revit_Screen.Bounds.Size);
            Form1 form2 = new Form1();
            form2.Width = btm.Width;
            form2.Height = btm.Height;
            form2.BackgroundImage = btm;
            form2.Text = "2";
            form2.Show();
            btm.Save(memoryStream, ImageFormat.Jpeg);
            byte[] hash_b2 = hash.ComputeHash(memoryStream.GetBuffer());
            string ori_str2 = BitConverter.ToString(hash_b2);
            MessageBox.Show(ori_str + ":" + ori_str2);
            //List<ElementId> elementIds = new List<ElementId>();
            //foreach (Reference item in uIDocument.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element))
            //{
            //    elementIds.Add(item.ElementId);
            //}
            //List<ElementId> empty_elementIds = new List<ElementId>();
            //uIDocument.Selection.SetElementIds(empty_elementIds);


            //Form1 form1 = new Form1();
            //form1.Text = elementIds.Count.ToString();
            //form1.Show();

            //foreach (ElementId item in elementIds)
            //{
            //    form1.Text = (int.Parse(form1.Text) - 1).ToString();
            //    using (Transaction trans = new Transaction(document, "hide"))
            //    {
            //        empty_elementIds.Clear();
            //        empty_elementIds.Add(new ElementId(248595));
            //        //uIDocument.Selection.SetElementIds(empty_elementIds);
            //        trans.Start();
            //        uIDocument.ActiveView.HideElements(empty_elementIds);
            //        Application.DoEvents();
            //        ori_g.CopyFromScreen(points[0].X, points[0].Y, 0, 0, revit_Screen.Bounds.Size);
            //        btm.Save(memoryStream, ImageFormat.Jpeg);
            //        byte[] hash_b2 = hash.ComputeHash(memoryStream.GetBuffer());
            //        string ori_str2 = BitConverter.ToString(hash_b2);
            //        if (ori_str==ori_str2)
            //        {
            //            trans.Commit();
            //            MessageBox.Show("1");
            //        }
            //        else
            //        {
            //            trans.RollBack();
            //        }
            //    }
            //    return Result.Succeeded;
            //}


            return Result.Succeeded;
        }
    }
}
