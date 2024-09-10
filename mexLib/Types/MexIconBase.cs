using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mexLib.Types
{
    public abstract class MexIconBase : MexReactiveObject
    {
        private float _x = 0;
        public float X { get => _x; set { _x = value; OnPropertyChanged(); } }

        private float _y = 0;
        public float Y { get => _y; set { _y = value; OnPropertyChanged(); } }

        private float _z = 0;
        public float Z { get => _z; set { _z = value; OnPropertyChanged(); } }

        private float _scaleX = 1.0f;
        public float ScaleX { get => _scaleX; set { _scaleX = value; OnPropertyChanged(); } }

        private float _scaleY = 1.0f;
        public float ScaleY { get => _scaleY; set { _scaleY = value; OnPropertyChanged(); } }

        [Browsable(false)]
        public abstract float BaseWidth { get; }

        [Browsable(false)]
        public abstract float BaseHeight { get; }

        [Browsable(false)]
        public abstract int ImageKey { get; }

        public abstract MexImage? GetIconImage(MexWorkspace workspace);
    }
}
