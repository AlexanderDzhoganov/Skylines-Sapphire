using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace Sapphire
{

    public class Mod : IUserMod
    {

        public string Name
        {
            get
            {
                SapphireBootstrap.Bootstrap(); return "Sapphire";
            }
        }

        public string Description
        {
            get { return "UI reskin framework"; }
        }

    }

    public class ModLoad : LoadingExtensionBase
    {


        public override void OnLevelLoaded(LoadMode mode)
        {
        }

        public override void OnLevelUnloading()
        {
        }
    }

}
