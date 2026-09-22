using System;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        string playInventoryOrigin;
        InventoryWindow nativeInventory;
        Action inventoryPreviousRepaint;
        public bool PlayInventoryOpen=>nativeInventory!=null;
        public InventoryWindow InventoryPanel=>nativeInventory;
        public void ShowPlayInventory()
        {
            if(PlayInventoryOpen||game.Store==null||Page!="plaza"&&Page!="battle")return;
            OpenNativeInventory(false);
        }
        void OpenNativeInventory(bool portal)
        {
            if(nativeInventory!=null||game.Store==null)return;
            playInventoryOrigin=Page;inventoryPreviousRepaint=pageRepaint;
            ResetTownInput();game.Town?.Cancel();CloseHudPanel();Page="bag";
            nativeInventory=InventoryWindow.Open(transform,game.Store,game.catalog,font,()=>InterfaceFactor,()=>
            {
                nativeInventory=null;if(Page=="bag"){Page=playInventoryOrigin;pageRepaint=inventoryPreviousRepaint;}playInventoryOrigin=null;
                RefreshHud();
            },()=>{if(game.Combat!=null&&game.Combat.Hero==game.Store.Data.Hero)game.Combat.RefreshEquipment();});
            pageRepaint=()=>nativeInventory?.Repaint();
        }
        public void ClosePlayInventory(){if(nativeInventory!=null)nativeInventory.Close();}
        void ReleasePlayInventory(){ClosePlayInventory();playInventoryOrigin=null;}
    }
}
