using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Metadata;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Nop.Web.Framework.Extensions;

namespace DevPartner.Nop.Plugin.CloudStorage.TagHelpers
{
    /// <summary>
    /// dp-choose-provider tag helper
    /// </summary>
    [HtmlTargetElement("dp-choose-provider", TagStructure = TagStructure.WithoutEndTag)]
    public class DPChooseProviderTagHelper : TagHelper
    {

        private const string ForAttributeName = "asp-for";
        private const string NameAttributeName = "asp-for-name";
        private const string ItemsAttributeName = "asp-items";
        private const string TemplateAttributeName = "asp-template";
        private const string PostfixAttributeName = "asp-postfix";

        private readonly IHtmlHelper _htmlHelper;
        private readonly IEnumerable<Meta<ICloudStorageProviderFactory>> _cloudStorageProviders;

        /// <summary>
        /// An expression to be evaluated against the current model
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        /// <summary>
        /// Editor template for the field
        /// </summary>
        [HtmlAttributeName(TemplateAttributeName)]
        public string Template { set; get; }

        /// <summary>
        /// Postfix
        /// </summary>
        [HtmlAttributeName(PostfixAttributeName)]
        public string Postfix { set; get; }

        /// <summary>
        /// Name for a list
        /// </summary>
        [HtmlAttributeName(NameAttributeName)]
        public string Name { get; set; }

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="htmlHelper">HTML helper</param>
        public DPChooseProviderTagHelper(IHtmlHelper htmlHelper, IEnumerable<Meta<ICloudStorageProviderFactory>> cloudStorageProviders)
        {
            _htmlHelper = htmlHelper;
            _cloudStorageProviders = cloudStorageProviders;
        }

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="output">Output</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            //clear the output
            output.SuppressOutput();

            //contextualize IHtmlHelper
            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            //get htmlAttributes object
            var htmlAttributes = new Dictionary<string, object>();
            var attributes = context.AllAttributes;
            foreach (var attribute in attributes)
            {
                if (!attribute.Name.Equals(ForAttributeName) &&
                    !attribute.Name.Equals(NameAttributeName) &&
                    !attribute.Name.Equals(ItemsAttributeName) /*&&
                    !attribute.Name.Equals(DisabledAttributeName) &&
                    !attribute.Name.Equals(RequiredAttributeName)*/)
                {
                    htmlAttributes.Add(attribute.Name, attribute.Value);
                }
            }


            //generate editor
            var tagName = For != null ? For.Name : Name;
            var settings = For != null ? _cloudStorageProviders.GetComponentSettings(For.Model.ToString()) : null;
            var items = (from provider in _cloudStorageProviders
                         select provider.Metadata["SystemName"] as string into systemName
                         where !String.IsNullOrEmpty(systemName)
                         select new SelectListItem() { Value = systemName, Text = systemName }).ToList();
            if (!string.IsNullOrEmpty(tagName))
            {
                IHtmlContent selectList;
                if (!String.IsNullOrEmpty(Template))
                {
                    selectList = _htmlHelper.Editor(tagName, Template, new { htmlAttributes, SelectList = items, Component = settings, Name = tagName });
                }
                else
                {
                    if (htmlAttributes.ContainsKey("class"))
                        htmlAttributes["class"] += " form-control";
                    else
                        htmlAttributes.Add("class", "form-control");

                    selectList = _htmlHelper.DropDownList(tagName, items, htmlAttributes);
                }
                output.Content.SetHtmlContent(selectList.RenderHtmlContent());
            }
        }
    }
}