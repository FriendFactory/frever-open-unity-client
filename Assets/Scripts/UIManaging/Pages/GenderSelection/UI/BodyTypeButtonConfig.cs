using DTT.UI.ProceduralUI;

namespace UIManaging.Pages.UmaEditorPage.Ui.Stages
{
    public struct BodyTypeButtonConfig
    {
        public CornerSettings[] Corners { get; set; }
    }

    public struct CornerSettings
    {
        public float Roundness { get; set; }
        public Corner Corner { get; set; }
    }

    public static class GenderConfigs
    {
        public static readonly BodyTypeButtonConfig Left = new()
        {
            Corners = new[]
            {
                new CornerSettings
                {
                    Corner = Corner.TOP_LEFT, Roundness = .57f
                },
                new CornerSettings
                {
                    Corner = Corner.BOTTOM_LEFT, Roundness = .57f
                },
                new CornerSettings
                {
                    Corner = Corner.TOP_RIGHT, Roundness = 0f
                },
                new CornerSettings
                {
                    Corner = Corner.BOTTOM_RIGHT, Roundness = 0f
                }
            }
        };
        
        public static readonly BodyTypeButtonConfig Right = new()
        {
            Corners = new[]
            {
                new CornerSettings
                {
                    Corner = Corner.TOP_LEFT, Roundness = 0f
                },
                new CornerSettings
                {
                    Corner = Corner.BOTTOM_LEFT, Roundness = 0f
                },
                new CornerSettings
                {
                    Corner = Corner.TOP_RIGHT, Roundness = .57f
                },
                new CornerSettings
                {
                    Corner = Corner.BOTTOM_RIGHT, Roundness = .57f
                }
            }
        };
        
        public static readonly BodyTypeButtonConfig Middle = new()
        {
            Corners = new[]
            {
                new CornerSettings
                {
                    Corner = Corner.TOP_LEFT, Roundness = 0f
                },
                new CornerSettings
                {
                    Corner = Corner.BOTTOM_LEFT, Roundness = 0f
                },
                new CornerSettings
                {
                    Corner = Corner.TOP_RIGHT, Roundness = 0f
                },
                new CornerSettings
                {
                    Corner = Corner.BOTTOM_RIGHT, Roundness = 0f
                }
            }
        };
        
        public static readonly BodyTypeButtonConfig TheOnly = new()
        {
            Corners = new[]
            {
                new CornerSettings
                {
                    Corner = Corner.TOP_LEFT, Roundness = .57f
                },
                new CornerSettings
                {
                    Corner = Corner.BOTTOM_LEFT, Roundness = .57f
                },
                new CornerSettings
                {
                    Corner = Corner.TOP_RIGHT, Roundness = .57f
                },
                new CornerSettings
                {
                    Corner = Corner.BOTTOM_RIGHT, Roundness = .57f
                }
            }
        };
    }
} 

