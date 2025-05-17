using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.PropertyGrid.Controls.Factories;
using Avalonia.PropertyGrid.Controls;
using System.Collections.Generic;
using PropertyModels.Extensions;
using mexLib.Attributes;
using mexLib;

namespace MexManager.Factories
{

    public class MexReferenceCellFactory : AbstractCellEditFactory
    {
        public override int ImportPriority => base.ImportPriority - 100000;

        public override Control? HandleNewProperty(PropertyCellContext context)
        {
            if (Global.Workspace == null)
                return null;

            var propertyDescriptor = context.Property;
            MexLinkAttribute? link = propertyDescriptor.GetCustomAttribute<MexLinkAttribute>();

            if (link == null)
                return null;

            IEnumerable<object>? coll = null;
            switch (link.Link)
            {
                case MexLinkType.Fighter:
                    coll = Global.Workspace?.Project.Fighters;
                    break;
                case MexLinkType.Stage:
                    coll = Global.Workspace?.Project.Stages;
                    break;
                case MexLinkType.Music:
                    coll = Global.Workspace?.Project.Music;
                    break;
                case MexLinkType.Sound:
                    coll = Global.Workspace?.Project.SoundGroups;
                    break;
                case MexLinkType.Series:
                    coll = Global.Workspace?.Project.Series;
                    break;
            }

            if (coll == null)
                return null;

            var target = context.Target;
            if (propertyDescriptor.GetValue(target) is not int index)
                return null;

            var type = propertyDescriptor.PropertyType;

            var control = new ComboBox()
            {
                ItemsSource = coll,
            };

            if (link.Link == MexLinkType.Fighter && Global.Workspace != null)
            {
                control.SelectedIndex = MexFighterIDConverter.ToInternalID(index, Global.Workspace.Project.Fighters.Count);
            }
            else
            if (link.Link == MexLinkType.Stage)
            {
                control.SelectedIndex = MexStageIDConverter.ToInternalID(index);
            }
            else
            {
                control.SelectedIndex = index;
            }
            control.HorizontalAlignment = HorizontalAlignment.Stretch;

            control.SelectionChanged += (s, e) =>
            {
                if (link.Link == MexLinkType.Fighter)
                {
                    if (Global.Workspace != null)
                        propertyDescriptor.SetValue(target, MexFighterIDConverter.ToExternalID(control.SelectedIndex, Global.Workspace.Project.Fighters.Count));
                }
                else
                if (link.Link == MexLinkType.Stage)
                {
                    propertyDescriptor.SetValue(target, MexStageIDConverter.ToExternalID(control.SelectedIndex));
                }
                else
                {
                    propertyDescriptor.SetValue(target, control.SelectedIndex);
                }
            };

            return control;
        }

        public override bool HandlePropertyChanged(PropertyCellContext context)
        {
            return false;
        }
    }
}
