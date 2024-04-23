namespace plugins
{
    using Autodesk.Revit.UI;
    using ui;
    using core;
    public class SetupInterface
    {
        public SetupInterface()
        {

        }
        public void Initialize(UIControlledApplication app)
        {
            string tabName = "Плагины Гипробум";
            app.CreateRibbonTab(tabName);
            var commandsPanel = app.CreateRibbonPanel(tabName, "Команды");

            var antiConnectionsButtonData = new RevitPushButtonDataModel
            {
                Label = "Анти\nсоединение",
                Panel = commandsPanel,
                Tooltip = "Плагин для совмещения объектов стен заданного типа по плоскости",
                CommandNamespacePath = AntiConnectionsCommand.GetPath(),
                IconImageName = "Anticonnection.png",
                TooltipImageName = "Anticonnection.png"
            };

            var antiConnectionsButton = RevitPushButton.Create(antiConnectionsButtonData);

            var splitWallsAndColumnsButtonData = new RevitPushButtonDataModel
            {
                Label = "Разрезание\nстен и колонн",
                Panel = commandsPanel,
                Tooltip = "Плагин для разрезания объектов стен/колонн заданного типа по уровню",
                CommandNamespacePath = SplittingWallsAndColumnsCommand.GetPath(),
                IconImageName = "Splitting.png",
                TooltipImageName = "Splitting.png"
            };

            var splitWallsAndColumnsButton = RevitPushButton.Create(splitWallsAndColumnsButtonData);

            var cuttingElementsButtonData = new RevitPushButtonDataModel
            {
                Label = "Вырезание\nотверстий",
                Panel = commandsPanel,
                Tooltip = "Плагин для вырезания объектов отверстий заданного семейства",
                CommandNamespacePath = CuttingHolesCommand.GetPath(),
                IconImageName = "Cutting.png",
                TooltipImageName = "Cutting.png"
            };

            var cuttingElementsButton = RevitPushButton.Create(cuttingElementsButtonData);

        }
    }
}
