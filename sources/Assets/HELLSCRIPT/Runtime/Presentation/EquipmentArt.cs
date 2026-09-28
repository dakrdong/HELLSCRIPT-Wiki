using UnityEngine;

namespace Hellscript
{
    // Artwork follows the original equipment definition, independently of an imprinted power.
    public static class EquipmentArt
    {
        public static Texture2D ForItem(Item item)
        {
            if(item==null)return null;
            return ForDefinition(item.special)??ForBase(ItemCatalog.Base(item).id);
        }
        public static Texture2D ForBase(string id)
        {
            // The legacy iron shield shares the ironbound round shield silhouette.
            return Resources.Load<Texture2D>("Art/EquipmentVariants/"+(id=="B28"?"B32":id));
        }
        public static Texture2D ForDefinition(string id)
        {
            if(string.IsNullOrEmpty(id))return null;
            var definition=ItemCatalog.Unique(id);if(definition==null)return null;
            string folder=string.IsNullOrEmpty(definition.setId)?"ClassLegendaryIcons":"ClassSetIcons";
            return Resources.Load<Texture2D>("Art/"+folder+"/"+definition.id);
        }
        public static Texture2D SetEmblem(Item item)
        {
            if(string.IsNullOrEmpty(item?.special))return null;
            string setId=ItemCatalog.Unique(item.special)?.setId;
            return string.IsNullOrEmpty(setId)?null:Resources.Load<Texture2D>("Art/ClassSetIcons/"+setId);
        }
    }
}
