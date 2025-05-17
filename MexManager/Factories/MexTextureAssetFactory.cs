using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using mexLib;
using mexLib.AssetTypes;
using MexManager.Tools;
using PropertyModels.Extensions;
using System.IO;
using System.Linq;

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
        }

        private static StackPanel GenerateImagePanel(MexTextureAsset asset, out Image imageControl)
        {
            // create border
            Border imageBorder = new()
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
            StackPanel panel = new()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            panel.Children.Add(imageBorder);

            // optional size attribute view
            if (asset != null && asset.Width != -1 && asset.Height != -1)
            {
                imageBorder.Width = asset.Width;
                imageBorder.Height = asset.Height;
                imageControl.Width = asset.Width;
                imageControl.Height = asset.Height;
                panel.Children.Add(new TextBlock()
                {
                    Text = $"{asset.Width}x{asset.Height}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
            }

            return panel;
        }
        /// <summary>
        /// 
        /// </summary>
        private async static void ImportImage(
            MexWorkspace workspace,
            string? file,
            MexTextureAsset asset,
            Image imageControl)
        {
            if (Global.Workspace == null)
                return;

            // get file to import
            file ??= await FileIO.TryOpenFile("Image", "", FileIO.FilterPng);

            // no file to import
            if (file == null)
                return;

            // get absolute path
            using FileStream stream = new(file, FileMode.Open);
            asset.SetFromImageFile(workspace, stream);

            // update image preview
            imageControl.Source = asset.GetSourceImage(workspace)?.ToBitmap();
            imageControl.Width = asset.Width;
            imageControl.Height = asset.Height;
        }

        /// <summary>
        /// Handles the new property.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Control.</returns>
        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            if (Global.Workspace == null)
                return null;

            System.ComponentModel.PropertyDescriptor propertyDescriptor = context.Property;
            object target = context.Target;

            // check type
            if (propertyDescriptor.PropertyType != typeof(MexTextureAsset))
                return null;

            // get texture asset
            if (context.GetValue() is not MexTextureAsset textureAsset)
                return null;

            // create image panel and enable drag and drop
            StackPanel imagePanel = GenerateImagePanel(textureAsset, out Image imageControl);

            Button importButton = new()
            {
                Content = "Import"
            };
            importButton.Click += (s, e) =>
            {
                ImportImage(
                    Global.Workspace,
                    null,
                    textureAsset,
                    imageControl);
            };
            Button exportButton = new()
            {
                Content = "Export"
            };
            exportButton.Click += async (s, e) =>
            {
                if (Global.Workspace == null)
                    return;

                string? file = await FileIO.TrySaveFile("Image", "", FileIO.FilterPng);
                if (file != null)
                {
                    textureAsset.GetSourceImage(Global.Workspace)?.ToBitmap().Save(file);
                }
            };

            StackPanel buttonStack = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = Thickness.Parse("2"),
            };
            buttonStack.Children.Add(importButton);
            buttonStack.Children.Add(exportButton);

            // Create a StackPanel or any other container to hold the text box and image
            DockPanel control = new()
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


            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragEnterEvent, (s, e) =>
            {
                if (e.Data.Contains(DataFormats.Files))
                {
                    e.DragEffects = DragDropEffects.Copy;
                }
                else
                {
                    e.DragEffects = DragDropEffects.None;
                }
            });
            control.AddHandler(DragDrop.DragOverEvent, (s, e) =>
            {
                // Check if the data is file data
                if (e.Data.Contains(DataFormats.Files))
                {
                    e.DragEffects = DragDropEffects.Copy;
                }
                else
                {
                    e.DragEffects = DragDropEffects.None;
                }
            });
            control.AddHandler(DragDrop.DropEvent, (s, e) =>
            {
                // Get the dropped file names
                if (e.Data.Contains(DataFormats.Files))
                {
                    System.Collections.Generic.IEnumerable<Avalonia.Platform.Storage.IStorageItem>? fileNames = e.Data.GetFiles();
                    if (fileNames == null)
                        return;

                    foreach (string? f in fileNames.Select(e => e.Path.AbsolutePath))
                    {
                        if (f.EndsWith(".png"))
                        {
                            ImportImage(
                                Global.Workspace,
                                f,
                                textureAsset,
                                imageControl);
                        }
                    }
                }
            });

            return control;
        }
        /// <summary>
        /// Handles the property changed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool HandlePropertyChanged(PropertyCellContext context)
        {
            if (Global.Workspace == null)
                return false;

            Control control = context.CellEdit;
            System.ComponentModel.PropertyDescriptor propertyDescriptor = context.Property;
            object target = context.Target;

            // check type
            if (propertyDescriptor.PropertyType != typeof(MexTextureAsset))
                return false;

            // get texture asset
            if (context.GetValue() is not MexTextureAsset textureAsset)
                return false;

            ValidateProperty(control, propertyDescriptor, target);

            if (control.Tag is UserData data)
            {
                if (data.Image != null)
                {
                    MexImage? image = textureAsset.GetSourceImage(Global.Workspace);
                    if (image != null)
                    {
                        data.Image.Source = image.ToBitmap();
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
