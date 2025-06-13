using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Template;
using Bridge.Results;
using JetBrains.Annotations;

namespace Common.Services
{
    public interface ITemplateManagingService
    {
        event Action<string, string> TemplateRenamed; 
        
        Task<Result<TemplateInfo>> ChangeTemplateName(TemplateInfo templateInfo, string newName);
    }

    [UsedImplicitly]
    internal sealed class TemplateManagingService: ITemplateManagingService
    {
        private readonly IBridge _bridge;
        
        public event Action<string, string> TemplateRenamed;

        public TemplateManagingService(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<Result<TemplateInfo>> ChangeTemplateName(TemplateInfo templateInfo, string newName)
        {
            var resp = await _bridge.RenameTemplate(templateInfo.Id, newName);
            if (resp.IsSuccess)
            {
                TemplateRenamed?.Invoke(templateInfo.Title, resp.Model.Title);
            }
            return resp;
        }
    }
}
