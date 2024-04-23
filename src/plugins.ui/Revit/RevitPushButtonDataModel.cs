
namespace plugins.ui
{
    using Autodesk.Revit.UI;

    public class RevitPushButtonDataModel
    {
        public string Label { get; set; }
        public RibbonPanel Panel { get; set; }
        public string CommandNamespacePath { get; set; }
        public string Tooltip { get; set; }
        public string IconImageName { get; set; }
        public string TooltipImageName { get; set; }

        public RevitPushButtonDataModel()
        {

        }
    }
}
