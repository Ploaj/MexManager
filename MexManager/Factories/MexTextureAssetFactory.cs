using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using mexLib.Attributes;
using mexLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyModels.Extensions;
using Avalonia.Media.Imaging;
using Avalonia;
using MeleeMedia.Video;
using MexManager.Tools;
using System.IO;
using System.Diagnostics;
using System.Reflection.Metadata;
using Avalonia.Media;

namespace MexManager.Factories
{
    public class MexTextureAssetFactory : AbstractCellEditFactory
    {
        /// <summary>
        /// Gets the import priority.
        /// The larger the value, the earlier the object will be processed
        /// </summary>
        /// <value>The import priority.</value>
        public override int ImportPriority => base.ImportPriority;

        private class UserData
        {
            public Image? Image { get; set; }

            //public TextBox? TextBox { get; set; }
        }

        private static Control GenerateImagePanel(MexTextureSize sizeAttr, out Image imageControl)
        {
            // create border
            var imageBorder = new Border()
            {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = Thickness.Parse("1"),
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            // Create the Image control
            imageControl = new Image
            {
                Source = BitmapManager.MissingImage,
                Margin = new Thickness(5),
            };
            imageBorder.Child = imageControl;

            // create final panel
            StackPanel panel = new ()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            panel.Children.Add(imageBorder);

            // optional size attribute view
            if (sizeAttr != null)
            {
                imageBorder.Width = sizeAttr.Width;
                imageBorder.Height = sizeAttr.Height;
                imageControl.Width = sizeAttr.Width;
                imageControl.Height = sizeAttr.Height;
                panel.Children.Add(new TextBlock()
                {
                    Text = $"{sizeAttr.Width}x{sizeAttr.Height}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
            }

            return panel;
        }

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

            var attr = propertyDescriptor.GetCustomAttribute<MexTextureAssetAttribute>();
            var sizeAttr = propertyDescriptor.GetCustomAttribute<MexTextureSize>();
            var fmtAttr = propertyDescriptor.GetCustomAttribute<MexTextureFormatAttribute>();

            if (attr == null)
            {
                return null;
            }

            var imagePanel = GenerateImagePanel(sizeAttr, out Image imageControl);

            var importButton = new Button()
            {
                Content = "Import"
            };
            importButton.Click += async (s, e) =>
            {
                if (Global.Workspace == null)
                    return;

                // create new asset if filepath is blank
                if (target is MexAssetContainerBase con)
                    con.GenerateAssetPaths(Global.Workspace);

                // get filepath and check for null
                var fileName = propertyDescriptor.GetValue(target) as string;
                if (string.IsNullOrEmpty(fileName))
                    return;

                // get absolute path
                var path = Global.Workspace.GetAssetPath(fileName);

                // get file to import
                var file = await FileIO.TryOpenFile("Image", "", FileIO.FilterPng);
                if (file != null)
                {

                    if (path != null)
                    {
                        // image->tex
                        MexImage tex;
                        if (fmtAttr != null)
                        {
                            // resize image
                            if (sizeAttr != null)
                            {
                                tex = ImageConverter.PNGtoMexImage(file, sizeAttr.Width, sizeAttr.Height, fmtAttr.Format, fmtAttr.TlutFormat);
                            }
                            else
                            {
                                tex = ImageConverter.PNGtoMexImage(file, fmtAttr.Format, fmtAttr.TlutFormat);
                            }
                        }
                        else
                        {
                            tex = ImageConverter.PNGtoMexImage(file, HSDRaw.GX.GXTexFmt.RGBA8, HSDRaw.GX.GXTlutFmt.IA8);
                        }

                        // update image preview
                        imageControl.Source = tex.ToBitmap();

                        // save files
                        Global.Files.Set(path, File.ReadAllBytes(file));
                        Global.Files.Set(path.Replace(".png", ".tex"), tex.ToByteArray());
                    }
                }
            };
            var exportButton = new Button()
            {
                Content = "Export"
            };
            exportButton.Click += async (s, e) =>
            {
                if (Global.Workspace == null)
                    return;

                var fileName = propertyDescriptor.GetValue(target) as string;
                if (string.IsNullOrEmpty(fileName))
                    return;

                var path = Global.Workspace.GetAssetPath(fileName);
                if (!Global.Files.Exists(path))
                    return;

                var file = await FileIO.TrySaveFile("Image", "", FileIO.FilterPng);
                if (file != null)
                {
                    File.WriteAllBytes(file, Global.Files.Get(path));
                }
            };

            var buttonStack = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = Thickness.Parse("2"),
            };
            buttonStack.Children.Add(importButton);
            buttonStack.Children.Add(exportButton);

            // Create a StackPanel or any other container to hold the text box and image
            var control = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left
            };
            DockPanel.SetDock(imagePanel, Dock.Top);
            DockPanel.SetDock(buttonStack, Dock.Left);

            control.Children.Add(imagePanel);
            control.Children.Add(buttonStack);

            control.Tag = new UserData()
            {
                Image = imageControl,
            };

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

            if (control.Tag is UserData data)
            {
                var filePath = propertyDescriptor.GetValue(target) as string;

                //if (data.TextBox != null)
                //    data.TextBox.Text = filePath;

                if (data.Image != null)
                {
                    if (filePath != null &&
                    Global.Workspace != null)
                    {
                        var assetPath = Global.Workspace.GetAssetPath(filePath);

                        if (!Global.Files.Exists(assetPath))
                        {
                            data.Image.Source = BitmapManager.MissingImage;
                        }
                        else
                        {
                            using var stream = new MemoryStream(Global.Files.Get(assetPath));
                            data.Image.Source = new Bitmap(stream);
                        }
                    }
                    else
                    {
                        data.Image.Source = BitmapManager.MissingImage;
                    }

                    data.Image.Height = data.Image.Source.Size.Height;
                }
                
                return true;
            }

            return false;
        }
    }
}
