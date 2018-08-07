﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;

namespace SvgToVectorDrawableConverter.DataFormat.Common
{
    sealed class ElementCollection : IEnumerable<Element>
    {
        private readonly XmlElement _xmlElement;

        public ElementCollection([NotNull] XmlElement xmlElement)
        {
            _xmlElement = xmlElement;
        }

        private IEnumerable<XmlElement> ChildElements => _xmlElement.ChildNodes.OfType<XmlElement>();

        public int Count => ChildElements.Count();

        public Element this[int index] => ElementFactory.Wrap(ChildElements.ElementAt(index));

        public void RemoveAt(int index)
        {
            _xmlElement.RemoveChild(ChildElements.ElementAt(index));
        }

        public T Append<T>()
            where T : Element
        {
            var result = ElementFactory.Create<T>(_xmlElement.OwnerDocument, out XmlElement child);
            _xmlElement.AppendChild(child);
            return result;
        }

        public void MoveTo(int index, Element item)
        {
            if (index >= Count)
            {
                _xmlElement.AppendChild(item.WrappedElement);
            }
            else
            {
                _xmlElement.InsertBefore(item.WrappedElement, ChildElements.ElementAt(index));
            }
        }

        public IEnumerator<Element> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
