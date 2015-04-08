using ICities;

namespace Sapphire
{

    public class Mod : IUserMod
    {

        public string Name
        {
            get
            {
                SapphireBootstrap.Bootstrap(Skin.ModuleClass.MainMenu); return "Sapphire";
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
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame)
            {
                SapphireBootstrap.Bootstrap(Skin.ModuleClass.InGame);
            }
            else if (mode == LoadMode.NewMap || mode == LoadMode.LoadMap)
            {
                SapphireBootstrap.Bootstrap(Skin.ModuleClass.MapEditor);
            }
            else if (mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
            {
                SapphireBootstrap.Bootstrap(Skin.ModuleClass.AssetEditor);
            }
        }

        public override void OnLevelUnloading()
        {
        }
    }

}
