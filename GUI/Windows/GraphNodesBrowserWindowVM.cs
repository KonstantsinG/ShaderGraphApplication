﻿using GUI.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using GUI.Representation.GraphNodes.Wrappers;
using GUI.Utilities.DataBindings;
using ShaderGraph.Serializers;
using ShaderGraph.GraphNodesImplementation.Types;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace GUI.Windows
{
    public class GraphNodesBrowserWindowVM : VmBase
    {
        private readonly List<TreeViewerNodeInfo>? _sourceItems;

        private ObservableCollection<TreeViewerNodeInfo> _treeItems = [];
        public ObservableCollection<TreeViewerNodeInfo> TreeItems
        {
            get => _treeItems;
            set
            {
                _treeItems = value;
                OnPropertyChanged(nameof(TreeItems));
            }
        }

        private string _selectedDescription = string.Empty;
        public string SelectedDescription
        {
            get => _selectedDescription;
            set
            {
                _selectedDescription = value;
                OnPropertyChanged(nameof(SelectedDescription));
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        private TreeViewerNodeInfo? _selectedItem;

        public delegate void TreeViewerCallback(uint? nodeId);
        public event TreeViewerCallback ItemCreated = delegate { };


        public GraphNodesBrowserWindowVM()
        {
            _sourceItems = WrapTreeViewerItems(GraphNodesTypesSerializer.DeserializeAll("ru-RU"));
            TreeItems = DeepCopyTreeViewerNodeInfos(_sourceItems!);
        }

        public void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (((TreeView)sender).SelectedItem is TreeViewerNodeInfo item)
            {
                _selectedItem = item;
                SelectedDescription = item.Description;
            }
            else
            {
                _selectedItem = null;
                SelectedDescription = string.Empty;
            }
        }

        public void CrossRect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TreeItems = DeepCopyTreeViewerNodeInfos(_sourceItems!);
            SearchText = string.Empty;
        }

        public void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TreeViewerNodeInfo? item;
            TreeViewerNodeInfo? subItem;
            TreeViewerNodeInfo? subSubItem;
            TreeItems = DeepCopyTreeViewerNodeInfos(_sourceItems!);
            if (SearchText == string.Empty) return;

            for (int i = 0; i < TreeItems.Count; i++)
            {
                item = TreeItems[i];
                item.IsExpanded = true;

                for (int j = 0; j < item.Children.Count; j++)
                {
                    subItem = item.Children[j];
                    subItem.IsExpanded = true;

                    for (int k = 0; k < subItem.Children.Count; k++)
                    {
                        subSubItem = subItem.Children[k];
                        subSubItem.IsExpanded = true;

                        if (!subSubItem.Synonyms.Any(s => s.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            subItem.Children.RemoveAt(k);
                            k--;
                        }
                    }

                    if (!subItem.Synonyms.Any(s => s.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        if (subItem.Children.Count > 0)
                        {
                            foreach (var child in subItem.Children)
                                item.Children.Add(child);
                        }

                        item.Children.RemoveAt(j);
                        j--;
                    }
                }

                if (!item.Synonyms.Any(s => s.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (item.Children.Count > 0)
                    {
                        foreach (var child in item.Children)
                            TreeItems.Add(child);
                    }

                    TreeItems.RemoveAt(i);
                    i--;
                }
            }
        }

        public void SearchBoxKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;

            if (e.Key == Key.Enter)
            {
                _selectedItem ??= TreeItems.FirstOrDefault();
                ItemCreated.Invoke(_selectedItem?.Id);
            }
        }

        public void TreeViewItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;
            if (((TreeViewItem)sender).Header != _selectedItem) return;

            if (e.Key == Key.Enter)
                ItemCreated.Invoke(_selectedItem?.Id);
        }

        public void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ItemCreated.Invoke(_selectedItem?.Id);
        }

        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ItemCreated.Invoke(null);
        }

        private static List<TreeViewerNodeInfo> WrapTreeViewerItems(List<GraphNodeType> items)
        {
            List<TreeViewerNodeInfo> wrappedOnes = [];
            TreeViewerNodeInfo? infoOne;
            TreeViewerNodeInfo? infoTwo;
            TreeViewerNodeInfo? infoThree;

            foreach (var type in items)
            {
                infoOne = new(type);

                foreach (var op in type.OperationsTypes)
                {
                    infoTwo = new(op);

                    foreach (var subop in op.OperationsSubTypes)
                    {
                        infoThree = new(subop);
                        infoTwo.Children.Add(infoThree!);
                    }

                    infoOne.Children.Add(infoTwo!);
                }

                wrappedOnes.Add(infoOne);
            }

            return wrappedOnes;
        }

        private static ObservableCollection<TreeViewerNodeInfo> DeepCopyTreeViewerNodeInfos(List<TreeViewerNodeInfo> nodes)
        {
            ObservableCollection<TreeViewerNodeInfo> copy = [];
            TreeViewerNodeInfo? infoOne;
            TreeViewerNodeInfo? infoTwo;
            TreeViewerNodeInfo? infoThree;

            foreach (var type in nodes)
            {
                infoOne = new(type);

                foreach (var op in type.Children)
                {
                    infoTwo = new(op);

                    foreach (var subop in op.Children)
                    {
                        infoThree = new(subop);
                        infoTwo.Children.Add(infoThree!);
                    }

                    infoOne.Children.Add(infoTwo!);
                }

                copy.Add(infoOne);
            }

            return copy;
        }
    }
}
