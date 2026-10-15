using UnityEngine;

namespace Hellscript
{
    // Artwork follows the original equipment definition, independently of an imprinted power.
    public static class EquipmentArt
    {
        public static Texture2D ForItem(Item item,EquipmentArtProvider provider=null)
        {
            if(item==null)return null;
            return provider?.ForItem(item)??ForDefinition(item.special)??ForBase(ItemCatalog.Base(item).id);
        }
        public static Texture2D ForBase(string id)
        {
            return CasualEquipmentArt.Provider.ForBase(id);
        }
        public static Texture2D ForDefinition(string id)
        {
            return CasualEquipmentArt.Provider.ForDefinition(id);
        }
        public static Texture2D SetEmblem(Item item,EquipmentArtProvider provider=null)
        {
            var supplied=provider?.SetEmblem(item);if(supplied!=null)return supplied;
            if(string.IsNullOrEmpty(item?.special))return null;
            return CasualEquipmentArt.Provider.SetEmblem(item);
        }
    }
}
