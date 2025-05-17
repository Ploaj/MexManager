using HSDRaw;
using HSDRaw.MEX;
using mexLib.Attributes;
using System.Collections.ObjectModel;

namespace mexLib.Types
{
    public class MexItemState : HSDAccessor
    {
        public int AnimID { get; set; }

        [DisplayHex]
        public uint AnimationCallback { get; set; }

        [DisplayHex]
        public uint PhysicsCallback { get; set; }

        [DisplayHex]
        public uint CollisionCallback { get; set; }

        public void FromMexItemState(MEX_ItemStateInfo item)
        {
            AnimID = item.AnimID;
            AnimationCallback = (uint)item.AnimationCallback;
            PhysicsCallback = (uint)item.PhysicsCallback;
            CollisionCallback = (uint)item.CollisionCallback;
        }
        public MEX_ItemStateInfo ToMexItemState()
        {
            return new MEX_ItemStateInfo()
            {
                AnimID = AnimID,
                AnimationCallback = (int)AnimationCallback,
                PhysicsCallback = (int)PhysicsCallback,
                CollisionCallback = (int)CollisionCallback
            };
        }
        public override string ToString()
        {
            return string.Format("State AID:{0} 0x{1} 0x{2} 0x{3}", AnimID, AnimationCallback.ToString("X8"), PhysicsCallback.ToString("X8"), CollisionCallback.ToString("X8"));
        }
    }
    public class MexItem
    {
        public string Name { get; set; } = "New Item";

        public ObservableCollection<MexItemState> States { get; set; } = new ObservableCollection<MexItemState>();

        [DisplayHex]
        public uint OnSpawn { get; set; }

        [DisplayHex]
        public uint OnDestroy { get; set; }

        [DisplayHex]
        public uint OnPickup { get; set; }

        [DisplayHex]
        public uint OnDrop { get; set; }

        [DisplayHex]
        public uint OnThrow { get; set; }

        [DisplayHex]
        public uint OnGiveDamage { get; set; }

        [DisplayHex]
        public uint OnTakeDamage { get; set; }

        [DisplayHex]
        public uint OnEnterAir { get; set; }

        [DisplayHex]
        public uint OnReflect { get; set; }

        [DisplayHex]
        public uint OnClank { get; set; }

        [DisplayHex]
        public uint OnUnk2 { get; set; }

        [DisplayHex]
        public uint OnHitShieldBounce { get; set; }

        [DisplayHex]
        public uint OnHitShieldDetermineDestroy { get; set; }

        [DisplayHex]
        public uint OnUnk3 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void FromMexItem(MEX_Item item)
        {
            States.Clear();
            if (item.ItemStates != null)
                foreach (MEX_ItemStateInfo? i in item.ItemStates)
                {
                    MexItemState s = new();
                    s.FromMexItemState(i);
                    States.Add(s);
                }
            OnSpawn = (uint)item.OnSpawn;
            OnDestroy = (uint)item.OnDestroy;
            OnPickup = (uint)item.OnPickup;
            OnDrop = (uint)item.OnDrop;
            OnThrow = (uint)item.OnThrow;
            OnGiveDamage = (uint)item.OnGiveDamage;
            OnTakeDamage = (uint)item.OnTakeDamage;
            OnEnterAir = (uint)item.OnEnterAir;
            OnReflect = (uint)item.OnReflect;
            OnClank = (uint)item.OnUnknown1;
            OnUnk2 = (uint)item.OnUnknown2;
            OnHitShieldBounce = (uint)item.OnHitShieldBounce;
            OnHitShieldDetermineDestroy = (uint)item.OnHitShieldDetermineDestroy;
            OnUnk3 = (uint)item.OnUnknown3;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MEX_Item ToMexItem()
        {
            return new MEX_Item()
            {
                ItemStates = States.Select(e => e.ToMexItemState()).ToArray(),
                OnSpawn = (int)OnSpawn,
                OnDestroy = (int)OnDestroy,
                OnPickup = (int)OnPickup,
                OnDrop = (int)OnDrop,
                OnThrow = (int)OnThrow,
                OnGiveDamage = (int)OnGiveDamage,
                OnTakeDamage = (int)OnTakeDamage,
                OnEnterAir = (int)OnEnterAir,
                OnReflect = (int)OnReflect,
                OnUnknown1 = (int)OnClank,
                OnUnknown2 = (int)OnUnk2,
                OnHitShieldBounce = (int)OnHitShieldBounce,
                OnHitShieldDetermineDestroy = (int)OnHitShieldDetermineDestroy,
                OnUnknown3 = (int)OnUnk3,
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
