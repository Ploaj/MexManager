using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Services;
using MexManager.Factories;

namespace MexManager.Views
{
    public class PropertyGridExt : PropertyGrid
    {
        static PropertyGridExt()
        {
            CellEditFactoryService.Default.AddFactory(new MexTextureAssetFactory());
            CellEditFactoryService.Default.AddFactory(new MexObjFactory());
            CellEditFactoryService.Default.AddFactory(new MexReferenceCellFactory());
            CellEditFactoryService.Default.AddFactory(new MediaImageFactory());
            CellEditFactoryService.Default.AddFactory(new HexCellFactory());
            CellEditFactoryService.Default.AddFactory(new MexFilePathFactory());
        }
    }

}
