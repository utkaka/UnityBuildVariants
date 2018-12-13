using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace BuildVariants.Utils {
    public static class YamlNodeExtensions {

        public static YamlNode GetChildBranch(this YamlNode node, YamlNode child) {
            YamlNode result = null;
            if (node == child) {
                result = node.Clone();
            } else if (node.NodeType == YamlNodeType.Mapping){
                foreach (var mappingChild in ((YamlMappingNode) node).Children) {
                    var childBranch = mappingChild.Value.GetChildBranch(child);
                    if (childBranch == null) continue;
                    result = new YamlMappingNode();
                    ((YamlMappingNode)result).Add(mappingChild.Key.Clone(), childBranch);   
                    break;
                }
            }
            return result;
        }

        public static YamlNode Clone(this YamlNode node) {
            YamlNode result;
            switch (node.NodeType) {
                case YamlNodeType.Scalar:
                    result = new YamlScalarNode(((YamlScalarNode) node).Value);
                    ((YamlScalarNode) result).Style = ((YamlScalarNode) node).Style;
                    break;
                case YamlNodeType.Mapping:
                    result = new YamlMappingNode();
                    ((YamlMappingNode) result).Style = ((YamlMappingNode) node).Style;
                    foreach (var mappingChild in ((YamlMappingNode) node).Children) {
                        ((YamlMappingNode) result).Add(mappingChild.Key.Clone(), mappingChild.Value.Clone());
                    }
                    break;
                case YamlNodeType.Sequence:
                    result = new YamlSequenceNode();
                    ((YamlSequenceNode) result).Style = ((YamlSequenceNode) node).Style;
                    foreach (var child in ((YamlSequenceNode) node).Children) {
                        ((YamlSequenceNode) result).Add(child.Clone());
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return result;   
        }
        
        public static YamlNode Merge(this YamlNode node, YamlNode otherNode) {
            var resultNode = node.Clone();
            if (otherNode == null || node.NodeType != otherNode.NodeType) return resultNode;
            switch (node.NodeType) {
                case YamlNodeType.Mapping:
                    foreach (var mappingChild in ((YamlMappingNode)otherNode).Children) {
                        if (((YamlMappingNode) resultNode).Children.ContainsKey(mappingChild.Key)) {
                            ((YamlMappingNode) resultNode).Children[mappingChild.Key.Clone()] =
                                ((YamlMappingNode) resultNode).Children[mappingChild.Key].Merge(mappingChild.Value);
                        } else {
                            ((YamlMappingNode) resultNode).Children[mappingChild.Key.Clone()] = mappingChild.Value.Clone();   
                        }
                    }
                    break;
                case YamlNodeType.Scalar:
                case YamlNodeType.Sequence:
                    resultNode = otherNode.Clone();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return resultNode;
        }
        
        public static YamlNode Diff(this YamlNode node, YamlNode otherNode) {
            if (otherNode == null || node.NodeType != otherNode.NodeType || node.GetHashCode() == otherNode.GetHashCode()) 
                return null;
            YamlNode result = null;
            switch (node.NodeType) {
                case YamlNodeType.Mapping:
                    foreach (var keyValuePair in GetMappingChildrenDiff(((YamlMappingNode)node).Children,
                        ((YamlMappingNode)otherNode).Children).Concat(
                        GetMappingChildrenDiff(((YamlMappingNode)otherNode).Children.
                        Where(kv => !((YamlMappingNode)node).Children.ContainsKey(kv.Key)),
                        ((YamlMappingNode)node).Children))) {
                        if (result == null) result = new YamlMappingNode();
                        ((YamlMappingNode) result).Children.Add(keyValuePair);
                    }
                    break;
                case YamlNodeType.Scalar:
                case YamlNodeType.Sequence:
                    result = otherNode.Clone();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return result;
        }

        private static IEnumerable<KeyValuePair<YamlNode, YamlNode>> GetMappingChildrenDiff(IEnumerable<KeyValuePair<YamlNode, YamlNode>> mapping1, 
            IDictionary<YamlNode, YamlNode> mapping2) {
            foreach (var mapping1Child in mapping1) {
                YamlNode mapping2Child;
                mapping2.TryGetValue(mapping1Child.Key, out mapping2Child);
                if (mapping2Child != null) {
                    var childDiff = mapping1Child.Value.Diff(mapping2Child);
                    if (childDiff != null) {
                        yield return new KeyValuePair<YamlNode, YamlNode>(mapping1Child.Key.Clone(), childDiff);
                    }
                } else {
                    yield return new KeyValuePair<YamlNode, YamlNode>(mapping1Child.Key.Clone(),
                        mapping1Child.Value.Clone());
                }
            }
        }
    }
}