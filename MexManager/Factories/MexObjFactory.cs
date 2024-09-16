using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.PropertyGrid.Controls;
using Avalonia;
using Avalonia.PropertyGrid.Controls.Factories;
using MeleeMedia.Video;
using mexLib.Attributes;
using MexManager.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mexLib.AssetTypes;
using MexManager.Controls;
using Avalonia.Platform.Storage;

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
                HorizontalAlignment=HorizontalAlignment.Stretch,
                Height = 400,
            };

            // Create the Image control

            var importButton = new Button()
            {
                Content = "Import OBJ",
                HorizontalAlignment = HorizontalAlignment.Stretch
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
                    asset.SetFromData(Global.Workspace, File.ReadAllBytes(file));
                    objControl.RefreshRender();
                }
            };
            var exportButton = new Button()
            {
                Content = "Export OBJ",
                HorizontalAlignment = HorizontalAlignment.Stretch
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
