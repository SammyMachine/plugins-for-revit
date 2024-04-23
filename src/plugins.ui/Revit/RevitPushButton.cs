namespace plugins.ui
{
    using System;
    using System.Windows;
    using Autodesk.Revit.UI;
    using res;
    using core;
    public static class RevitPushButton
    {
        public static PushButton Create(RevitPushButtonDataModel data)
        {
            var btnDataName = Guid.NewGuid().ToString();

            var btnData = new PushButtonData(btnDataName, data.Label, CoreAssembly.GetAssemblyLocation(), data.CommandNamespacePath)
            {
                ToolTip = data.Tooltip,
                LargeImage = ResourceImage.GetIcon(data.IconImageName),
                ToolTipImage = ResourceImage.GetIcon(data.TooltipImageName)
            };

            return data.Panel.AddItem(btnData) as PushButton;
        }
    }
}
