using System.ComponentModel;

namespace mexLib.Types
{
    public abstract class MexIconBase : MexReactiveObject
    {
        private float _x = 0;
        [Category("1 - General")]
        public float X { get => _x; set { _x = value; OnPropertyChanged(); } }

        private float _y = 0;
        [Category("1 - General")]
        public float Y { get => _y; set { _y = value; OnPropertyChanged(); } }

        private float _z = 0;
        [Category("1 - General")]
        public float Z { get => _z; set { _z = value; OnPropertyChanged(); } }

        private float _scaleX = 1.0f;
        [Category("1 - General")]
        [DisplayName("Scale X")]
        public float ScaleX { get => _scaleX; set { _scaleX = value; OnPropertyChanged(); } }

        private float _scaleY = 1.0f;
        [Category("1 - General")]
        [DisplayName("Scale Y")]
        public float ScaleY { get => _scaleY; set { _scaleY = value; OnPropertyChanged(); } }

        [Browsable(false)]
        public abstract float BaseWidth { get; }

        [Browsable(false)]
        public abstract float BaseHeight { get; }

        [Browsable(false)]
        public abstract (float, float) CollisionOffset { get; }

        [Browsable(false)]
        public abstract (float, float) CollisionSize { get; }

        [Browsable(false)]
        public abstract int ImageKey { get; }

        public abstract MexImage? GetIconImage(MexWorkspace workspace);

    }
}
