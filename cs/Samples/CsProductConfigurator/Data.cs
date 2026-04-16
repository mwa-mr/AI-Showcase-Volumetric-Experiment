using System.Collections.Generic;
using Microsoft.UI;
using Windows.UI;

namespace Volumetric.Samples.ProductConfigurator
{
    public class ImageSlot
    {
        public required int Index { get; set; }
        public required string OriginalImage { get; set; }
        public required string SelectedImage { get; set; }
        public required string PreviewImage { get; set; }
    }

    public class AccessorySlot
    {
        public required int Id { get; set; }
        public required string AccessoryName { get; set; }
        public required string OriginalImage { get; set; }
        public required string SelectedImage { get; set; }
    }

    public class Data
    {
        public static Color HeadbandSelectedColor { get; set; } = Microsoft.UI.ColorHelper.FromArgb(255, 255, 255, 255);
        public static Color SpeakersSelectedColor { get; set; } = Microsoft.UI.ColorHelper.FromArgb(255, 255, 255, 255);
        public static ImageSlot? SelectedTexture { get; set; } = null;
        public static AccessorySlot? SelectedAccesory { get; set; } = null;
        public static AccessorySlot? SelectedAccesory2 { get; set; } = null;
        public static AccessorySlot? SelectedAccesory3 { get; set; } = null;

        public static List<ImageSlot> ImageSlots = new List<ImageSlot>
        {
            new ImageSlot
            {
                Index = 0,
                OriginalImage = "Assets/Images/EarCup1.png",
                SelectedImage = "Assets/Images/EarCup1_selected.png",
                PreviewImage = "Assets/Images/EarCup1_preview.png"
            },
            new ImageSlot
            {
                Index = 1,
                OriginalImage = "Assets/Images/EarCup2.png",
                SelectedImage = "Assets/Images/EarCup2_selected.png",
                PreviewImage = "Assets/Images/EarCup2_preview.png"
            },
            new ImageSlot
            {
                Index = 2,
                OriginalImage = "Assets/Images/EarCup3.png",
                SelectedImage = "Assets/Images/EarCup3_selected.png",
                PreviewImage = "Assets/Images/EarCup3_preview.png"
            },
            new ImageSlot
            {
                Index = 3,
                OriginalImage = "Assets/Images/EarCup4.png",
                SelectedImage = "Assets/Images/EarCup4_selected.png",
                PreviewImage = "Assets/Images/EarCup4_preview.png"
            }
        };

        public static List<AccessorySlot> AccessoriesSlot = new List<AccessorySlot>
        {
            new AccessorySlot
            {
                Id = 1,
                AccessoryName = "Accessory 1",
                OriginalImage = "Assets/Images/acc1.png",
                SelectedImage = "Assets/Images/acc1sel.png"
            },
            new AccessorySlot
            {
                Id = 2,
                AccessoryName = "Accessory 2",
                OriginalImage = "Assets/Images/acc2.png",
                SelectedImage = "Assets/Images/acc2sel.png"
            },
            new AccessorySlot
            {
                Id = 3,
                AccessoryName = "Accessory 3",
                OriginalImage = "Assets/Images/acc3.png",
                SelectedImage = "Assets/Images/acc3sel.png"
            },
        };
    }
}
