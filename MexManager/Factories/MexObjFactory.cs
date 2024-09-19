using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using MexManager.Tools;
using System.IO;
using mexLib.AssetTypes;
using MexManager.Controls;
using Avalonia.Platform.Storage;
using mexLib.Utilties;

namespace MexManager.Factories
{
    public class MexObjFactory : AbstractCellEditFactory
    {
        /// <summary>
        /// Gets the import priority.
        /// The larger the value, the earlier the object will be processed
        /// </summary>
        /// <value>The import priority.</value>
        public override int ImportPriority => base.ImportPriority;

        /// <summary>
        /// Handles the new property.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Control.</returns>
        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            var propertyDescriptor = context.Property;
            var target = context.Target;

            if (propertyDescriptor.PropertyType != typeof(MexOBJAsset))
                return null;

            if (propertyDescriptor.GetValue(target) is not MexOBJAsset asset)
                return null;

            // Create a StackPanel or any other container to hold the text box and image
            var control = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var objControl = new ObjControl(asset)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 300,
                Height = 300,
            };

            // Create the Image control

            var importButton = new Button()
            {
                Content = "Import OBJ",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            importButton.Click += async (s, e) =>
            {
                if (Global.Workspace == null)
                    return;

                var file = await FileIO.TryOpenFile("Export OBJ", "",
                    [
                        new FilePickerFileType("OBJ")
                            {
                                Patterns = [ "*.obj", ],
                            },
                    ]);

                if (file != null)
                {
                    var obj = new ObjFile();
                    using var fs = new FileStream(file, FileMode.Open);
                    obj.Load(fs);
                    obj.FlipFaces();

                    asset.SetFromObjFile(Global.Workspace, obj);
                    objControl.RefreshRender();
                }
            };
            var exportButton = new Button()
            {
                Content = "Export OBJ",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            exportButton.Click += async (s, e) =>
            {
                if (Global.Workspace == null)
                    return;

                var obj = asset.GetOBJFile(Global.Workspace);
                if (obj != null)
                {
                    var file = await FileIO.TrySaveFile("Export OBJ", "emblem.obj", 
                    [
                        new FilePickerFileType("OBJ")
                            {
                                Patterns = [ "*.obj", ],
                            },
                    ]);
                    if (file != null)
                    {
                        using var stream = new FileStream(file, FileMode.Create);
                        obj.Write(stream);
                    }
                }
            };
            var optionStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            optionStack.Children.Add(importButton);
            optionStack.Children.Add(exportButton);

            control.Children.Add(objControl);
            control.Children.Add(optionStack);
            objControl.RefreshRender();

            return control;
        }
        /// <summary>
        /// Handles the property changed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool HandlePropertyChanged(PropertyCellContext context)
        {
            return false;
        }
    }
}
