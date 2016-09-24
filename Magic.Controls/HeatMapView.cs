using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Magic.Controls
{
    public class HeatMapView : Selector
    {
        private readonly Dictionary<object, Dummy> _dummies = new Dictionary<object, Dummy>();

        private double _totalValue;

        static HeatMapView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (HeatMapView),
                new FrameworkPropertyMetadata(typeof (HeatMapView)));
        }

        public HeatMapView()
        {
            var notify = Items as INotifyCollectionChanged;
            notify.CollectionChanged += NotifyOnCollectionChanged;
        }

        private void NotifyOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _dummies[args.NewItems[0]] = new Dummy(args.NewItems[0], ValueMembername);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _dummies.Clear();
                    foreach (var item in Items)
                    {
                        _dummies[item] = new Dummy(item, ValueMembername);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _totalValue = _dummies.Values.Sum(d => (double) d.GetValue(Dummy.ValueProperty));
        }

        #region Overriders

        /// <summary>
        ///     Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <returns>
        ///     true if the item is (or is eligible to be) its own container; otherwise, false.
        /// </returns>
        /// <param name="item">The item to check.</param>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            // Even wrap other ContentControls
            return false;
        }

        /// <summary>
        ///     Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>
        ///     The element that is used to display the given item.
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            var item = new HeatMapViewItem
            {
                Focusable = true
            };
            return item;
        }

        /// <summary>
        ///     Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">The element that is used to display the specified item.</param>
        /// <param name="item">The specified item to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
        }

        #endregion Overrides 

        #region dependent Properties

        public string ValueMembername
        {
            get { return (string) GetValue(ValueMembernameProperty); }
            set { SetValue(ValueMembernameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueMembername.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueMembernameProperty =
            DependencyProperty.Register("ValueMembername", typeof (string), typeof (HeatMapView),
                new PropertyMetadata(null));

        #endregion
    }


    internal class Dummy : DependencyObject
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof (double),
            typeof (Dummy), new UIPropertyMetadata(null));


        public Dummy(object item, string valueMembername)
        {
            var b = new Binding(valueMembername)
            {
                Mode = BindingMode.OneWay
            };
            b.Source = item;
            BindingOperations.SetBinding(this, ValueProperty, b);
        }
    }
}