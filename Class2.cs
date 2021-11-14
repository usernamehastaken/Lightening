using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Windows.Forms;

namespace Lightening
{
    public static class Class2
    {
        public static void test(UIDocument uIDocument)
        {
            FilteredElementCollector flt = new FilteredElementCollector(uIDocument.Document);
            List<Element> elements = (List<Element>)flt.OfCategory(BuiltInCategory.OST_Materials).ToElements();
            foreach (Element item in elements)
            {
                Transaction tran = new Transaction(uIDocument.Document, "0");
                if (((Material)item).Transparency!=0)
                {
                    tran.Start();
                    ((Material)item).Transparency = 0;
                    tran.Commit();
                }
            }
            uIDocument.RefreshActiveView();
        }

    }
}
