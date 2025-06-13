using Bridge.Models.ClientServer.Template;

namespace UIManaging.Pages.OnboardingTemplateSelection
{
    internal sealed class CarouselTemplateListModel
    {
        public TemplateInfo[] TemplateInfo { get; }
        public float CellSize { get; }

        public CarouselTemplateListModel(TemplateInfo[] templateInfos, float cellSize)
        {
            TemplateInfo = templateInfos;
            CellSize = cellSize;
        }
    }
}
