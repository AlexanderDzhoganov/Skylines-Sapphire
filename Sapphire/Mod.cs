using System;
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
                try
                {
                    Core.Bootstrap(Skin.ModuleClass.MainMenu);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                return "Sapphire";
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
            try
            {
                if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame)
                {
                    Core.Bootstrap(Skin.ModuleClass.InGame);
                }
                else if (mode == LoadMode.NewMap || mode == LoadMode.LoadMap)
                {
                    Core.Bootstrap(Skin.ModuleClass.MapEditor);
                }
                else if (mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
                {
                    Core.Bootstrap(Skin.ModuleClass.AssetEditor);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public override void OnLevelUnloading()
        {
            Core.Deinitialize();
        }
    }

}
