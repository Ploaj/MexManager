using HSDRaw;
using HSDRaw.MEX;
using HSDRaw.MEX.Menus;
using mexLib.MexScubber;
using mexLib.Utilties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

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

        [DisplayName("Use ColorSmash")]
        public bool UseColorSmash { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mxdt"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void FromMxDt(MEX_Data mxdt)
        {
            CharacterSelectHandScale = mxdt.MenuTable.Parameters.CSSHandScale;

            foreach (MEX_CSSIcon? i in mxdt.MenuTable.CSSIconData.Icons)
            {
                MexCharacterSelectIcon icon = new()
                {
                    Fighter = i.ExternalCharID,
                    SFXID = i.SFXID,
                };

                FighterIcons.Add(icon);
            }

            Template.Apply(FighterIcons);
        }
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
            foreach (MEX_CSSIcon? i in css.Icons)
            {
                FighterIcons.Add(new MexCharacterSelectIcon()
                {
                    Fighter = i.ExternalCharID,
                    SFXID = i.SFXID,
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        public void ApplyCompression(MexWorkspace ws, bool force, ProgressChangedEventHandler? progress = null)
        {
            int csp_width = (int)(136 * CSPCompression);
            int csp_height = (int)(188 * CSPCompression);

            int remainingImages = ws.Project.Fighters.Sum(e => e.Costumes.Count);
            int totalImages = remainingImages;

            if (remainingImages > 0)
            {
                // Create a list of tasks
                ManualResetEvent doneEvent = new(false);

                foreach (MexFighter fighter in ws.Project.Fighters)
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        foreach (var group in fighter.Costumes.GroupBy(e=>e.ColorSmashGroup))
                        {
                            List<(MexCostume, MexImage)> colorsmash = new ();

                            // resize 
                            foreach (var costume in group)
                            {
                                if (costume == null)
                                    continue;

                                // Process the image
                                MexImage? textureAsset = costume.CSPAsset.GetTexFile(ws);

                                // check for compression
                                if (textureAsset != null &&
                                    (textureAsset.Width > csp_width ||
                                    textureAsset.Height > csp_height ||
                                    force))
                                {
                                    costume.CSPAsset.Resize(ws, csp_width, csp_height);
                                    textureAsset = costume.CSPAsset.GetTexFile(ws);
                                }

                                if (textureAsset != null)
                                    colorsmash.Add((costume, textureAsset));
                            }

                            // color smash the texture assets if they aren't already?
                            // they are already smashed if their indices are equal
                            if (group.Key >= 0 &&
                                colorsmash.Count > 0 && 
                                colorsmash.Any(e => !e.Item2.ImageData.SequenceEqual(colorsmash[0].Item2.ImageData)))
                            {
                                // apply color smash
                                progress?.Invoke(this, new ProgressChangedEventArgs(-1, $"ColorSmashing Fighter: \"{fighter.Name}\" Group: {group.Key} Costumes: {colorsmash.Count}..."));

                                // 
                                var sw = new Stopwatch();
                                sw.Start();
                                ColorSmash.Quantize(colorsmash.Select(e => e.Item2), ColorSmash.ColorType.Rgb5a3, 256, false);
                                foreach (var c in colorsmash)
                                    c.Item1.CSPAsset.SetFromMexImage(ws, c.Item2, false);
                                sw.Stop();

                                //
                                progress?.Invoke(this, new ProgressChangedEventArgs(-1, $"Completed - Fighter: \"{fighter.Name}\" Group: {group.Key} Costumes: {colorsmash.Count} in {sw.Elapsed.ToString()}"));
                            }

                            // decrement percentage
                            foreach (var c in group)
                            {
                                progress?.Invoke(this, new ProgressChangedEventArgs((int)((1 - (remainingImages / (decimal)totalImages)) * 100), $"{fighter.Name} -  {c.Name}"));
                                // Decrement the remaining counter
                                if (Interlocked.Decrement(ref remainingImages) == 0)
                                {
                                    doneEvent.Set(); // Signal when all images are processed
                                }
                            }
                        }
                    });
                }

                // Wait until all images are processed
                doneEvent.WaitOne();
                progress?.Invoke(this, new ProgressChangedEventArgs(100, $"Done!"));
            }
        }
    }
}
