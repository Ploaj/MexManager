using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using mexLib.AssetTypes;
using mexLib.Utilties;
using MexManager.Controls;
using MexManager.Tools;
using System.IO;

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
            System.ComponentModel.PropertyDescriptor propertyDescriptor = context.Property;
            object target = context.Target;

            if (propertyDescriptor.PropertyType != typeof(MexOBJAsset))
                return null;

            if (propertyDescriptor.GetValue(target) is not MexOBJAsset asset)
                return null;

            // Create a StackPanel or any other container to hold the text box and image
            StackPanel control = new()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            ObjControl objControl = new(asset)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 300,
                Height = 300,
            };

            // Create the Image control

            Button importButton = new()
            {
                Content = "Import OBJ",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            importButton.Click += async (s, e) =>
            {
                if (Global.Workspace == null)
                    return;

                string? file = await FileIO.TryOpenFile("Export OBJ", "",
                    [
                        new FilePickerFileType("OBJ")
                            {
                                Patterns = [ "*.obj", ],
                            },
                    ]);

                if (file != null)
                {
                    ObjFile obj = new();
                    using FileStream fs = new(file, FileMode.Open);
                    obj.Load(fs);
                    obj.FlipFaces();

                    asset.SetFromObjFile(Global.Workspace, obj);
                    objControl.RefreshRender();
                }
            };
            Button exportButton = new()
            {
                Content = "Export OBJ",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            exportButton.Click += async (s, e) =>
            {
                if (Global.Workspace == null)
                    return;

                ObjFile? obj = asset.GetOBJFile(Global.Workspace);
                if (obj != null)
                {
                    string? file = await FileIO.TrySaveFile("Export OBJ", "emblem.obj",
                    [
                        new FilePickerFileType("OBJ")
                            {
                                Patterns = [ "*.obj", ],
                            },
                    ]);
                    if (file != null)
                    {
                        using FileStream stream = new(file, FileMode.Create);
                        obj.Write(stream);
                    }
                }
            };
            StackPanel optionStack = new()
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
