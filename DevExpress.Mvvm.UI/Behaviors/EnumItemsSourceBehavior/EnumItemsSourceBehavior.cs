using DevExpress.Internal;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Interactivity.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevExpress.Mvvm.UI {
    [TargetType(typeof(ItemsControl))]
    public class EnumItemsSourceBehavior : Behavior<FrameworkElement> {
        public EnumItemsSourceBehavior() {
            InteractionHelper.SetBehaviorInDesignMode(this, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            GetDefaultDataTemplate();
        }
        #region Dependency Properties
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(EnumItemsSourceBehavior),
            new FrameworkPropertyMetadata(null, (d, e) => ((EnumItemsSourceBehavior)d).OnItemTemplateChanged(e)));
        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register("EnumType", typeof(Type), typeof(EnumItemsSourceBehavior),
            new FrameworkPropertyMetadata(null, (d, e) => ((EnumItemsSourceBehavior)d).OnEnumTypeChanged(e)));
        public static readonly DependencyProperty UseNumericEnumValueProperty =
            DependencyProperty.Register("UseNumericEnumValue", typeof(bool), typeof(EnumItemsSourceBehavior),
            new FrameworkPropertyMetadata(false, (d, e) => ((EnumItemsSourceBehavior)d).OnUseNumericEnumValueChanged(e)));
        public static readonly DependencyProperty SplitNamesProperty =
            DependencyProperty.Register("SplitNames", typeof(bool), typeof(EnumItemsSourceBehavior),
            new FrameworkPropertyMetadata(true, (d, e) => ((EnumItemsSourceBehavior)d).OnSplitNamesChanged(e)));
        public static readonly DependencyProperty NameConverterProperty =
            DependencyProperty.Register("NameConverter", typeof(IValueConverter), typeof(EnumItemsSourceBehavior),
            new FrameworkPropertyMetadata(null, (d, e) => ((EnumItemsSourceBehavior)d).OnNameConverterChanged(e)));
        public static readonly DependencyProperty SortModeProperty =
            DependencyProperty.Register("SortMode", typeof(EnumMembersSortMode), typeof(EnumItemsSourceBehavior),
            new FrameworkPropertyMetadata(EnumMembersSortMode.Default, (d, e) => ((EnumItemsSourceBehavior)d).OnSortModeChanged(e)));
        #endregion

        public DataTemplate ItemTemplate {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public Type EnumType {
            get { return (Type)GetValue(EnumTypeProperty); }
            set { SetValue(EnumTypeProperty, value); }
        }
        public bool UseNumericEnumValue {
            get { return (bool)GetValue(UseNumericEnumValueProperty); }
            set { SetValue(UseNumericEnumValueProperty, value); }
        }
        public bool SplitNames {
            get { return (bool)GetValue(SplitNamesProperty); }
            set { SetValue(SplitNamesProperty, value); }
        }
        public IValueConverter NameConverter {
            get { return (IValueConverter)GetValue(NameConverterProperty); }
            set { SetValue(NameConverterProperty, value); }
        }
        public EnumMembersSortMode SortMode {
            get { return (EnumMembersSortMode)GetValue(SortModeProperty); }
            set { SetValue(SortModeProperty, value); }
        }

        void OnItemTemplateChanged(DependencyPropertyChangedEventArgs e) {
            ChangeItemTemplate();
        }
        void OnEnumTypeChanged(DependencyPropertyChangedEventArgs e) {
            ChangeAssociatedObjectItemsSource();
        }
        void OnUseNumericEnumValueChanged(DependencyPropertyChangedEventArgs e) {
            ChangeAssociatedObjectItemsSource();
        }
        void OnSplitNamesChanged(DependencyPropertyChangedEventArgs e) {
            ChangeAssociatedObjectItemsSource();
        }
        void OnNameConverterChanged(DependencyPropertyChangedEventArgs e) {
            ChangeAssociatedObjectItemsSource();
        }
        void OnSortModeChanged(DependencyPropertyChangedEventArgs e) {
            ChangeAssociatedObjectItemsSource();
        }
        void ChangeAssociatedObjectItemsSource() {
            if(this.AssociatedObject != null) {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.AssociatedObject).Find("ItemsSource", true);
                if(EnumType == null)
                    throw new Exception("EnumType required");
                else {
                    if(descriptor == null)
                        throw new Exception("ItemsSource dependency property required");
                    else
                        descriptor.SetValue(this.AssociatedObject, EnumSourceHelper.GetEnumSource(EnumType, UseNumericEnumValue, NameConverter, SplitNames, SortMode));
                }
            }
        }
        void ChangeItemTemplate() {
            ItemsControl itemsControl = this.AssociatedObject as ItemsControl;
            if(itemsControl != null)
                itemsControl.ItemTemplate = ItemTemplate;
        }

 internal DataTemplate defaultDataTemplate;

        void GetDefaultDataTemplate() {
            ResourceDictionary resourceDictionary = new ResourceDictionary() {
                Source = new Uri(string.Format("pack://application:,,,/{0};component/Behaviors/EnumItemsSourceBehavior/EnumItemsSourceDefaultTemplate.xaml",
                    AssemblyInfo.SRAssemblyXpfMvvmUIFree), UriKind.Absolute)
            };
            defaultDataTemplate = (DataTemplate)resourceDictionary["ItemsSourceDefaultTemplate"];
        }

        protected override void OnAttached() {
            base.OnAttached();
            if(ItemTemplate == null)
                this.SetCurrentValue(ItemTemplateProperty, defaultDataTemplate);
            else
                ChangeItemTemplate();
            ChangeAssociatedObjectItemsSource();
        }
        protected override void OnDetaching() {
            base.OnDetaching();
        }
    }

    public class EnumMemberInfoPresenter : Control {
        public EnumMemberInfo EnumMemberInfo {
            get { return (EnumMemberInfo)GetValue(EnumMemberInfoProperty); }
            set { SetValue(EnumMemberInfoProperty, value); }
        }
        public static readonly DependencyProperty EnumMemberInfoProperty =
            DependencyProperty.Register("EnumMemberInfo", typeof(EnumMemberInfo), typeof(EnumMemberInfoPresenter), new PropertyMetadata(null));

        static EnumMemberInfoPresenter() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumMemberInfoPresenter), new FrameworkPropertyMetadata(typeof(EnumMemberInfoPresenter)));
        }
    }
}