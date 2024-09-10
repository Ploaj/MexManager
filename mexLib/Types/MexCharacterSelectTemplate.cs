using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace mexLib.Types
{
    public class MexCharacterSelectTemplate : MexReactiveObject
    {
        //public float TotalWidth { get; set; } = 70.1f;
        private int _iconsPerRow = 9;
        [Range(1, 100)]
        public int IconsPerRow { get => _iconsPerRow; set { _iconsPerRow = value; OnPropertyChanged(); } }

        private float _scale = 1.0f;
        [Range(0.01f, 100f)]
        public float Scale { get => _scale; set { _scale = value; OnPropertyChanged(); } }

        private float _centerX = 0.05f;
        [DisplayName("Center X")]
        public float CenterX { get => _centerX; set { _centerX = value; OnPropertyChanged(); } }

        private float _centerY = 9.5f;
        [DisplayName("Center Y")]
        public float CenterY { get => _centerY; set { _centerY = value; OnPropertyChanged(); } }

        private float _iconWidth = 7.05f;
        [DisplayName("Icon Width")]
        [Browsable(false)]
        [Range(0.01f, 100f)]
        public float IconWidth { get => _iconWidth; set { _iconWidth = value; OnPropertyChanged(); } }

        private float _iconHeight = 7.2f;
        [DisplayName("Icon Height")]
        [Browsable(false)]
        [Range(0.01f, 100f)]
        public float IconHeight { get => _iconHeight; set { _iconHeight = value; OnPropertyChanged(); } }

        private float _iconSideDropX = 0;
        [DisplayName("Icon Side Drop X")]
        public float IconSideDropX { get => _iconSideDropX; set { _iconSideDropX = value; OnPropertyChanged(); } }

        private float _iconSideDropY = -0.3f;
        [DisplayName("Icon Side Drop Y")]
        public float IconSideDropY { get => _iconSideDropY; set { _iconSideDropY = value; OnPropertyChanged(); } }

        private float _iconSideDropZ = -1;
        [DisplayName("Icon Side Drop Z")]
        public float IconSideDropZ { get => _iconSideDropZ; set { _iconSideDropZ = value; OnPropertyChanged(); } }

        public void Apply(ObservableCollection<MexCharacterSelectIcon> icons)
        {
            int num_of_rows = (int)Math.Ceiling(icons.Count / (double)IconsPerRow);

            float icon_height = IconHeight * Scale;
            float icon_width = IconWidth * Scale;

            float total_height = (num_of_rows) * icon_height;
            float total_width = IconsPerRow * icon_width;

            for (int i = 0; i < icons.Count; i++)
            {
                int col = i % IconsPerRow;
                int row = i / IconsPerRow;

                int lastRow = IconsPerRow - 1;

                if (row >= num_of_rows - 1 && (icons.Count % IconsPerRow) > 0)
                {
                    lastRow = (icons.Count % IconsPerRow) - 1;
                    total_width = (icons.Count % IconsPerRow) * icon_width;
                }

                icons[i].X = CenterX - total_width / 2 + icon_width * col + icon_width / 2;
                icons[i].Y = CenterY + total_height / 2 - icon_height * row - icon_height / 2;
                icons[i].Z = 0;
                icons[i].ScaleX = Scale;
                icons[i].ScaleY = Scale;
                icons[i].CollisionSizeX = IconWidth;
                icons[i].CollisionSizeY = IconHeight;

                if (col == lastRow || col == 0)
                {
                    icons[i].X += IconSideDropX * Scale;
                    icons[i].Y += IconSideDropY * Scale;
                    icons[i].Z += IconSideDropZ;

                    icons[i].CollisionOffsetX = -IconSideDropX;
                    icons[i].CollisionOffsetY = -IconSideDropY;
                }
                else
                {
                    icons[i].CollisionOffsetX = 0;
                    icons[i].CollisionOffsetY = 0;
                }
            }
        }
        
    }
}
