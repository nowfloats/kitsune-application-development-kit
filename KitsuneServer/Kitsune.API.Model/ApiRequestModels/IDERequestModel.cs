using Kitsune.Models.Project;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class GetIDEProjectDetailsResponseModel : GetProjectDetailsResponseModel
    {
        public DateTime LastPublishedOn { get; set; }
        public AssetChildren Assets { get; set; }
    }

    public class ApplicationUploadModel
    {
        public string ProjectId { get; set; }
        public string UserEmail { get; set; }
        //public bool IsMobileResponsive { get; set; }
        public string SourcePath { get; set; }
        public KitsunePageType _PageType { get; internal set; }
        public ProjectStatus _ProjectStatus { get; internal set; }
        public ResourceType _ResourceType { get; internal set; }
        public string PageType
        {
            get { return this._PageType.ToString(); }
            set
            {
                KitsunePageType tempPageType;

                if (!string.IsNullOrEmpty(value) && Enum.TryParse(value.Trim().ToUpper(), out tempPageType))
                {
                    this._PageType = tempPageType;
                }
                else
                {
                    this._PageType = KitsunePageType.DEFAULT;
                }

            }
        }
        public string ProjectStatus
        {
            get { return this._ProjectStatus.ToString(); }

            set
            {
                ProjectStatus tempProjectStatus;
                if (!string.IsNullOrEmpty(value) && Enum.TryParse(value, out tempProjectStatus))
                {
                    this._ProjectStatus = tempProjectStatus;
                }
                else
                {
                    this._ProjectStatus = Kitsune.Models.Project.ProjectStatus.IDLE;
                }

            }
        }
        public string ResourceType
        {
            get { return this._ResourceType.ToString(); }

            set
            {
                ResourceType tempResourceType;
                if (!string.IsNullOrEmpty(value) && Enum.TryParse(value, out tempResourceType))
                {
                    this._ResourceType = tempResourceType;
                }
                else
                {
                    this._ResourceType = Kitsune.Models.Project.ResourceType.FILE;
                }
            }

        }
        public bool IsPreview { get; set; }
        public string Offset { get; set; }
        public string Configuration { get; set; }

    }
}
