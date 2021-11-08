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
            while (true)
            {
                Reference reff = uIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element el = uIDocument.Document.GetElement(reff);
                TaskDialog.Show("1", el.GetType().ToString());
            }
        }

    }
}
