using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace com.AylanJ123.CodeDecay.UI
{
    /// <summary>
    /// A temporary UI element that is instantiated to represent an item being dragged
    /// </summary>
    public sealed class DragItemUI : MonoBehaviour
    {
        [Tooltip("The image component for the main item icon")]
        [SerializeField, MustBeAssigned]
        private Image iconImage;
        [Tooltip("The image component for the item detail icon")]
        [SerializeField, MustBeAssigned]
        private Image detailImage;

        /// <summary> Initializes the drag item with the item's visual data </summary>
        /// <param name="data"> The ItemData to display </param>
        public void Initialize(ItemData data)
        {
            iconImage.sprite = data.icon;
            detailImage.sprite = data.iconDetail;
            detailImage.color = data.highlightColor;
        }
    }
}