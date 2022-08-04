using System.Collections;
using HtmlAgilityPack;
using XPing365.Core.Source;

namespace XPing365.Core.Parser.Internals
{
    internal class DefaultParser<T> : IParser<T> where T : HtmlSource
    {
        public T Parse(ref T dataSource)
        {
            HtmlNode rootNode = dataSource.GetHtmlDocument().DocumentNode;
            this.Traverse(rootNode, ref dataSource);

            return dataSource;
        }

        private void Traverse<TItem>(HtmlNode rootNode, ref TItem item)
        {
            if (rootNode == null || item == null)
            {
                return;
            }

            var properties = item.GetType().GetPropertiesToVisit();

            foreach (var p in properties)
            {
                var attribute = p.GetAttribute<XPathAttribute>();
 
                if (attribute == null)
                {
                    if (p.PropertyType.IsClass)
                    {
                        object? propertyValue = Activator.CreateInstance(p.PropertyType);
                        p.SetValue(item, propertyValue);
                        this.Traverse(rootNode, ref propertyValue);
                    }
                }
                else
                {
                    try
                    {
                        if (p.PropertyType.IsList())
                        {
                            IList? list = p.PropertyType.CreateList();
                            p.SetValue(item, list);
                            var selectedNodes = rootNode.SelectNodes(attribute.XPath);

                            foreach (var selectedNode in selectedNodes)
                            {
                                var listItem = p.PropertyType.CreateListItem();
                                list?.Add(listItem);
                                this.Traverse(selectedNode, ref listItem);
                            }
                            continue;
                        }
                        if (p.PropertyType.IsClass && p.PropertyType != typeof(string))
                        {
                            var singleNode = rootNode.SelectSingleNode(attribute.XPath);
                            object? propertyValue = Activator.CreateInstance(p.PropertyType);
                            p.SetValue(item, propertyValue);
                            this.Traverse(singleNode, ref propertyValue);
                            continue;
                        }
                        else
                        {
                            var singleNode = rootNode.SelectSingleNode(attribute.XPath);
                            var value = singleNode.GetValue(attribute);
                            
                            if (!string.IsNullOrEmpty(value))
                            {
                                var result = p.GetConverter().Convert(value, p.PropertyType);
                                p.SetValue(item, result);
                            }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Currently we treat all XPathAttributes as optional and skip if cannot retrieve from the Html.
                        // The plan is to be able to decorate properties as [Required] so the DataParser won't continue
                        // if the XPathAttribute is not found in the Html document.
                    }
                }
            }
        }
    }
}
