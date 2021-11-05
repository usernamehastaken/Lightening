using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace Lightening
{
    public static class Class2
    {
        public static void test(UIDocument uIDocument)
        {
            FilteredElementCollector flt_elementids = new FilteredElementCollector(uIDocument.Document);
            flt_elementids.WherePasses(new ElementIsElementTypeFilter(true));
            TaskDialog.Show("1", flt_elementids.Count().ToString());
            //uIDocument.Selection.SetElementIds(flt_elementids.ToElementIds());
        }

    }
}
