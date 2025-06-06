﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls;
using Avalonia.PropertyGrid.Controls.Factories;
using Avalonia.PropertyGrid.Services;
using mexLib.Attributes;
using MexManager.Extensions;
using MexManager.Tools;
using PropertyModels.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MexManager.Factories
{
    public class MexFilePathFactory : AbstractCellEditFactory
    {
        public override int ImportPriority => base.ImportPriority - 100000;

        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            return GenerateControl(context);
        }

        public static Control? GenerateControl(PropertyCellContext context)
        {
            PropertyDescriptor prop = context.Property;
            MexFilePathValidatorAttribute attr = prop.GetCustomAttribute<MexFilePathValidatorAttribute>();

            if (attr == null)
                return null;

            if (context.Property.GetValue(context.Target) is not string path)
                return null;

            MexFilePathValidatorCallback cbAttr = context.Property.GetCustomAttribute<MexFilePathValidatorCallback>();

            DockPanel control = new();

            TextBox stringControl = (TextBox)new Avalonia.PropertyGrid.Controls.Factories.Builtins.StringCellEditFactory().HandleNewProperty(context);
            stringControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            stringControl.Background = ThemeExtensions.SystemAccentColor;
            stringControl.TextChanged += (s, e) =>
            {
                ValidationResult? res = attr.IsValid(Global.Workspace, stringControl.Text);
                if (res == null)
                {

                }
                else if (res == ValidationResult.Success)
                {
                    DataValidationErrors.ClearErrors(stringControl);
                }
                else
                {
                    DataValidationErrors.SetErrors(stringControl, [LocalizationService.Default[res.ErrorMessage]]);
                }
            };
            DockPanel.SetDock(stringControl, Dock.Left);

            DragDrop.SetAllowDrop(stringControl, true);
            stringControl.AddHandler(DragDrop.DragEnterEvent, (s, e) =>
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
            stringControl.AddHandler(DragDrop.DragOverEvent, (s, e) =>
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
            stringControl.AddHandler(DragDrop.DropEvent, async (s, e) =>
            {
                // Get the dropped file names
                if (e.Data.Contains(DataFormats.Files))
                {
                    IEnumerable<Avalonia.Platform.Storage.IStorageItem>? fileNames = e.Data.GetFiles();
                    if (fileNames == null)
                        return;

                    foreach (string? f in fileNames.Select(e => e.Path.AbsolutePath))
                    {
                        string? newPath = await GetAndValidatePath(f, context.Target, attr, cbAttr);
                        if (newPath != null)
                            stringControl.Text = newPath;
                    }
                }
            });

            Button button = new()
            {
                Content = "...",
            };
            button.Click += async (s, e) =>
            {
                // get file
                string? res = await FileIO.TryOpenFile("Open File", path, FileIO.FilterAll);
                if (res == null)
                    return;

                string? newPath = await GetAndValidatePath(res, context.Target, attr, cbAttr);
                if (newPath != null)
                    stringControl.Text = newPath;
            };
            DockPanel.SetDock(button, Dock.Right);

            control.Children.Add(button);
            control.Children.Add(stringControl);

            return control;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPath"></param>
        /// <param name="target"></param>
        /// <param name="attr"></param>
        /// <param name="cbAttr"></param>
        /// <returns></returns>
        private static async Task<string?> GetAndValidatePath(
            string newPath,
            object target,
            MexFilePathValidatorAttribute attr,
            MexFilePathValidatorCallback? cbAttr)
        {
            if (Global.Workspace == null)
                return null;

            // validation callback
            if (cbAttr != null)
            {
                System.Reflection.MethodInfo? callback = target.GetType().GetMethod(cbAttr.CallbackMethodName);
                object? ret = callback?.Invoke(target, [Global.Workspace, newPath]);

                if (ret is MexFilePathError error)
                {
                    await MessageBox.Show(error.Message, "File Error", MessageBox.MessageBoxButtons.Ok);
                    return null;
                }
            }

            // get new filename
            string fileName = System.IO.Path.GetFileName(newPath);
            string fullPath = attr.GetFullPath(Global.Workspace, fileName);

            // check if file already exists
            if (Global.Files.Exists(fullPath))
            {
                MessageBox.MessageBoxResult overwrite = await MessageBox.Show($"File already exists!\nWould you like to overwrite \"{fileName}\"?", "Import File", MessageBox.MessageBoxButtons.YesNoCancel);

                if (overwrite != MessageBox.MessageBoxResult.Yes)
                    return null;
            }

            // add file to filesystem
            Global.Files.Set(fullPath, File.ReadAllBytes(newPath));

            // update the path value
            return fileName;
        }

        public override bool HandlePropertyChanged(PropertyCellContext context)
        {
            return false;
        }
    }
}
