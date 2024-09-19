using HSDRaw;
using HSDRaw.MEX.Menus;
using mexLib.MexScubber;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace mexLib.Types
{
    public class MexCharacterSelect
    {
        [DisplayName("Cursor Scale")]
        public float CharacterSelectHandScale { get; set; } = 1.0f;

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Browsable(false)]
        public MexCharacterSelectTemplate Template { get; set; } = new();

        [Browsable(false)]
        public ObservableCollection<MexCharacterSelectIcon> FighterIcons { get; set; } = new();

        [DisplayName("CSP Compression Level")]
        public float CSPCompression { get; set; } = 1.0f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dol"></param>
        public void FromDOL(MexDOL dol)
        {
            // CSSIconData - 0x803F0A48 0x398
            MEX_IconData css = new()
            {
                _s = new HSDStruct(dol.GetData(0x803F0A48, 0x398))
            };
            // extract icon data
            foreach (var i in css.Icons)
            {
                FighterIcons.Add(new MexCharacterSelectIcon()
                {
                    Fighter = i.ExternalCharID,
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        public void ApplyCompression(MexWorkspace ws)
        {
            int csp_width = (int)(136 * CSPCompression);
            int csp_height = (int)(188 * CSPCompression);

            // Create a list of tasks
            ManualResetEvent doneEvent = new (false);
            int remainingImages = ws.Project.Fighters.Sum(e => e.Costumes.Count);

            foreach (var fighter in ws.Project.Fighters)
            {
                foreach (var c in fighter.Costumes)
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        // Process the image
                        var textureAsset = c.CSPAsset.GetTexFile(ws);

                        // check for compression
                        if (textureAsset != null &&
                            (textureAsset.Width != csp_width ||
                            textureAsset.Height != csp_height))
                        {
                            c.CSPAsset.Resize(ws, csp_width, csp_height);
                            textureAsset = c.CSPAsset.GetTexFile(ws);
                        }

                        // Decrement the remaining counter
                        if (Interlocked.Decrement(ref remainingImages) == 0)
                        {
                            doneEvent.Set(); // Signal when all images are processed
                        }
                    });
                }
            }

            // Wait until all images are processed
            doneEvent.WaitOne();
        }
    }
}
