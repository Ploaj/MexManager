using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using Avalonia.PropertyGrid.Utils;
using MeleeMedia.Video;
using mexLib.Attributes;
using MexManager.Converters;
using MexManager.Tools;
using Octokit;
using PropertyModels.ComponentModel;
using PropertyModels.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MexManager.Factories
{
    public class MediaImageFactory : AbstractCellEditFactory
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

            if (propertyDescriptor.PropertyType != typeof(string))
            {
                return null;
            }

            if (propertyDescriptor.GetCustomAttribute<MexMediaAttribute>() == null)
            {
                return null;
            }

            // Create a StackPanel or any other container to hold the text box and image
            var control = new StackPanel 
            { 
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            // Create the Image control
            var stringControl = new Avalonia.PropertyGrid.Controls.Factories.Builtins.StringCellEditFactory().HandleNewProperty(context);
            stringControl.Name = "fileName";
            stringControl.HorizontalAlignment = HorizontalAlignment.Stretch;

            var imageControl = new Avalonia.Controls.Image
            {
                Source = BitmapManager.MexFighterImage,
                Width = 320,
                Height = 240,
                Margin = new Thickness(5)
            };

            var importButton = new Button()
            {
                Content = "Import Image",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            importButton.Click += async (s, e) =>
            {
                var file = await FileIO.TryOpenFile("Image", "", FileIO.FilterJpeg);
                if (file != null &&
                    propertyDescriptor.GetValue(context.Target) is string fileName)
                {
                    var thpPath = Global.Workspace?.GetFilePath(fileName);
                    if (thpPath != null &&
                        Path.GetExtension(thpPath) == ".thp")
                    {
                        imageControl.Source = new Bitmap(file);
                        Global.Files.Set(thpPath, THP.FromJPEG(Global.Files.Get(file)).Data);
                    }
                }
            };
            var exportButton = new Button()
            {
                Content = "Export Image",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            exportButton.Click += async (s, e) =>
            {
                var fileName = propertyDescriptor.GetValue(context.Target) as string;
                var file = await FileIO.TrySaveFile("Image", Path.GetFileNameWithoutExtension(fileName) + ".jpg", FileIO.FilterJpeg);
                if (file != null &&
                    fileName != null)
                {
                    var thpPath = Global.Workspace?.GetFilePath(fileName);
                    if (thpPath != null &&
                        Path.GetExtension(thpPath) == ".thp")
                    {
                        Global.Files.Set(file, new THP(Global.Files.Get(thpPath)).ToJPEG());
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


            control.Children.Add(stringControl);
            control.Children.Add(imageControl);
            control.Children.Add(optionStack);

            return control;
        }
        /// <summary>
        /// Handles the property changed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool HandlePropertyChanged(PropertyCellContext context)
        {
            var propertyDescriptor = context.Property;
            var target = context.Target;
            var control = context.CellEdit;

            if (propertyDescriptor.PropertyType != typeof(string))
            {
                return false;
            }

            ValidateProperty(control, propertyDescriptor, target);

            if (control is StackPanel stack &&
                stack.Children[0] is TextBox textBox &&
                stack.Children[1] is Avalonia.Controls.Image image)
            {
                var value = propertyDescriptor.GetValue(target) as string;

                textBox.Text = value;

                if (value != null &&
                    Global.Workspace != null)
                {
                    var thpPath = Global.Workspace.GetFilePath(value);

                    if (!Global.Files.Exists(thpPath) || 
                        Path.GetExtension(thpPath) != ".thp")
                    {
                        image.Source = BitmapManager.MissingImage;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(thpPath);

                        var thp = new THP(Global.Files.Get(thpPath));
                        var jpeg = thp.ToJPEG();
                        using var stream = new MemoryStream(jpeg);
                        image.Source = new Bitmap(stream);
                    }

                }
                else
                {
                    image.Source = BitmapManager.MissingImage;
                }

                return true;
            }

            return false;
        }
    }
}
