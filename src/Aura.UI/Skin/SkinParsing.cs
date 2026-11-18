using System;
using System.Collections.Generic;
using Aura.UI.Parser;
using Cosmos.Kernel.System.Graphics;

namespace Aura.UI.Skin
{
    public class SkinParsing
    {
        private Dictionary<string, Bitmap> _bitmaps = new Dictionary<string, Bitmap>();
        private Dictionary<string, Frame> _frames = new Dictionary<string, Frame>();
        private string _skinName;

        /// <summary>
        /// Reads a skin: its bitmaps, then its frames.
        /// </summary>
        /// <param name="skinXmlContent">The skin's XML, without a byte order mark.</param>
        /// <param name="bitmapLoader">
        /// Gives the bytes of a skin bitmap (a BMP file) from the contentPath its XML names
        /// ("SuaveSheet"). A bitmap it throws for is skipped, and so are the frames cut from it.
        /// </param>
        public void loadSkin(string skinXmlContent, Func<string, byte[]> bitmapLoader)
        {
            NanoXMLDocument xml = new NanoXMLDocument(skinXmlContent);
            NanoXMLNode skin = xml.RootNode;

            foreach (NanoXMLNode node in skin.SubNodes)
            {
                if (node.Name.Equals("resources"))
                {
                    loadResources(node, bitmapLoader);
                }
            }

            foreach (NanoXMLNode node in skin.SubNodes)
            {
                if (node.Name.Equals("frames"))
                {
                    loadFrames(node);
                }
            }
        }

        private void loadResources(NanoXMLNode resourcesNode, Func<string, byte[]> bitmapLoader)
        {
            foreach (NanoXMLNode node in resourcesNode.SubNodes)
            {
                if (node.Name.Equals("bitmap"))
                {
                    string bitmapName = Attr(node, "name");
                    _skinName = Attr(node, "contentPath");

                    if (bitmapName == null)
                    {
                        continue;
                    }

                    try
                    {
                        _bitmaps.Add(bitmapName, new Bitmap(bitmapLoader(_skinName)));
                    }
                    catch (Exception)
                    {
                        // The frames cut from this bitmap are skipped by loadFrames.
                    }
                }
            }
        }

        private void loadFrames(NanoXMLNode framesNode)
        {
            foreach (NanoXMLNode node in framesNode.SubNodes)
            {
                if (node.Name.Equals("frame"))
                {
                    string name = Attr(node, "name");

                    if (name != null && (name.StartsWith("window") || name.StartsWith("button") ||
                        name.StartsWith("slider") || name.StartsWith("rail") ||
                        name.StartsWith("cursor") || name.StartsWith("check") || name.StartsWith("input") ||
                        name.StartsWith("list")))
                    {
                        Frame.Region[] regions = RegionListBuilder.Build(node, _bitmaps);
                        Frame.Text[] texts = null;

                        _frames.Add(name, new Frame(regions, texts));
                    }
                }
            }
        }

        /// <summary>Value of an optional XML attribute, or null if the node has no such attribute.</summary>
        private static string Attr(NanoXMLNode node, string name)
        {
            // GEN3-GAP(null-deref): GetAttribute returns null for a missing attribute; dereferencing it
            // is a fatal #PF on gen3.
            NanoXMLAttribute attribute = node.GetAttribute(name);
            return attribute == null ? null : attribute.Value;
        }

        public Frame GetFrame(string name)
        {
            return _frames[name];
        }

        public string GetSkinName()
        {
            return _skinName;
        }
    }
}
