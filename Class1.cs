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
using Autodesk.Revit.DB.Mechanical;
using System.Threading;
using UIFrameworkServices;

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

            //Class2.test(commandData.Application.ActiveUIDocument);
            //return Result.Succeeded;

            ShowCursor(true);
            List<System.Drawing.Point> points = new List<System.Drawing.Point>();
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document document = uIDocument.Document;

            #region 获取需要遍历的图元
            FilteredElementCollector flt_elementids = new FilteredElementCollector(document);
            flt_elementids.OfClass(typeof(FamilyInstance));
            //List<Reference> list_refs = (List<Reference>)uIDocument.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
            List<ElementId> elementIds = new List<ElementId>();
            //foreach (Reference item in list_refs)
            //{
            //    elementIds.Add(item.ElementId);
            //}

            elementIds = (List<ElementId>)flt_elementids.ToElementIds();
            #endregion

            #region 截图区域
            MessageBox.Show("截取第一个点");
            while (points.Count < 1)
            {
                if (System.Windows.Forms.Control.MouseButtons == MouseButtons.Left)
                {
                    points.Add(Cursor.Position);
                }
                Application.DoEvents();
            }
            MessageBox.Show("截取第二个点");
            while (points.Count < 2)
            {
                if (System.Windows.Forms.Control.MouseButtons == MouseButtons.Left)
                {
                    points.Add(Cursor.Position);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, (UIntPtr)0);
                }
                Application.DoEvents();
            }
            //ShowCursor(false);
            #endregion

            #region 原始截图
            IntPtr revit_handle= Process.GetCurrentProcess().MainWindowHandle;
            Screen revit_Screen = Screen.FromHandle(revit_handle);
            Bitmap btm = new Bitmap(points[1].X-points[0].X-2, points[1].Y-points[0].Y-2);//减少两个像素，防止鼠标乱入
            Graphics ori_g = Graphics.FromImage(btm);
            ori_g.CopyFromScreen(points[0].X, points[0].Y, 0, 0, btm.Size);
            MemoryStream memoryStream = new MemoryStream();
            btm.Save(memoryStream,ImageFormat.Jpeg);
            string ori_str = Convert.ToBase64String(memoryStream.GetBuffer());
            #endregion

            Form1 f1 = new Form1();


            f1.Text = elementIds.Count().ToString();
            f1.Show();
            List<ElementId> hide_elementids = new List<ElementId>();
            List<ElementId> hide_tmp_elementids = new List<ElementId>();
            string str;
            #region
            DateTime dt = DateTime.Now;
            foreach (ElementId item in elementIds)
            {
                f1.Text = (int.Parse(f1.Text) - 1).ToString();
                using (Transaction trans=new Transaction (document,"hide"))
                {
                    hide_tmp_elementids = new List<ElementId>();
                    hide_tmp_elementids.Add(item);
                    trans.Start();
                    uIDocument.ActiveView.HideElements(hide_tmp_elementids);
                    trans.Commit();
                    Application.DoEvents();
                    memoryStream = new MemoryStream();
                    ori_g.CopyFromScreen(points[0].X, points[0].Y, 0, 0, btm.Size);
                    btm.Save(memoryStream, ImageFormat.Jpeg);
                    str = Convert.ToBase64String(memoryStream.GetBuffer());
                    if (ori_str != str)
                    {
                        f1.richTextBox1.Text = f1.richTextBox1.Text + "\n" + item.ToString();
                        UIFrameworkServices.QuickAccessToolBarService.performMultipleUndoRedoOperations(true, 1);
                        Application.DoEvents();
                        //Thread.Sleep(500);
                    }
                    else
                    {
                        f1.richTextBox2.Text = f1.richTextBox2.Text + "\n" + item.ToString();
                    }
                }
            }
            #endregion
            ShowCursor(true);
            MessageBox.Show((DateTime.Now-dt).TotalSeconds.ToString()+":"+dt.ToString()+">>"+DateTime.Now.ToString());

            return Result.Succeeded;
        }
    }
}
