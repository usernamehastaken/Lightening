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
using System.Collections;

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
            List<ElementId> elementIds = new List<ElementId>();
            List<Reference> reffs = (List<Reference>)uIDocument.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
            List<string> categorys = new List<string>();
            Dictionary<string, Celle> dic_ells = new Dictionary<string, Celle>();
            foreach (Reference item in reffs)
            {
                Element elt = document.GetElement(item);
                if (!categorys.Contains(elt.Category.Name))
                {
                    categorys.Add(elt.Category.Name);
                    dic_ells.Add(elt.Category.Name, new Celle());
                }
                dic_ells[elt.Category.Name].elementIds.Add(item.ElementId);
            }
            foreach (UIView item in uIDocument.GetOpenUIViews())
            {
                if (item.ViewId==uIDocument.ActiveView.Id)
                {
                    item.ZoomToFit();
                }
            }
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
            Cursor.Position = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y);

            #endregion

            #region 原始截图
            IntPtr revit_handle = Process.GetCurrentProcess().MainWindowHandle;
            Screen revit_Screen = Screen.FromHandle(revit_handle);
            Bitmap btm = new Bitmap(points[1].X-points[0].X-2, points[1].Y-points[0].Y-2);//减少两个像素，防止鼠标乱入
            Graphics ori_g = Graphics.FromImage(btm);
            ori_g.CopyFromScreen(points[0].X, points[0].Y, 0, 0, btm.Size);
            MemoryStream memoryStream = new MemoryStream();
            btm.Save(memoryStream,ImageFormat.Jpeg);
            string ori_str = Convert.ToBase64String(memoryStream.GetBuffer());
            #endregion
            
            Form1 f1 = new Form1();
            f1.Top = points[0].Y - f1.Height - 10;
            f1.Show();

            #region 循环
            DateTime dt = DateTime.Now;
            List<ElementId> remain_ells = new List<ElementId>();
            foreach (string item in dic_ells.Keys)
            {
                ArrayList arrayList = new ArrayList();
                arrayList.Add(dic_ells[item]);
                int flag = 0; int count = arrayList.Count;
                f1.Text = item;
                f1.richTextBox1.Text = f1.richTextBox1.Text + "\n" + f1.Text;
                int nn = 0;
                do
                {
                    f1.Text = item + ">" + nn.ToString();
                    count = arrayList.Count;flag = 0;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (get_show_ells(((Celle)arrayList[i]).elementIds, uIDocument, ori_str, ori_g, points[0].X, points[0].Y, btm).Count == 0)
                        {
                            arrayList.RemoveAt(i);
                            foreach (ElementId id in ((Celle)arrayList[i]).elementIds)
                            {
                                f1.richTextBox2.Text = id + "\n" + f1.richTextBox2.Text;
                            }
                        }
                    }
                    count = arrayList.Count;//优化后的表
                    if (count > 0)
                    {
                        ArrayList new_arrayList = new ArrayList();
                        foreach (Celle celle in arrayList)
                        {
                            new_arrayList.AddRange(celle.slice_ells_to_Celle());
                        }
                        arrayList = new_arrayList;
                        foreach (Celle celle in arrayList)
                        {
                            flag = celle.elementIds.Count + flag;
                        }
                    }
                } while (flag!=count);
                if (arrayList.Count>1)
                {
                    foreach (Celle celle in arrayList)
                    {
                        remain_ells.AddRange(celle.elementIds);
                        f1.richTextBox1.Text = celle.elementIds[0].IntegerValue.ToString() + "\n" + f1.richTextBox1.Text;
                    }
                }
            }
            
            #endregion

            MessageBox.Show((DateTime.Now - dt).TotalSeconds.ToString() + ":" + dt.ToString() + ">>" + DateTime.Now.ToString());

            return Result.Succeeded;
        }

        public List<ElementId> get_show_ells(List<ElementId> elementIds,UIDocument uIDocument,string ori_str, Graphics ori_g,int x,int y, Bitmap btm)
        {
            using (Transaction trans = new Transaction(uIDocument.Document, "hide"))
            {
                trans.Start();
                uIDocument.ActiveView.HideElements(elementIds);
                trans.Commit();
                Application.DoEvents();
                MemoryStream memoryStream = new MemoryStream();
                ori_g.CopyFromScreen(x, y, 0, 0, btm.Size);
                btm.Save(memoryStream, ImageFormat.Jpeg);
                string str = Convert.ToBase64String(memoryStream.GetBuffer());
                if (ori_str != str)
                {
                    UIFrameworkServices.QuickAccessToolBarService.performMultipleUndoRedoOperations(true, 1);
                    Application.DoEvents();
                    return elementIds;
                }
                else
                {
                    return new List<ElementId>();
                }
            }
        }
    }
    
    public class Celle
    {
        public List<ElementId> elementIds;
        public List<Celle> celles = new List<Celle>();
        public Celle()
        {
            elementIds = new List<ElementId>();
        }
        public Celle(List<ElementId> ells)
        {
            elementIds = ells;
        }

        public List<Celle> slice_ells_to_Celle()
        {
            int count = (int)Math.Floor(Math.Sqrt(elementIds.Count));
            int i=0;
            while (i * count < elementIds.Count())
            {
                if ((i+1)*count>elementIds.Count())
                {
                    celles.Add(new Celle(elementIds.GetRange(i * count, elementIds.Count - i * count)));
                }
                else
                {
                    celles.Add(new Celle(elementIds.GetRange(i * count, count)));
                }
                i++;
            }
            return celles;
        }
        
    }
}
